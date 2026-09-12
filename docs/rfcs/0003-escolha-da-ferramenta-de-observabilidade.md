# RFC-0003: Escolha da ferramenta de observabilidade

| | |
|---|---|
| **Status** | Em revisão |
| **Autor** | Grupo 160 |
| **Data** | 2026-09-10 |
| **Issue** | [#66](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/66) |
| **Prazo para comentários** | 2026-09-17 |

## Resumo

Esta RFC compara Datadog e New Relic para a observabilidade da Fase 3, cobrindo
latência, recursos do Kubernetes, health checks, alertas, logs JSON com
correlação e dashboards.

As duas ferramentas atendem tecnicamente aos requisitos. A decisão proposta é
adotar o **Datadog**, porque ele já está integrado ao projeto: o Agent roda no
EKS por Helm, as duas Lambdas .NET usam a Datadog Lambda Extension e a camada de
tracing, e a API já produz logs estruturados com identificadores de requisição.
Escolher New Relic agora adicionaria uma segunda integração e não reduziria
nenhum requisito funcional.

O custo de observabilidade durante o período do desafio pode ser zero para as
duas opções quando coberto pelo trial ou pelo plano gratuito, desde que o
volume permaneça dentro dos limites vigentes. Os custos AWS do EKS, NAT, RDS,
API Gateway e Lambda são independentes desta decisão e não estão incluídos.

## Motivação

A Fase 3 exige evidência operacional da aplicação distribuída, não apenas
logs locais. A solução precisa permitir:

- medir latência de entrada, API, Lambda e dependências;
- observar workloads, nodes, pods e autoscaling do EKS;
- acompanhar liveness/readiness e disponibilidade;
- criar alertas acionáveis;
- consultar logs JSON preservando correlação entre requisição, trace e serviço;
- apresentar dashboards que possam ser demonstrados no ambiente AWS.

Sem uma ferramenta única para correlacionar esses sinais, a investigação de um
erro atravessando API Gateway, Lambda, VPC Link, EKS e RDS dependeria de
consultas manuais em vários serviços.

## Critérios de avaliação

As notas usam a escala de 1 a 5:

- **5**: atende diretamente, com integração já validada;
- **4**: atende com configuração pequena;
- **3**: atende, mas exige integração ou operação adicional;
- **2**: atende parcialmente ou depende de solução externa;
- **1**: não atende ao requisito.

O esforço considera o estado atual do projeto, que usa API .NET 10, duas
Lambdas .NET, EKS gerenciado, manifests Kubernetes, API Gateway e RDS.

## Alternativas avaliadas

### Datadog

No EKS, o Agent é instalado pelo chart oficial via `helm_release`, como
DaemonSet. Ele coleta métricas de nodes e containers, logs dos containers e
dados do estado do Kubernetes.

Nas Lambdas, o `datadog-ci` adiciona a Datadog Lambda Extension e as camadas
`Datadog-Extension` e `dd-trace-dotnet`. A API key fica no Secrets Manager e a
instrumentação usa `DD_API_KEY_SECRET_ARN`, sem colocar a chave em texto puro
na função.

Na API .NET, os logs continuam estruturados e a instrumentação adiciona
traces, duração, erros e correlação com o serviço e o ambiente.

### New Relic

No EKS, o caminho equivalente seria o `nri-bundle`, com o New Relic
Infrastructure agent, integração Kubernetes, kube-state-metrics e configuração
de coleta de logs.

Nas Lambdas .NET, seria necessário instalar as camadas oficiais do New Relic,
configurar a chave/licença por Secrets Manager e revisar as variáveis de
runtime e os parâmetros de tracing. A aplicação e os manifests precisariam de
uma segunda implementação de exportação e validação.

O New Relic também oferece APM, alertas, dashboards e correlação de logs, mas
esses recursos não estão integrados ao projeto no momento.

## Comparativo por critério

| Critério | Datadog | New Relic | Observação |
|---|---:|---:|---|
| Latência da API .NET | 5 | 5 | APM e métricas de duração atendem às duas opções |
| Latência do API Gateway e VPC Link | 5 | 4 | Datadog centraliza os sinais já usados no deploy; New Relic exigiria integração AWS adicional |
| Latência e erros das Lambdas .NET | 5 | 5 | As duas têm camada/extensão para Lambda |
| Nodes, pods, deployments e HPA no Kubernetes | 5 | 5 | Agent/integrações oficiais nas duas ferramentas |
| Health checks e disponibilidade | 5 | 5 | `/health/live`, probes e monitores podem ser usados nas duas |
| Alertas operacionais | 5 | 5 | Ambas suportam alertas por métrica, log e APM |
| Logs JSON com correlação | 5 | 4 | Datadog já recebe o padrão atual e a correlação da instrumentação existente |
| Dashboards | 5 | 5 | Ambas oferecem dashboards customizados |
| Integração com .NET existente | 5 | 4 | Datadog já está instalado e validado nas Lambdas |
| Integração com EKS existente | 5 | 4 | Datadog já está no Terraform; New Relic exigiria novo chart e validação |
| Esforço de integração incremental | 5 | 2 | Datadog requer manutenção da integração atual; New Relic seria uma segunda pilha |
| Risco para o cronograma | 5 | 3 | Datadog não introduz uma ferramenta nova na reta final |

## Cobertura dos requisitos obrigatórios da Fase 3

| Requisito | Implementação com Datadog | Evidência/validação |
|---|---|---|
| Latência | APM .NET, duração de invocações Lambda, métricas do API Gateway e traces | Services/APM, Serverless/Lambda e dashboards |
| Recursos do Kubernetes | DaemonSet do Datadog Agent no EKS, métricas de nodes, pods, deployments, HPA e kube-state-metrics | `kubectl get daemonset -n datadog`; Infrastructure → Kubernetes |
| Health checks | Endpoint público `GET /health/live`, probes do deployment e monitores de disponibilidade | `k8s/nuvem` e rota `GET /health/live` |
| Alertas | Monitores para erro 5xx, p95, timeout de Lambda, reinício de pods, pods pendentes e falha de health check | Monitors do Datadog |
| Logs JSON com correlação | Logs estruturados da API/Lambda, request ID e tracing Datadog com `DD_TRACE_ENABLED` | Logs → Explorer e traces correlacionados |
| Dashboards | Dashboard operacional para API, Lambda, EKS, RDS e gateway | Dashboards Datadog por ambiente |

### Critério de latência

Devem ser acompanhados, no mínimo:

- p50, p95 e p99 da API;
- duração e erro das Lambdas;
- latência do API Gateway/VPC Link;
- tempo de consulta ao RDS quando disponível no trace.

### Critério de recursos Kubernetes

O Agent deve observar CPU, memória, reinícios, estado dos pods, nodes,
replicas, HPA e eventos de scheduling. O Cluster Autoscaler continua sendo
responsável por ajustar nodes; o Datadog apenas observa e alerta esse
comportamento.

### Critério de health checks

O endpoint `GET /health/live` é o alvo de disponibilidade externa. As probes
do Kubernetes continuam sendo a fonte de decisão de liveness/readiness dos
pods. O dashboard e o monitor devem diferenciar indisponibilidade da API,
falha do banco e falha de infraestrutura.

### Critério de alertas

Alertas mínimos:

| Alerta | Condição inicial sugerida |
|---|---|
| API indisponível | `/health/live` sem sucesso por 2 minutos |
| Erro HTTP | 5xx acima de 5% por 5 minutos |
| Latência | p95 acima do SLO definido por 5 minutos |
| Lambda | erro ou timeout acima de 1% por 5 minutos |
| Kubernetes | pod em `CrashLoopBackOff`, `Pending` ou deployment sem réplicas disponíveis |
| Banco | falha de conexão ou saturação observável |

Os limiares finais devem ser calibrados após uma janela de tráfego real, para
evitar alertas ruidosos durante o laboratório.

### Critério de logs e correlação

Os logs devem permanecer em JSON/estrutura equivalente, sem credenciais,
tokens ou dados sensíveis. O identificador da requisição deve permitir seguir
o caminho API Gateway → Lambda ou API → EKS → RDS. A instrumentação deve
associar logs e traces por serviço, ambiente e versão.

### Critério de dashboards

O dashboard mínimo deve apresentar:

- disponibilidade e taxa de erro;
- p95/p99;
- invocações, erros e duração das Lambdas;
- CPU e memória de nodes e pods;
- réplicas desejadas/disponíveis;
- reinícios e pods pendentes;
- estado do RDS e latência de dependências;
- eventos recentes de alerta.

## Custo no período do desafio

Estimativa para até 14 dias, baixo tráfego de demonstração e sem retenção
elevada. Os valores são uma premissa de planejamento e devem ser confirmados
na página comercial de cada fornecedor no momento do uso.

| Item | Datadog | New Relic |
|---|---:|---:|
| Trial durante o desafio | US$ 0, se dentro do trial vigente | US$ 0, se dentro do trial vigente |
| Plano gratuito após trial | Depende dos limites e produtos ativos | US$ 0 dentro dos limites do plano gratuito vigente |
| Ingestão esperada no desafio | Baixa: métricas, logs e traces de demonstração | Baixa: métricas, logs e traces de demonstração |
| Custo adicional AWS da integração | Aproximadamente US$ 0; usa DaemonSet, extensão e NAT já existentes | Aproximadamente US$ 0; mesma premissa, se não houver serviço AWS adicional |
| Risco de cobrança | Logs/traces e hosts podem exceder limites após o trial | Ingestão e usuários podem exceder limites do free tier |

O custo de EKS, NAT Gateway, RDS Multi-AZ, ALB e API Gateway permanece mesmo
sem Datadog ou New Relic. A disciplina operacional é destruir o ambiente ao
final do uso, conforme a RFC-0001.

## Esforço de integração

| Trabalho | Datadog | New Relic |
|---|---:|---:|
| Agent no EKS | Já implementado no Terraform | Criar e validar `nri-bundle` |
| Instrumentação Lambda .NET | Já implementada com extension e camada | Adicionar camadas, segredo, variáveis e validar runtime |
| Logs de containers | Já habilitados no chart | Configurar coleta e parsing |
| APM .NET | Já habilitado | Adicionar agente/camada e validar correlação |
| Dashboards e monitores | Configurar objetos no Datadog | Criar uma segunda coleção de dashboards e monitores |
| Operação no CI/CD | Chave via variável/Secrets Manager | Novos secrets, comandos e validações |
| Risco de coexistência | Baixo | Alto se as duas ferramentas forem instaladas simultaneamente |

## Segurança e privacidade

- Nenhuma API key deve ser commitada em repositórios.
- A chave do Datadog deve ficar no Secrets Manager ou em secret do pipeline.
- Logs não devem conter CPF completo, senha, JWT ou connection string.
- O acesso do Agent deve usar apenas as permissões necessárias.
- A saída da Lambda em VPC depende de NAT ou endpoint compatível para enviar
  telemetria; a regra de entrada do RDS permanece restrita.
- A decisão não autoriza habilitar captura de payloads sensíveis. O padrão
  `DD_CAPTURE_LAMBDA_PAYLOAD=false` deve ser mantido.

## Decisão

Adotar **Datadog** como ferramenta oficial de observabilidade da Fase 3.

New Relic foi descartado para este projeto não por incapacidade técnica, mas
porque duplicaria a plataforma, aumentaria o esforço de integração e
introduziria risco no cronograma sem melhorar a cobertura dos requisitos.

A decisão será considerada aprovada somente após revisão e merge do PR desta
RFC. O PR deve registrar os revisores e eventuais ressalvas nesta seção antes
de alterar o status para `Aceita`.

### Registro de aprovação

| Item | Registro |
|---|---|
| PR de aprovação | A preencher no PR |
| Revisores | A preencher no PR |
| Data de aprovação | A preencher no PR |
| Ressalvas | A preencher no PR |

## Impacto

- **Nos repositórios:** `tech-challenge-infra-k8s` mantém o Helm release e o
  segredo; `tech-challenge-oficina-mecanica` mantém o script de orquestração e
  a documentação; as Lambdas recebem as camadas pelo `datadog-ci`.
- **No cronograma:** não há migração de ferramenta; a implementação pode seguir
  imediatamente após a aprovação.
- **No custo:** o custo da ferramenta tende a ser zero durante o trial ou
  dentro do plano gratuito no período do desafio; os custos AWS continuam
  separados.
- **Na operação:** há uma plataforma única para EKS, API .NET e Lambdas.
- **Na segurança:** a API key fica fora do código, e a coleta de payloads de
  Lambda permanece desabilitada.

## Questões em aberto

- Criar os monitores e dashboards como código ou administrá-los inicialmente
  pela interface do Datadog.
- Definir os SLOs finais de latência após medir uma carga representativa.
- Confirmar os limites comerciais vigentes antes de manter o ambiente ativo
  após o trial.
- Registrar o PR, revisores e aprovação formal desta RFC.
