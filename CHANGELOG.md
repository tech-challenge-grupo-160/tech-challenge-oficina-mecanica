# Changelog

Todas as mudancas notaveis do projeto serao documentadas neste arquivo.

O formato segue [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/).

## [Unreleased]

### Added
- Endpoint `PATCH /ordens-servico/{numero}/avancar-status` para avancar a OS para o proximo status do fluxo principal
- CONTRIBUTING.md com guia de branches, commits, PRs e convencoes de codigo
- CHANGELOG.md com historico de mudancas por versao
- Documentacao de erros detalhada por endpoint na API_REFERENCE.md
- Documentacao de uso do Postman para a API
- Templates de PR e issues no GitHub
- Diagramas Mermaid de arquitetura e fluxo de autenticacao
- Busca em lote por IDs em servicos e pecas (repositorios e handler)
- Validacao de idempotencia em `RegistrarPagamento` e `Entregar`
- Tipos de evento `ServicoRemovido` e `PecaRemovida` no enum `TipoEventoOrdemServico`
- CI/CD local com GitHub Actions self-hosted runner

### Changed
- Monitoramento de ordens de servico refatorado para contagem e media no banco (sem carregar todas as entidades em memoria)
- Tabela de transicoes de status completada com 6 transicoes faltantes (estoque e cancelamento)
- Metodos `Cancelar`, `BloquearPorFaltaEstoque` e `LiberarExecucaoAposValidacaoEstoque` agora passam por `AlterarStatus`

### Fixed
- Tipo de evento errado ao remover servicos e pecas da OS (usava `ServicoAdicionado`/`PecaAdicionada`)
- Links de documentacao quebrados e inconsistentes no README.md

## [1.0.0] - 2026-04-30

### Added
- API REST completa para gestao de oficina mecanica
- CRUD de clientes, veiculos, servicos e pecas
- Fluxo completo de ordem de servico: abertura, diagnostico, orcamento, aprovacao, execucao, finalizacao e entrega
- Maquina de estados para transicoes de status da OS
- Gestao de estoque com movimentacoes automaticas (baixa e reposicao)
- Fluxo de bloqueio por falta de estoque com pedidos de compra
- Endpoint publico de acompanhamento de OS por codigo e token
- Notificacoes ao cliente (orcamento disponivel, servico finalizado)
- Historico e auditoria de eventos da OS
- Autenticacao JWT com roles
- Validacao de exclusao de entidades vinculadas a OS ativas
- Monitoramento de ordens com tempo medio de finalizacao
- Estimativa de tempo de servico
- Testes unitarios e de integracao com xUnit e Testcontainers
- Documentacao Swagger/OpenAPI
- Colecao Postman com ambiente configurado
- Docker Compose com PostgreSQL 16 e health checks
- Migrations com retry e seeding na inicializacao

### Infrastructure
- Terraform para provisionamento de infraestrutura
- Manifests Kubernetes (deployment, service, configmap)
- Imagem Docker otimizada com Alpine e usuario nao-root
- Pipeline CI para PRs na develop

### Architecture
- Clean Architecture com 5 camadas: API, Application, Domain, Infrastructure, Shared
- CQRS com MediatR (Commands, Queries, Handlers)
- FluentValidation com ValidationBehavior no pipeline do MediatR
- Entidades de dominio encapsuladas (nao anemicas)
- Repositorios abstraidos por interfaces na Application
- Mapeamentos EF Core separados em IEntityTypeConfiguration
- Bootstrap modular com AddApplication e AddInfrastructure
- Geracao de JWT extraida para TokenGenerator
- Logging estruturado centralizado
