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
