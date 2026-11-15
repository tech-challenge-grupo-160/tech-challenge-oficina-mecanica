# Diagramas

Diagramas de arquitetura do projeto, organizados pela fase do Tech Challenge em que foram feitos. Os de fases anteriores ficam como histórico: mostram como o sistema evoluiu e não são apagados quando a arquitetura muda.

## Índice

| Diagrama | Fase | Situação | Formato |
|---|---|---|---|
| [C4 — contexto](fase-4/c4-contexto.md) | 4 | **Atual** | Mermaid |
| [C4 — containers](fase-4/c4-containers.md) | 4 | **Atual**: os três serviços, bancos e mensageria | Mermaid |
| [Implantação na AWS](fase-4/implantacao-aws.md) | 4 | **Atual** | Mermaid |
| [Sequência da Saga](fase-4/saga-sequencia.md) | 4 | **Atual**: caminho feliz e compensações | Mermaid |
| [Arquitetura em microsserviços](fase-4/arquitetura-microsservicos.md) | 4 | Atual (visão resumida da Sprint 1) | Mermaid |
| [Implantação na AWS](fase-3/aws-deployment.md) | 3 | Histórico: uma API só; substituído pela [implantação da Fase 4](fase-4/implantacao-aws.md) | PlantUML + [PNG](fase-3/aws-deployment.png) |
| [Sequência — autenticação por CPF](fase-3/sequencia-autenticacao.drawio.png) | 3 | Atual: a autenticação não muda na Fase 4 | draw.io |
| [Sequência — abertura da OS](fase-3/sequencia-abertura-os.drawio.png) | 3 | Histórico: a abertura e o fluxo da OS estão na [sequência da Saga](fase-4/saga-sequencia.md) | draw.io |
| [Infraestrutura na AWS](fase-3/infraestrutura-aws.drawio.png) | 3 | Histórico: substituído pela [implantação da Fase 4](fase-4/implantacao-aws.md) | draw.io |
| [Modelo de dados (ER)](fase-3/er-banco-de-dados.drawio.png) | 3 | Histórico: banco único da Fase 3; na Fase 4 cada serviço tem o seu ([ADR-0006](../adrs/0006-banco-por-servico.md)) | draw.io |
| [C4 — contexto](fase-1-2/c4-contexto.md) | 1 e 2 | Histórico | PlantUML + [PNG](fase-1-2/c4-contexto.png) |
| [C4 — containers](fase-1-2/c4-containers.md) | 1 e 2 | Histórico: monólito | PlantUML + [PNG](fase-1-2/c4-containers.png) |
| [C4 — componentes](fase-1-2/c4-componentes.md) | 1 e 2 | Histórico | PlantUML + [PNG](fase-1-2/c4-componentes.png) |
| [DevOps local (kind)](fase-1-2/devops-architecture.png) | 2 | Histórico: substituído pelo EKS na Fase 3 | draw.io ([fonte](fase-1-2/devops-architecture.drawio)) |

## Formatos

- **Mermaid** (`.md`): o GitHub desenha direto na página e o diff aparece no PR. É o formato para diagramas novos.
- **PlantUML** (`.md` com bloco `plantuml`): o GitHub não renderiza; o `.png` ao lado é a imagem exportada. Para editar, copie o bloco para o [editor do PlantUML](https://www.plantuml.com/plantuml) ou para a extensão do VS Code e exporte o PNG de novo.
- **draw.io** (`.drawio.png`): o diagrama editável fica embutido no próprio PNG. Abra o arquivo no [draw.io](https://app.diagrams.net) ou na extensão do VS Code, edite e salve no mesmo arquivo.

## Exportar para imagem

Os diagramas da Fase 4 são Mermaid. Para colocar em documento (o PDF de
entrega, por exemplo):

1. Copie o conteúdo do bloco `mermaid` do arquivo, sem as linhas de crase.
2. Cole no [Mermaid Live Editor](https://mermaid.live).
3. Use **Actions → PNG** (ou SVG) e salve a imagem.

Com o [mermaid-cli](https://github.com/mermaid-js/mermaid-cli) instalado, o mesmo
sai pela linha de comando:

```bash
npx -p @mermaid-js/mermaid-cli mmdc -i diagrama.mmd -o diagrama.png -s 2
```

## Convenções

- Uma pasta por fase: `fase-N/`.
- Nomes em kebab-case, sem espaço nem acento.
- Diagrama que deixa de valer não é apagado: muda a situação no índice para "Histórico".
