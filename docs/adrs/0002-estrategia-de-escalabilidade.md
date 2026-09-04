# ADR-0002: Escalar em duas camadas, com HPA por utilização e Cluster Autoscaler por pod pendente

| | |
|---|---|
| **Status** | Aceita |
| **Data** | 2026-09-04 |
| **Decisores** | Grupo 160 |
| **Issue** | [#63](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/63) |

## Contexto

A Fase 3 exige um cluster Kubernetes gerenciado "com escalabilidade". O projeto já tinha um HPA herdado do ambiente `kind` — 2 a 10 réplicas, CPU 70% e memória 75% — e um node group com `min_size = 2` e `max_size = 4`.

Isso parecia resolvido e não estava. Um managed node group **não escala sozinho**: `min_size` e `max_size` são limites, não um motor. Sem alguém movendo o `desired_size`, o HPA escala pod até esgotar o espaço dos nodes existentes e para — os pods excedentes ficam `Pending` indefinidamente, sem erro visível no Deployment.

As forças em jogo:

- **Crédito finito.** O Learner Lab dá cerca de US$ 100, e cada node `t3.medium` cobra enquanto existir. Escalabilidade sem teto é uma forma cara de aprender.
- **Sem IAM próprio.** O lab não permite criar roles, o que elimina IRSA e força soluções que usem a role da instância.
- **Demonstração de 15 minutos.** Os tempos padrão de scale down (10 min) não cabem na gravação.

## Decisão

Escalar em **duas camadas independentes**, cada uma com seu gatilho:

| Camada | Componente | Gatilho | Limites |
|---|---|---|---|
| Pod | HPA | utilização de CPU e memória acima do alvo | 2 a 10 réplicas |
| Node | Cluster Autoscaler | pod `Pending` que não cabe em node nenhum | 2 a 4 nodes |

A ordem importa e não é intercambiável: o HPA age primeiro, e o autoscaler só existe para o caso em que o HPA pediu mais do que cabe.

### Requests e limits

Definidos em `k8s/api/deployment.yaml`:

| | CPU | Memória |
|---|---|---|
| `requests` | 200m | 256Mi |
| `limits` | 500m | 512Mi |

**`requests` não é opcional aqui.** O HPA calcula utilização como percentual do *request*; sem ele o alvo de 70% não tem denominador e o HPA fica em `<unknown>`. É também o que o scheduler usa para decidir se o pod cabe num node — ou seja, é o mesmo número que faz o autoscaler concluir que precisa de node novo.

Os valores vêm da medição, não de chute. Em repouso cada pod consome ~11m de CPU e ~90Mi; sob carga, ~85m. O request de 200m dá folga de arranque sem reservar node à toa, e o limit de 500m permite absorver pico antes de o HPA reagir. A memória fica em 256Mi/512Mi porque o consumo é estável em ~90Mi e o risco real é `OOMKilled` durante a migration do startup, não em regime.

### Limiares do HPA

CPU 70% e memória 75%, mantidos do ambiente anterior após validação.

Setenta por cento deixa 30% de margem para o intervalo entre a métrica subir e o pod novo estar pronto. Mais alto que isso, a API degrada antes de a réplica entrar; mais baixo, o cluster escala por ruído.

A memória em 75% é deliberadamente *menos* sensível: o consumo de memória da API é estável, e memória alta raramente se resolve com mais réplicas — quando ela sobe, é vazamento ou payload anômalo, e escalar só espalha o problema. O limiar existe como rede de segurança, não como mecanismo primário.

O `behavior` completa o desenho: `scaleUp` com `stabilizationWindowSeconds: 0`, para subir imediatamente; `scaleDown` com 300s, para não derrubar réplica a cada vale de tráfego.

### Parâmetros do Cluster Autoscaler

Em `k8s/cluster-autoscaler/deployment.yaml`, no repositório `tech-challenge-infra-k8s`:

- `--scale-down-unneeded-time=5m` e `--scale-down-delay-after-add=5m`, contra os 10 minutos padrão — a demonstração tem 15 minutos no total;
- `--skip-nodes-with-system-pods=false`, sem o qual nenhum node seria removível: todos têm pod de `kube-system`;
- `--expander=least-waste`;
- descoberta por tag no Auto Scaling group, com o **nome do cluster** na tag. Os três ambientes dividem a mesma conta do lab, e sem isso o autoscaler de `dev` escalaria os nodes de `hom` e `prod`.

## Alternativas consideradas

### Karpenter no lugar do Cluster Autoscaler

Provisiona nodes por forma do pod, sem depender de ASG, e costuma ser mais rápido e mais econômico.

Descartado por duas razões: exige criar roles e um perfil de instância próprios — impossível no Learner Lab — e substituiria uma peça que a banca reconhece por uma que exigiria justificar sozinha. O ganho não paga o risco no prazo da fase.

### Só HPA, sem autoscaling de nodes

Era o estado anterior, e é o que a issue #60 apontou como critério não atendido.

Funciona enquanto a carga couber nos nodes existentes — e o teste de carga desta ADR mostra que couberam 8 réplicas em 2 nodes. O problema é o silêncio: quando não cabe, o pod fica `Pending` e o Deployment continua reportando o número desejado de réplicas. Falha sem sinal é pior que falha com erro.

### Cluster Autoscaler com IRSA

O caminho correto: uma role dedicada ao ServiceAccount, com apenas as permissões de `autoscaling:` que ele usa.

Impossível aqui — o lab nega criação de roles. Ficou a role da instância, a `LabRole`, que é larga demais. Registrado como dívida, não como desenho.

## Evidência: teste de carga em `dev`, 04/09/2026

Cluster `tc-grupo160-dev`, dois `t3.medium`, 1930m de CPU alocável por node.

### Camada 1 — HPA sob carga HTTP

Oito geradores em loop contra `GET /health`:

```
+0s    cpu: 4%/70%    réplicas: 2
+30s   cpu: 154%/70%  réplicas: 2   -> acima do alvo
       SuccessfulRescale  New size: 5  reason: cpu resource utilization above target
       SuccessfulRescale  New size: 6
       SuccessfulRescale  New size: 8
+300s  cpu: 43%/70%   réplicas: 8   -> estabilizou
       SuccessfulRescale  New size: 6  reason: All metrics below target
```

Subiu de 2 para 8 réplicas e a utilização caiu de 154% para 43%. **Os nodes ficaram em 2 o tempo todo**: 8 réplicas × 200m = 1600m cabiam folgadamente. O autoscaler não fez nada — corretamente.

### Camada 2 — Cluster Autoscaler sob pressão de agendamento

Seis pods de 900m, além do que dois nodes comportam:

```
18:55:54  Estimated 2 nodes needed
18:55:54  Final scale-up plan: [2->4 (max: 4)]
18:56:35  node registrado e Ready              <- 45s da decisão
18:56:04  pod didn't trigger scale-up: 1 max node group size reached
```

Depois, removida a pressão:

```
~19:07    scaleDown: CandidatesPresent          <- já na primeira amostra
19:12:38  ScaleDownEmpty: removing empty node   <- 313s depois, bate com --scale-down-unneeded-time=5m
19:12:44  empty node removed  (x2)
~19:15    dois nodes no cluster, ASG em desired=2
```

Três coisas que este teste prova e que a configuração sozinha não provava:

1. **45 segundos** da decisão ao node pronto — muito abaixo dos ~2 minutos que estimávamos.
2. O autoscaler **para no teto**: dois pods continuaram `Pending` com `max node group size reached`, em vez de estourar o `max_size`. É isso que impede uma carga anômala de consumir o crédito da conta.
3. O scale down removeu **apenas os nodes vazios**, sem tocar nos que rodavam a API.

## Consequências

### Positivas

- A capacidade acompanha a demanda nas duas dimensões, e o caminho `pod Pending -> node novo -> pod agendado` está medido, não suposto.
- O teto de 4 nodes transforma um pico anômalo em pods `Pending` — degradação visível — em vez de uma fatura silenciosa.
- Requests e limits explícitos deixam o HPA e o scheduler decidindo sobre o mesmo número.

### Negativas

- **Dois mecanismos, dois lugares para errar.** Um HPA que escala para 10 réplicas contra um `max_size` que só comporta 8 produz pods `Pending` permanentes. Os limites das duas camadas precisam ser revistos juntos.
- **O autoscaler usa a `LabRole`**, que é muito mais ampla do que ele precisa. Fora do lab, isso não passaria em revisão de segurança.
- **`hostNetwork: true` no autoscaler.** Necessário porque os nodes sobem com `HttpPutResponseHopLimit = 1` e um pod na rede de pods não alcança o IMDS. É um contorno; a solução estrutural é um launch template com hop limit 2.
- **Janelas de scale down encurtadas para 5 minutos** servem à demonstração, não à produção. Em produção, o valor padrão de 10 minutos protegeria melhor contra oscilação.

### Riscos e mitigação

| Risco | Mitigação |
|---|---|
| HPA pede mais réplicas do que 4 nodes comportam | Medido: 8 réplicas cabem em 2 nodes. O teto de 10 réplicas exige ~3 nodes, dentro do `max_size` |
| `max_size = 4` atingido sob carga real | O autoscaler emite `NotTriggerScaleUp` com `max node group size reached`; é evento observável, e a #73 vai transformá-lo em alerta |
| Node novo demora e a requisição cai | `scaleUp` do HPA com janela zero absorve o pico nas réplicas antes de precisar de node |
| Perda do contorno de `hostNetwork` numa mudança futura | Documentado no manifest com o erro exato que ele evita: `NoCredentialProviders` |

## Referências

- [RFC-0001: escolha da nuvem](../rfcs/0001-escolha-da-nuvem.md) — orçamento e disciplina de destruir o ambiente
- [Issue #60](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/60) — cluster gerenciado com node group escalável
- `k8s/api/hpa.yaml` e `k8s/cluster-autoscaler/` em [tech-challenge-infra-k8s](https://github.com/tech-challenge-grupo-160/tech-challenge-infra-k8s)
