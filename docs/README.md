# Documentação

Este diretório reúne a documentação viva do projeto, alinhada ao código atual do repositório.

## Leitura recomendada

1. [README principal](../README.md)
2. [Rodar os três serviços localmente](EXECUCAO-LOCAL.md) (fluxo completo com LocalStack, sem AWS)
3. [Setup e operação do OS Service](SETUP.md)
4. [Ciclo de vida dos ambientes na AWS](CICLO-DE-VIDA.md) (subir, conferir e derrubar)
5. [Arquitetura](ARCHITECTURE.md)
6. [Referência da API](API_REFERENCE.md)
7. [Infraestrutura](INFRAESTRUTURA.md)
8. [Catálogo de eventos e mensageria](mensageria/catalogo.md)
9. [Relatório do BDD do fluxo completo da OS](bdd/relatorio-bdd.html) (HTML do Reqnroll; baixe e abra no navegador)
10. [Diagramas da arquitetura](diagrams/README.md) (C4, implantação na AWS e sequência da Saga)
11. [Contrato OpenAPI do OS Service](openapi/os-service.json) e [collection do Postman](postman/README.md)
12. [Decisões (ADRs)](adrs/README.md) e [propostas (RFCs)](rfcs/README.md)

## Conteúdo

### [EXECUCAO-LOCAL.md](EXECUCAO-LOCAL.md)

OS Service, Billing e Execução juntos na sua máquina, com LocalStack: subida na ordem certa, o fluxo completo pela collection do Postman (caminho feliz e compensações) e os problemas mais comuns.

### [SETUP.md](SETUP.md)

Pré-requisitos, execução local, execução com Docker, testes e troubleshooting. Só do OS Service.

### [CICLO-DE-VIDA.md](CICLO-DE-VIDA.md)

Como subir e derrubar um ambiente completo (dev, hom ou prod) no Learner Lab com `sobe-tudo.sh` e `derruba-tudo.sh`: pré-requisitos, credenciais da AWS, do Datadog e do Mercado Pago, as dez etapas, a conferência e o que fazer quando dá errado.

### [ARCHITECTURE.md](ARCHITECTURE.md)

Organização em camadas, responsabilidades de cada módulo e fluxo principal da aplicação.

### [API_REFERENCE.md](API_REFERENCE.md)

Rotas, autenticação, payloads principais, fluxo da OS, monitoramento e tratamento de erros.
 
### [INFRAESTRUTURA.md](INFRAESTRUTURA.md)

Onde cada ambiente roda e onde cada coisa é definida: os três ambientes na AWS, o kind como opção de desenvolvimento local, o caminho da requisição até o banco, e por que a infraestrutura vive em outros dois repositórios.

### [Catálogo de eventos e mensageria](mensageria/catalogo.md)

Eventos SNS, comandos SQS, filas consumidoras, filtros e a topologia provisionada por serviço.

### [ROLLBACK-BANCO.md](ROLLBACK-BANCO.md)

Como voltar atrás no banco gerenciado: por que `kubectl rollout undo` não desfaz migration, snapshot manual antes de operação arriscada, restore por snapshot ou point-in-time, e o caminho de emergência de volta ao PostgreSQL no cluster.

### [MATRIZ_AUTORIZACAO.md](MATRIZ_AUTORIZACAO.md)

Classificação das rotas atuais do OS Service entre anônimas, autenticadas e exclusivas do cliente.

### [Histórico](historico/README.md)

Documentos de fases anteriores que não descrevem mais o sistema atual, guardados como registro: a migração para Clean Architecture (Fases 1 e 2) e os relatórios de segurança da Fase 1.

## Convenções

- a documentação deve refletir o estado atual do código;
- documento que deixa de valer vai para `historico/` com uma linha no índice de lá; não fica misturado com os atuais;
- `README.md` da raiz é o ponto de entrada para o projeto;
- `docs/` deve conter documentação operacional e técnica, não anotações temporárias.
