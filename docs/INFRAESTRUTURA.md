# Terraform — Cluster Kubernetes local com kind

Este README descreve a configuração Terraform usada para provisionar um cluster Kubernetes local com **kind** (Kubernetes in Docker), sem necessidade de conta em cloud.

## Visão geral da infraestrutura

A infraestrutura do projeto combina três camadas principais:

- `infra/`: scripts Terraform para criar um cluster Kind local, namespace e recursos de suporte.
- `k8s/`: manifests Kubernetes para implantar a API e o banco de dados PostgreSQL no cluster.
- `docker/`: Dockerfiles que geram a imagem da API e fornecem um exemplo de extensão para o Postgres.

O fluxo de implantação segue estes passos:

1. Provisionar o cluster local com Terraform.
2. Construir a imagem Docker da API.
3. Carregar a imagem no cluster Kind.
4. Aplicar os manifests Kubernetes para criar namespace, deployments, services e HPA.

## Estrutura dos arquivos

```text
infra/
├── main.tf                  # Criação do cluster kind, namespace e metrics-server
├── variables.tf             # Variáveis configuráveis (nome, portas, réplicas)
├── versions.tf              # Providers e versões mínimas exigidas
├── outputs.tf               # Valores exportados após o apply
├── inventories/
│   └── dev/terraform.tfvars  # Configuração do ambiente de desenvolvimento
└── .gitignore               # Exclui state e credenciais do Git
k8s/
├── kustomization.yaml
├── namespace.yaml
├── api/
│   ├── configmap.yaml
│   ├── secret.yaml
│   ├── deployment.yaml
│   ├── service.yaml
│   └── hpa.yaml
└── postgres/
    ├── configmap.yaml
    ├── secret.yaml
    ├── pvc.yaml
    ├── deployment.yaml
    └── service.yaml
docker/
├── backend/
│   └── Dockerfile
└── postgres/
    └── Dockerfile
```

## Infraestrutura do projeto

A infraestrutura do projeto está dividida em três camadas:

- `infra/`: provisiona o cluster Kind local e os recursos do Kubernetes necessários ao ambiente.
- `k8s/`: agrupa os manifests Kubernetes usados para implantar a API e o PostgreSQL no cluster.
- `docker/`: contém os Dockerfiles para a API e o container PostgreSQL estendido.

### Kubernetes

O `kustomization.yaml` orquestra os recursos abaixo, criando-os no namespace `oficina-mecanica`:

- `namespace.yaml`: define o namespace do cluster.
- `postgres/`: configura o banco de dados PostgreSQL, incluindo configmap, secret, PVC, deployment e service.
- `api/`: configura a API da aplicação, com configmap, secret, deployment, service e HPA.

A API é exposta no cluster por um `Service` do tipo `NodePort`, com porta interna `80` apontando para `8080` no pod. O deployment da API usa `imagePullPolicy: Never`, o que significa que a imagem é carregada localmente no Kind antes de aplicar os manifests.

O `HorizontalPodAutoscaler` da API escala entre `2` e `10` réplicas com base em uso de CPU (`70%`) e memória (`75%`).

O PostgreSQL utiliza uma estratégia `Recreate` no deployment para evitar múltiplos pods escrevendo no mesmo volume simultaneamente. O banco monta um `PersistentVolumeClaim` de `5Gi` em `/var/lib/postgresql/data`.

### Docker

O `docker/backend/Dockerfile` builda a aplicação .NET:

- restaura os projetos da solução
- publica a API em Release
- usa a imagem `mcr.microsoft.com/dotnet/aspnet:10.0-alpine` para execução
- expõe a porta `8080`
- define `ASPNETCORE_URLS=http://+:8080`

O `docker/postgres/Dockerfile` estende a imagem `postgres:16` e instala o `pgagent`, usando o Debian package manager.

## Pré-requisitos

Você precisa de **três ferramentas** instaladas no Windows :

### 1. Docker Desktop
Já deve estar instalado (`docker-compose`). Confirme com:
```bash
docker --version
```

### 2. Terraform

Baixe o instalador para Windows em: https://developer.hashicorp.com/terraform/install

Ou instale via **winget** :
```powershell
winget install HashiCorp.Terraform
```

Confirme:
```bash
terraform --version
# Deve mostrar >= 1.6.0
```

### 3. kind

Baixe o binário para Windows em: https://kind.sigs.k8s.io/docs/user/quick-start/#installing-from-release-binaries

Ou instale via **winget**:
```powershell
winget install Kubernetes.kind
```

Confirme:
```bash
kind --version
```
---

## Como executar

**Instale**

winget install HashiCorp.Terraform

winget install Kubernetes.kind

**Acesse a pasta infra/ do projeto**
cd infra

terraform init

terraform apply -var-file="inventories/dev/terraform.tfvars"

**Volte para a raiz do projeto**
cd ..

docker build -f docker/backend/Dockerfile -t oficina-mecanica-api:local .

**Em seguida:**

kind load docker-image oficina-mecanica-api:local --name oficina-mecanica

**Aplicar o kubernets**

kubectl apply -k k8s/

kubectl rollout restart deployment/oficina-mecanica-api -n oficina-mecanica

**Espere até ficar running**

kubectl get pods -n oficina-mecanica -w

**Expor endpoint se maquina windows**

kubectl port-forward -n oficina-mecanica svc/oficina-mecanica-api 8080:80

Esses são os endpoints disponiveis
http://localhost:8080/health    # health check

http://localhost:8080/swagger   # documentacao

POST http://localhost:8080/api/v1/auth/login
  body: { "username": "admin", "password": "admin123" }

**Para destruir**

cd infra

terraform destroy -var-file="inventories/dev/terraform.tfvars"
