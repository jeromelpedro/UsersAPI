# FIAP Cloud Games - Users API

Uma API REST em .NET 9 para gerenciar usuários. Responsável por cadastro, autenticação (geração de token JWT) e autorização de usuários. Pronta para rodar em container com Docker e orquestração via Docker Compose. Inclui inicialização do banco, seed de usuário de teste e integração com RabbitMQ.

## Sumário
- Sobre
- Tecnologias
- Pré-requisitos
- Variáveis e configurações importantes
- Executando com Docker Compose (modo recomendado)
- Acessando a API (Swagger & endpoints)
- Exemplos de requests (PowerShell e curl)
- DTOs / Exemplos de payload
- Banco de dados & Seed
- RabbitMQ & Painel de gestão
- Segurança e produção
- Executando localmente (opcional)
- Testes
- Troubleshooting
- Contribuição

## Sobre
Esta API fornece endpoints para autenticação (`Login`) e para operações CRUD/administrativas de usuários (`UsuarioController`). O projeto já inclui scripts para criar o banco e um seed inicial com um usuário admin de teste.

## Tecnologias
- .NET 9
- ASP.NET Core Web API
- Entity Framework Core (SQL Server)
- Docker / Docker Compose
- RabbitMQ
- Redis (cache de autenticação)
- JWT (autenticação)
- Swagger (documentação)

## Pré-requisitos
- Docker Desktop instalado e em execução (Compose v2 recomendado).
- Pelo menos 4GB de RAM livre ao rodar a stack completa (SQL Server + RabbitMQ + API).
- Git (opcional).

Exemplos de comandos abaixo são para PowerShell (Windows).

## Variáveis e configurações importantes
As configurações padrão estão em `src/Users.Api/appsettings.json` e o `docker-compose.yaml` define as variáveis de ambiente para o container `api`.

Principais chaves:
- Connection Strings
	- `ConnectionStrings:SetupConnection` — usada para criar/migrar banco e criar o login `usuario_app`.
	- `ConnectionStrings:DefaultConnection` — conexão usada pela aplicação.
- JWT
	- `Jwt:Key` — chave simétrica (trocar em produção)
	- `Jwt:Issuer`, `Jwt:Audience`
- Secrets
	- `Secrets:Password`
- RabbitMQ
	- `RabbitMq:HostName`, `RabbitMq:Port`, `RabbitMq:UserName`, `RabbitMq:Password`, `RabbitMq:ExchangeName`
- Redis
	- `Redis:Host` — host do Redis (fallback padrão: `localhost`)

Valores padrão do repositório (exemplos):
- SA SQL Server: `SenhaForte123!`
- Usuário application criado: `usuario_app` / `SenhaForte123!`
- Seed admin: `teste@teste.com` / `SenhaForte123!` (Role = Admin)
- Porta exposta da API: 5055

> Atenção: troque todas as credenciais antes de usar em produção.

## Executando com Docker Compose (recomendado)
O arquivo `docker-compose.yaml` sobe três serviços:
- `mssql` (SQL Server 2022)
- `rabbitmq` (com management UI)
- `api` (construída pelo `Dockerfile`)

Na raiz do projeto (onde está `docker-compose.yaml`) execute em PowerShell:

```powershell
docker compose up --build -d
```

Verificar status dos serviços:

```powershell
docker compose ps
```

Ver logs da API:

```powershell
docker compose logs -f api
```

Parar e remover a stack:

```powershell
docker compose down
```

O `docker-compose.yaml` já injeta variáveis de ambiente (connection strings, jwt, rabbitmq). Para sobrescrever localmente use um `.env` ou ajuste o `environment` no `docker-compose.yaml`.

## Acessando a API e documentação (Swagger)
A UI do Swagger está configurada na raiz da aplicação (RoutePrefix = string.Empty). Depois de subir os serviços, abra:

http://localhost:5055/

Lá você encontra a documentação interativa com todos os endpoints.

## Endpoints principais
Base: `http://localhost:5055/api`

- POST `/api/login`
	- Autentica o usuário e retorna um token JWT (string).
	- Parâmetros: `Email`, `Senha` (query string ou form-url-encoded).
- POST `/api/usuario/CadastrarUsuario`
	- Cadastra um usuário (não-admin). Body: `UsuarioCadastroDto`.
- POST `/api/usuario/CadastrarUsuarioAdmin`
	- Cadastra usuário admin — requer role `Admin`.
- POST `/api/usuario/AlterarSenha`
	- Alterar senha — Body: `AlterarSenhaInputDto`.
- GET `/api/usuario/Listar`
	- Lista todos usuários (AllowAnonymous).
- GET `/api/usuario/ListarPorId/{id}`
	- Retorna usuário por id.
- DELETE `/api/usuario/Excluir/{id}`
	- Exclui usuário (requer Admin).

Observação: `LoginController.Login(string Email, string Senha)` aceita parâmetros simples — prefira query string ou `application/x-www-form-urlencoded`.

## DTOs / Exemplos de payload
Campos reais extraídos do código (arquivo `src/Users.Domain/Dto`):

- `UsuarioCadastroDto`
	```json
	{
		"Nome": "Nome do Usuário",
		"Email": "usuario@exemplo.com",
		"Senha": "SenhaForte123!"
	}
	```

- `AlterarSenhaInputDto`
	```json
	{
		"IdUsuario": "id-do-usuario",
		"SenhaAntiga": "SenhaForte123!",
		"SenhaNova": "NovaSenha!234"
	}
	```

## Exemplos de requests
PowerShell — login com usuário seed:

```powershell
#$token recebe o JWT retornado
$token = Invoke-RestMethod -Method Post -Uri "http://localhost:5055/api/login?Email=teste@teste.com&Senha=SenhaForte123!"

#usar token em chamadas autenticadas
$headers = @{ Authorization = "Bearer $token" }
Invoke-RestMethod -Method Get -Uri "http://localhost:5055/api/usuario/Listar" -Headers $headers
```

curl (login via query string):

```bash
curl -X POST "http://localhost:5055/api/login?Email=teste@teste.com&Senha=SenhaForte123!"
```

## Banco de dados e seed
- O `docker-compose` expõe SQL Server na porta 1433.
- A classe `DatabaseUserInitializer` usa `ConnectionStrings:SetupConnection` para garantir que o banco exista, aplicar migrações e criar o login `usuario_app` com role `db_owner`.
- `SeedUsuario` adiciona `teste@teste.com` como Admin (se não existir).

## Redis (cache de autenticação)
O login usa Redis como cache antes da consulta ao banco para reduzir latência em autenticações repetidas.

Fluxo atual na autenticação:
- A API monta uma chave no formato `[email]_[senhaEncrypt]` (senha criptografada com `Encrypt`).
- Faz `GET` no Redis para essa chave.
- Se existir valor (cache hit), desserializa o objeto `user` e já gera o JWT sem consultar o banco.
- Se não existir (cache miss), segue o fluxo normal: busca usuário no banco e valida senha.
- Em caso de sucesso no fluxo normal, salva no Redis o objeto `user` serializado nessa mesma chave.

Detalhes de implementação:
- Valor armazenado: objeto `user` serializado em JSON.
- TTL da chave: 1 horas.
- Se o Redis estiver indisponível, o login continua funcionando via banco (fallback sem quebra de fluxo).

## RabbitMQ
- Porta AMQP: 5672
- Management UI: 15672 (ex.: http://localhost:15672/)
- Credenciais padrão: guest / guest

## Segurança e produção
- Troque imediatamente: `Jwt:Key`, `Secrets:Password`, senha do `sa` e do `usuario_app`.
- Use Secret Manager/Key Vault/variáveis de ambiente para segredos.
- Ative TLS/HTTPS em produção.

## Executando localmente (sem Docker) — opcional
1. Abra a solução `Users.slnx` no Visual Studio / VS Code.
2. Ajuste `ASPNETCORE_ENVIRONMENT=Development` e `appsettings.Development.json` se necessário.
3. No diretório `src/Users.Api`:

```powershell
dotnet restore
dotnet build
dotnet run --project .\\Users.Api.csproj
```

## Testes
Para executar testes unitários:

```powershell
dotnet test tests/Users.Tests/Users.Tests.csproj
```

## Troubleshooting (problemas comuns)
- SQL Server não sobe: verifique recursos (memória/disco) e logs: `docker compose logs -f mssql`.
- RabbitMQ inativo: `docker compose logs -f rabbitmq`.
- 401 / Token inválido: confira `Jwt:Key`, `Issuer` e `Audience`.
- Erro ao criar usuário DB: confira `ConnectionStrings:SetupConnection` e permissões.

## Próximos passos sugeridos
- Externalizar segredos (Key Vault).
- Adicionar CI que constrói a imagem Docker e executa testes.
- Criar `.env.example` com variáveis sensíveis para desenvolvimento local.

## Contribuição
Abra issues e PRs. Mantenha os testes verdes e atualize a documentação quando adicionar novos serviços ou mudanças de contrato.

## ☸️ Kubernetes

### Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) com Kubernetes habilitado
- [kubectl](https://kubernetes.io/docs/tasks/tools/) (já incluso no Docker Desktop)

### Habilitar Kubernetes no Docker Desktop

1. Abra o **Docker Desktop**
2. Vá em **Settings** (ícone de engrenagem)
3. Clique em **Kubernetes** no menu lateral
4. Marque **Enable Kubernetes**
5. Clique em **Apply & Restart**
6. Aguarde o Kubernetes iniciar (ícone verde no canto inferior esquerdo)

### Deploy da Aplicação

#### Passo 1: Construir a imagem Docker

```bash
# Na raiz do projeto
docker build -t users-api:latest .
```

#### Passo 2: Aplicar os manifests Kubernetes

```bash
# Aplicar todos os recursos (ConfigMap, Secret, Deployment e Service)
kubectl apply -f ./k8s/
```

**Saída esperada:**
```
configmap/users-api-config created
deployment.apps/users-api created
secret/users-api-secret created
service/users-api created
```

#### Passo 3: Verificar o status

```bash
# Ver status dos pods
kubectl get pods

# Ver status dos serviços
kubectl get services

# Ver logs da aplicação
kubectl logs -f deployment/users-api
```

**Saída esperada:**
```
NAME                           READY   STATUS    RESTARTS   AGE
users-api-75b78fc9f-xxxxx   1/1     Running   0          30s
```

#### Passo 4: Acessar a aplicação

Como o Service é do tipo `ClusterIP`, use **port-forward** para acessar localmente:

```bash
kubectl port-forward service/users-api 5055:5055
```

A aplicação estará disponível em:
- **API:** http://localhost:5055
- **Swagger:** http://localhost:5055/swagger

### Arquivos de Configuração Kubernetes

| Arquivo | Descrição |
|---------|-----------|
| `k8s/configmap.yaml` | Configurações não-sensíveis (hostname RabbitMQ, filas, etc.) |
| `k8s/secret.yaml` | Credenciais sensíveis (usuário/senha RabbitMQ em Base64) |
| `k8s/deployment.yaml` | Definição do pod, replicas, health checks e recursos |
| `k8s/service.yaml` | Exposição do serviço internamente no cluster |

### Comandos Úteis

```bash
# Ver detalhes do pod
kubectl describe pod -l app=users-api

# Ver eventos do cluster
kubectl get events --sort-by='.lastTimestamp'

# Escalar replicas
kubectl scale deployment/users-api --replicas=3

# Atualizar após mudanças na imagem
docker build -t users-api:latest .
kubectl rollout restart deployment/users-api

# Remover todos os recursos
kubectl delete -f ./k8s/