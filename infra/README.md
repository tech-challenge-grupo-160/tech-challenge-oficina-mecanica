# Mensageria do OS Service

Este diretorio provisiona o topico SNS publicado pelo OS Service, as filas SQS
que consomem eventos do Billing e da Execucao, suas DLQs e a configuracao no
AWS Secrets Manager.

## Recursos

- Topico `tc-grupo160-<ambiente>-os-events`, criptografado com a chave gerenciada
  pela AWS para SNS.
- Filas de eventos `...-os-billing-eventos` e `...-os-execucao-eventos`, com
  filtros pelo campo `type` do envelope.
- DLQ para cada fila; filas e DLQs usam criptografia gerenciada pelo SQS,
  retencao de 4 e 14 dias e redrive apos 5 recebimentos.
- Segredo `tc-grupo160/<ambiente>/os/messaging` com nomes, URLs e ARNs do topico,
  filas e filas de comando de destino.

O workflow de deploy do OS Service sincroniza o segredo para `api-secret` e
expõe os URLs/ARNs como variáveis `Messaging__*` do processo.

O mapa completo de eventos e comandos esta em
[`../docs/mensageria/catalogo.md`](../docs/mensageria/catalogo.md).

## Aplicacao

O backend usa o bucket compartilhado do Terraform. Configure a conta e a
sessao AWS antes de executar:

```powershell
terraform init -reconfigure `
  -backend-config="bucket=tc-grupo160-tfstate-<id-da-conta>" `
  -backend-config="key=dev/messaging-os.tfstate" `
  -backend-config="region=us-east-1" `
  -backend-config="dynamodb_table=tc-grupo160-tflock"
terraform plan -var="environment=dev"
```

O primeiro provisionamento precisa de duas fases para evitar a dependencia
circular entre os topicos (criados pelos produtores) e as assinaturas (criadas
pelos consumidores):

1. Em cada servico, aplique somente `aws_sns_topic.events` para criar os tres
   topicos produtores.
2. Depois que os tres topicos existirem, aplique o Terraform completo em cada
   servico para criar filas, assinaturas, filtros e segredos.

Use a mesma regiao, conta, prefixo e ambiente nos tres repositorios. As filas
de comando sao criadas pelo servico consumidor; o OS Service le seus destinos
no segredo `os/messaging`. O passo a passo para AWS Academy, verificacao de
assinaturas/DLQs e subida do ambiente completo está no
[catálogo de mensageria](../docs/mensageria/catalogo.md#teste-direto-na-aws-academy-learner-lab).

O LocalStack mencionado na ADR-0005 ainda não está configurado nos
`docker-compose.yml` disponíveis. A validação registrada para `dev` em
08/11/2026 encontrou as seis filas (incluindo as filas de comando) e as seis
DLQs, com as quatro assinaturas SNS de eventos esperadas. Os atributos de
filtros, redrive e criptografia devem ser conferidos com os comandos do
catálogo. Isso não substitui o spike F4-05 de acesso a partir de pod no EKS.

O workflow `terraform-messaging.yml` valida/planeja PRs e aplica em push nas
branches de ambiente. Sem push, ele não é disparado por este commit.
