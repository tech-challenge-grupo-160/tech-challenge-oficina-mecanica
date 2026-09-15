# Rollback do banco gerenciado

Como voltar atrás depois de um deploy que estragou dados ou schema no RDS.
Critério de aceite da [#62](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/62).

> **Leia a primeira seção antes de qualquer coisa.** Rollback de aplicação e
> rollback de banco são operações diferentes, e a segunda não acontece sozinha
> quando você faz a primeira.

## O que o rollback da aplicação **não** desfaz

O deploy tem rollback documentado — `kubectl rollout undo`, no resumo de cada
execução do workflow. Ele troca a imagem do container e nada mais.

As migrations rodam no **startup da API**, em
[`HostExtensions.cs`](../src/Infrastructure/Extensions/HostExtensions.cs), por
`context.Database.MigrateAsync()`. Esse método só aplica migration pendente:
ele **nunca** reverte. Voltar a imagem para um commit anterior deixa o banco com
o schema novo e o código velho — e o EF Core não reclama disso no startup, então
o pod sobe saudável e quebra depois, no primeiro acesso à coluna que mudou.

| Camada | Rollback | Onde |
|---|---|---|
| Imagem da API | `kubectl rollout undo` | resumo do workflow Deploy EKS |
| Schema | migration de down, explícita | seção abaixo |
| Dados | snapshot ou PITR do RDS | seção abaixo |

## Antes de uma operação arriscada: snapshot manual

A instância é criada com `skip_final_snapshot = true` e
`deletion_protection = false`
([`banco.tf`](https://github.com/tech-challenge-grupo-160/tech-challenge-infra-database/blob/develop/banco.tf)).
Isso é deliberado — o ambiente do lab é recriado a cada sessão e um snapshot
final só deixaria custo para trás —, mas tem uma consequência dura:

**`terraform destroy` apaga a instância sem deixar cópia.** O
`scripts/derruba-tudo.sh` faz exatamente isso.

Antes de migration destrutiva, carga em massa ou qualquer teste que mexa em
dado que você queira de volta:

```bash
aws rds create-db-snapshot --db-instance-identifier tc-grupo160-dev --db-snapshot-identifier tc-grupo160-dev-antes-da-migration
```

Troque o sufixo do ambiente (`dev`, `hom`, `prod`) nos dois identificadores.
O snapshot sobrevive ao `destroy` da instância e continua cobrando
armazenamento — apague quando não precisar mais.

## Rollback de schema

Cada migration do EF Core traz o método `Down`. Para voltar uma:

```bash
dotnet ef database update 20260615234350_ReplaceMd5PasswordHashWithBCrypt --project src/Infrastructure --startup-project src/API
```

O argumento é a migration que deve passar a ser a **última aplicada**, não a que
você quer desfazer. Para listar o que está no banco:

```bash
dotnet ef migrations list --project src/Infrastructure --startup-project src/API
```

Três coisas que atrapalham na prática:

1. **O RDS não é alcançável da sua máquina.** Ele vive em subnet privada, sem
   rota para a internet. As opções são rodar o comando de dentro do cluster, ou
   ligar `acesso_externo_dev` no inventory de `dev` — que move a instância para
   as subnets públicas e força uma substituição da instância. A segunda é
   aceitável em `dev` e não deve ser usada em `hom` nem em `prod`.
2. **`Down` nem sempre restaura dado.** Uma migration que apagou coluna volta a
   criá-la vazia. Perda de dado por migration se resolve por snapshot, não por
   `Down`.
3. **O próximo deploy reaplica.** Enquanto a migration continuar no código, o
   startup da API a aplica de novo. Reverter no banco sem reverter no código é
   um estado que dura até o próximo pod subir.

## Rollback de dados

### Restaurar de um snapshot

O RDS não restaura por cima de uma instância existente: ele cria uma nova.

```bash
aws rds restore-db-instance-from-db-snapshot --db-instance-identifier tc-grupo160-dev-restaurado --db-snapshot-identifier tc-grupo160-dev-antes-da-migration --db-subnet-group-name tc-grupo160-dev --multi-az
```

Depois é preciso apontar a aplicação para o novo endereço. Ele está no
`connectionString` do segredo `tc-grupo160/<amb>/banco`, no Secrets Manager, que
o deploy monta como Secret do Kubernetes. Atualize o segredo e force o rollout:

```bash
kubectl rollout restart deployment/oficina-mecanica-api -n oficina-mecanica
```

O restart é obrigatório: a API lê o segredo **no startup**, como registra a
RFC-0002. Trocar o valor sem reiniciar não muda nada.

> A instância restaurada fica fora do state do Terraform. Ou você a promove
> renomeando os identificadores e importa (`terraform import`), ou trata como
> temporária, extrai o que precisa e destrói. Deixar as duas de pé cobra dobrado.

### Point-in-time recovery

`backup_retention_period = 7`, então dá para voltar a instância a qualquer
instante dos últimos **7 dias**, com granularidade de cerca de 5 minutos — sem
snapshot manual nenhum. Serve para o caso em que o estrago só foi percebido
horas depois.

```bash
aws rds restore-db-instance-to-point-in-time --source-db-instance-identifier tc-grupo160-dev --target-db-instance-identifier tc-grupo160-dev-pitr --restore-time 2026-09-03T14:30:00Z
```

Vale o mesmo aviso da seção anterior: nasce uma instância nova, fora do
Terraform, e a aplicação só a enxerga depois de o segredo mudar e os pods
reiniciarem.

## Rollback total: voltar ao PostgreSQL no cluster

Caminho de emergência, para o caso de o RDS ficar indisponível durante uma
demonstração. Os manifests do Postgres in-cluster continuam versionados em
`k8s/postgres/` no `tech-challenge-infra-k8s` — fora do overlay `k8s/nuvem`,
mas presentes.

```bash
kubectl apply -k k8s
```

Custa a perda de tudo o que estava no RDS: o banco do cluster sobe vazio, e o
startup da API o povoa com migrations e seed. É rollback de **disponibilidade**,
não de dado. Serve para a aplicação voltar a responder; não serve para recuperar
o que foi perdido.

Depois de usar, desfaça — dois bancos no ar significa que metade das requisições
grava no lugar errado:

```bash
kubectl delete -k k8s
```

## O que este projeto tem hoje, para constar

O schema nasceu no RDS pelo caminho normal — `MigrateAsync` no startup do
primeiro pod — e os dados vieram do **seed** da própria aplicação. Nunca houve
carga de dados de produção vinda do PostgreSQL in-cluster: aquele ambiente era
descartável e também era povoado pelo mesmo seed.

Ou seja: o rollback de dado descrito aqui está documentado e é executável, mas
não foi exercitado contra um volume real. Fica registrado para não parecer mais
testado do que é.
