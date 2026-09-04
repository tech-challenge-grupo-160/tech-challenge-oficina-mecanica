# Documentação

Este diretório reúne a documentação viva do projeto, alinhada ao código atual do repositório.

## Leitura recomendada

1. [README principal](../README.md)
2. [Setup e operação](SETUP.md)
3. [Arquitetura](ARCHITECTURE.md)
4. [Referência da API](API_REFERENCE.md)
5. [Infraestrutura](INFRAESTRUTURA.md)

## Conteúdo

### [SETUP.md](SETUP.md)

Pré-requisitos, execução local, execução com Docker, testes e troubleshooting.

### [ARCHITECTURE.md](ARCHITECTURE.md)

Organização em camadas, responsabilidades de cada módulo e fluxo principal da aplicação.

### [API_REFERENCE.md](API_REFERENCE.md)

Rotas, autenticação, payloads principais, fluxo da OS, monitoramento e tratamento de erros.
 
### [INFRAESTRUTURA.md](INFRAESTRUTURA.md)

Onde cada ambiente roda e onde cada coisa é definida: os três ambientes na AWS, o kind como opção de desenvolvimento local, o caminho da requisição até o banco, e por que a infraestrutura vive em outros dois repositórios.

### [ROLLBACK-BANCO.md](ROLLBACK-BANCO.md)

Como voltar atrás no banco gerenciado: por que `kubectl rollout undo` não desfaz migration, snapshot manual antes de operação arriscada, restore por snapshot ou point-in-time, e o caminho de emergência de volta ao PostgreSQL no cluster.

### [MATRIZ_AUTORIZACAO.md](MATRIZ_AUTORIZACAO.md)

Classificação das 53 rotas entre públicas, autenticadas, de cliente e internas, com o critério de sensibilidade e o que o API Gateway deve rotear.

## Convenções

- a documentação deve refletir o estado atual do código;
- `README.md` da raiz é o ponto de entrada para o projeto;
- `docs/` deve conter documentação operacional e técnica, não anotações temporárias.

