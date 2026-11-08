# Referência da API — OS Service

Última atualização: 2026-11-08

Esta é a superfície HTTP do **OS Service** na arquitetura de microsserviços da Fase 4. O serviço mantém clientes, veículos, ordens de serviço, histórico e a orquestração da Saga. Catálogo, diagnóstico, estoque e reparo pertencem à Execução; orçamento e pagamento pertencem ao Billing.

## Base local

```text
http://localhost:8080/api/v1
```

O Swagger fica disponível em `http://localhost:8080/swagger` no ambiente `Development`.

## Autenticação

| Método | Rota | Acesso | Responsabilidade |
|---|---|---|---|
| `POST` | `/auth/login` | Anônimo | Autenticação local de funcionários |
| `GET` | `/acompanhamento-os/{codigo}` | Cliente | Acompanhamento da própria OS |

As demais rotas exigem JWT válido. A autenticação externa por CPF continua sob responsabilidade da Lambda e do API Gateway.

## Clientes

| Método | Rota | Responsabilidade |
|---|---|---|
| `POST` | `/clientes` | Cadastrar cliente |
| `GET` | `/clientes` | Listar e filtrar clientes |
| `GET` | `/clientes/documento/{cpfCnpj}` | Consultar por documento |
| `PUT` | `/clientes/documento/{cpfCnpj}` | Atualizar cadastro |
| `DELETE` | `/clientes/documento/{cpfCnpj}` | Excluir cadastro |
| `GET` | `/clientes/{cpfCnpj}/veiculos` | Listar veículos do cliente |
| `POST` | `/clientes/{cpfCnpj}/veiculos` | Vincular veículo ao cliente |

## Veículos

| Método | Rota | Responsabilidade |
|---|---|---|
| `POST` | `/veiculos` | Cadastrar veículo |
| `GET` | `/veiculos` | Listar veículos |
| `GET` | `/veiculos/{id}` | Consultar por ID |
| `GET` | `/veiculos/placa/{placa}` | Consultar por placa |
| `GET` | `/veiculos/cliente/{clienteId}` | Listar por cliente |
| `PUT` | `/veiculos/{id}` | Atualizar veículo |
| `DELETE` | `/veiculos/{id}` | Excluir veículo |

## Ordens de serviço

| Método | Rota | Responsabilidade |
|---|---|---|
| `POST` | `/ordens-servico` | Abrir OS com snapshots de serviços e peças validados na Execução |
| `GET` | `/ordens-servico` | Listar ordens ativas |
| `GET` | `/ordens-servico/{id}` | Consultar OS |
| `PATCH` | `/ordens-servico/{id}/cancelar` | Cancelar OS |
| `PATCH` | `/ordens-servico/{id}/iniciar-diagnostico` | Solicitar início do diagnóstico de forma assíncrona |
| `PATCH` | `/ordens-servico/{id}/entregar` | Entregar veículo após finalização e pagamento |
| `GET` | `/ordens-servico/{id}/historico` | Consultar trajetória da OS |
| `GET` | `/ordens-servico/{id}/notificacoes` | Consultar notificações simuladas |
| `GET` | `/ordens-servico/{id}/monitoramento` | Consultar monitoramento individual |
| `GET` | `/ordens-servico/monitoramento` | Consultar resumo operacional |
| `GET` | `/ordens-servico/{id}/estimativa-tempo-servico` | Calcular estimativa pelos snapshots da OS |

## Fluxo distribuído

O OS Service é o único dono do estado global da ordem. As mudanças produzidas pelos demais microsserviços chegam por eventos idempotentes:

```text
Recebida
  -> EmDiagnostico
  -> AguardandoOrcamento
  -> AguardandoAprovacao
  -> EmExecucao
  -> Finalizada
  -> Entregue
```

- Execução processa diagnóstico, reserva e reparo.
- Billing processa orçamento e pagamento.
- O OS Service reage aos eventos, atualiza a OS e registra o histórico.
- Operações que precisam de resposta imediata, como consulta de catálogo durante a abertura, podem usar HTTP.

## Rotas removidas na Fase 4

As rotas abaixo não fazem mais parte do OS Service e devem retornar `404`:

- `/pecas`, `/servicos` e `/pedidos-compra`;
- composição local de itens após a abertura;
- finalização local do diagnóstico;
- aprovação local do orçamento;
- movimentação e liberação local de estoque;
- finalização local do reparo;
- registro local de pagamento;
- avanço genérico/manual do status.

Consulte as APIs de Execução e Billing para essas responsabilidades.

## Erros

- `400`: regra de negócio ou payload inválido;
- `401`: ausência ou falha de autenticação;
- `403`: identidade sem a permissão exigida;
- `404`: recurso ou rota inexistente;
- `409`: conflito de concorrência, quando aplicável;
- `503`: dependência necessária indisponível.
