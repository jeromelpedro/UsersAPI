# CloudGames - Infraestrutura Docker

Este projeto contém a configuração Docker Compose unificada para toda a infraestrutura CloudGames.

## Serviços

### Infraestrutura
- **mssql-users** - SQL Server para UsersAPI (porta 1433)
- **mssql-catalog** - SQL Server para CatalogAPI (porta 1434)
- **rabbitmq** - Message Broker (portas 5672, 15672)

### Microsserviços
- **UsersAPI** - Gerenciamento de usuários e autenticação (porta 5055)
- **CatalogAPI** - Catálogo de jogos (porta 5063)
- **NotificationsAPI** - Serviço de notificações (porta 5056)
- **PaymentsAPI** - Processamento de pagamentos (porta 5058)

## Comandos

### Subir todos os serviços

```bash
docker compose up -d --build
```

- `-d`: Executa em modo detached (background)
- `--build`: Reconstrói as imagens antes de iniciar

### Parar e remover todos os serviços

```bash
docker compose down
```

### Parar e remover incluindo volumes

```bash
docker compose down -v
```

> ⚠️ **Atenção**: O flag `-v` remove todos os volumes, apagando os dados persistidos nos bancos de dados.

### Visualizar logs

```bash
docker compose logs -f
```

### Visualizar logs de um serviço específico

```bash
docker compose logs -f users-api
```

### Verificar status dos containers

```bash
docker compose ps
```

---

## Deploy no Kubernetes

O deploy no Kubernetes deve ser feito na ordem correta para garantir que as dependências estejam disponíveis antes dos microsserviços.

### Pré-requisitos

- Kubernetes cluster configurado (minikube, kind, Docker Desktop, AKS, EKS, GKE, etc.)
- `kubectl` instalado e configurado
- Imagens Docker dos microsserviços disponíveis no registry

### Ordem de Deploy

A infraestrutura deve ser implantada primeiro, seguida pelos microsserviços.

#### 1. Infraestrutura (CatalogAPI/k8s)

O CatalogAPI contém os manifests da infraestrutura compartilhada (SQL Server e RabbitMQ). Aplique todos os recursos de uma vez:

```bash
kubectl apply -f ../CatalogAPI/k8s/
```

Aguarde os pods ficarem prontos:

```bash
kubectl get pods -w
```

#### 2. Microsserviços

Após a infraestrutura estar pronta, aplique os microsserviços:

```bash
kubectl apply -f ../UsersAPI/k8s/
kubectl apply -f ../NotificationsAPI/k8s/
kubectl apply -f ../PaymentsAPI/k8s/
```

> 💡 **Dica**: O `kubectl apply -f <pasta>/` aplica todos os arquivos YAML da pasta automaticamente.

### Comandos Úteis

#### Verificar status de todos os pods

```bash
kubectl get pods
```

#### Verificar status dos serviços

```bash
kubectl get services
```

#### Visualizar logs de um pod

```bash
kubectl logs -f <nome-do-pod>
```

#### Acessar um serviço localmente (port-forward)

```bash
# UsersAPI
kubectl port-forward svc/users-api 5055:80

# CatalogAPI
kubectl port-forward svc/catalog-api 5063:80

# NotificationsAPI
kubectl port-forward svc/notifications-api 5056:80

# PaymentsAPI
kubectl port-forward svc/payments-api 5058:80

# RabbitMQ Management
kubectl port-forward svc/rabbitmq 15672:15672
```

#### Remover todos os recursos

```bash
# Microsserviços
kubectl delete -f ../PaymentsAPI/k8s/
kubectl delete -f ../NotificationsAPI/k8s/
kubectl delete -f ../CatalogAPI/k8s/
kubectl delete -f ../UsersAPI/k8s/
```

> ⚠️ **Atenção**: Remover os PVCs (Persistent Volume Claims) apagará os dados persistidos nos bancos de dados.
