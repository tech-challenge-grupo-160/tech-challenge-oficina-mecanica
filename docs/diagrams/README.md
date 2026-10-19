# Diagramas

Diagramas de arquitetura do projeto, organizados pela fase do Tech Challenge em que foram feitos. Os de fases anteriores ficam como histórico: mostram como o sistema evoluiu e não são apagados quando a arquitetura muda.

## Índice

| Diagrama | Fase | Situação | Formato |
|---|---|---|---|
| [Arquitetura em microsserviços](fase-4/arquitetura-microsservicos.md) | 4 | **Atual** (desenho da Sprint 1; base da F4-40) | Mermaid |
| [Implantação na AWS](fase-3/aws-deployment.md) | 3 | Atual para a infraestrutura; na Fase 4 a API vira três serviços | PlantUML + [PNG](fase-3/aws-deployment.png) |
| [Sequência — autenticação por CPF](fase-3/sequencia-autenticacao.drawio.png) | 3 | Atual: a autenticação não muda na Fase 4 | draw.io |
| [Sequência — abertura da OS](fase-3/sequencia-abertura-os.drawio.png) | 3 | Atual até a Saga (F4-25), quando a abertura passa a envolver os três serviços | draw.io |
| [Infraestrutura na AWS](fase-3/infraestrutura-aws.drawio.png) | 3 | Atual para a infraestrutura (o desenho mostra `sa-east-1`; o lab usa `us-east-1`); na Fase 4 a API vira três serviços e entra a mensageria | draw.io |
| [Modelo de dados (ER)](fase-3/er-banco-de-dados.drawio.png) | 3 | Atual até a extração (F4-16), quando o banco é dividido entre os serviços | draw.io |
| [C4 — contexto](fase-1-2/c4-contexto.md) | 1 e 2 | Histórico | PlantUML + [PNG](fase-1-2/c4-contexto.png) |
| [C4 — containers](fase-1-2/c4-containers.md) | 1 e 2 | Histórico: monólito | PlantUML + [PNG](fase-1-2/c4-containers.png) |
| [C4 — componentes](fase-1-2/c4-componentes.md) | 1 e 2 | Histórico | PlantUML + [PNG](fase-1-2/c4-componentes.png) |
| [DevOps local (kind)](fase-1-2/devops-architecture.png) | 2 | Histórico: substituído pelo EKS na Fase 3 | draw.io ([fonte](fase-1-2/devops-architecture.drawio)) |

## Formatos

- **Mermaid** (`.md`): o GitHub desenha direto na página e o diff aparece no PR. É o formato para diagramas novos.
- **PlantUML** (`.md` com bloco `plantuml`): o GitHub não renderiza; o `.png` ao lado é a imagem exportada. Para editar, copie o bloco para o [editor do PlantUML](https://www.plantuml.com/plantuml) ou para a extensão do VS Code e exporte o PNG de novo.
- **draw.io** (`.drawio.png`): o diagrama editável fica embutido no próprio PNG. Abra o arquivo no [draw.io](https://app.diagrams.net) ou na extensão do VS Code, edite e salve no mesmo arquivo.

## Convenções

- Uma pasta por fase: `fase-N/`.
- Nomes em kebab-case, sem espaço nem acento.
- Diagrama que deixa de valer não é apagado: muda a situação no índice para "Histórico".
