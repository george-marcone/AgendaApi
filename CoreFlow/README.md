# CoreFlow

Backend .NET para CRUD de usuarios usando ASP.NET Core, CQRS com MediatR, FluentValidation, EF Core e SQL Server em Docker.

## Estrutura do Projeto

- `CoreFlow.API`: Web API ASP.NET Core, controllers e configuracao de DI.
- `CoreFlow.Application`: commands, queries, handlers, validators, interfaces e behaviors.
- `CoreFlow.Domain`: entidades de dominio.
- `CoreFlow.Infrastructure`: EF Core, `AppDbContext`, servicos de persistencia e migrations.
- `CoreFlow.Tests`: testes automatizados com xUnit.
- `docker/sql/init.sql`: script para criar o banco, tabela e seed inicial.
- `Dockerfile`: build da imagem do backend.
- `docker-compose.yml`: orquestra backend, SQL Server e inicializacao do banco.

## Requisitos

- .NET SDK 10
- Docker Desktop
- SQL Server client opcional: SSMS, Azure Data Studio ou `sqlcmd`

## Banco de Dados

O banco usado pela aplicacao e:

```text
CoreFlowDb
```

As tabelas principais sao:

```text
dbo.Users
```

Colunas de `dbo.Users`:

```sql
Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY
Name NVARCHAR(200) NOT NULL
Email NVARCHAR(200) NOT NULL
Phone NVARCHAR(50) NOT NULL
PasswordHash NVARCHAR(500) NOT NULL
CreatedAt DATETIMEOFFSET NOT NULL
```

Regras de unicidade:

- `Users.Email` nao pode ser repetido.
- `Users.Phone` nao pode ser repetido.

As regras do CRUD sao validadas na camada de aplicacao com FluentValidation. As regras de unicidade tambem sao protegidas no banco com indices unicos:

```text
IX_Users_Email
IX_Users_Phone
```

Credenciais do SQL Server no Docker:

```text
Server: localhost,1433
Database: CoreFlowDb
User: sa
Password: Str0ngP@ssw0rd!
TrustServerCertificate: True
```

Usuario inicial de autenticacao para desenvolvimento:

```text
Email: admin@coreflow.local
Senha: Admin@123456
```

Para consultar os registros:

```sql
USE CoreFlowDb;

SELECT COUNT(*) AS TotalUsers
FROM dbo.Users;

SELECT TOP 10 *
FROM dbo.Users;
```

## Autenticacao JWT

A API usa JWT Bearer. As configuracoes ficam em `Jwt` no `appsettings.json` e podem ser sobrescritas por variaveis de ambiente:

```text
Jwt__Issuer
Jwt__Audience
Jwt__Key
Jwt__ExpiresMinutes
```

Login:

```powershell
$login = @{
  email = "admin@coreflow.local"
  password = "Admin@123456"
} | ConvertTo-Json

$auth = Invoke-RestMethod `
  -Uri "http://localhost:5088/api/Auth/login" `
  -Method Post `
  -ContentType "application/json" `
  -Body $login
```

Validar token/autenticacao:

```powershell
Invoke-RestMethod `
  -Uri "http://localhost:5088/api/Auth/authenticate" `
  -Headers @{ Authorization = "Bearer $($auth.accessToken)" }
```

## Docker

### Subir API e banco

Na raiz do projeto:

```powershell
docker compose up -d --build
```

Servicos:

- `coreflow_api`: backend ASP.NET Core.
- `coreflow_sql`: SQL Server 2022.
- `db-init`: executa `docker/sql/init.sql` para criar/popular o banco.

Portas:

```text
API: http://localhost:5088
SQL Server: localhost,1433
```

### Testar a API em container

```powershell
$login = @{
  email = "admin@coreflow.local"
  password = "Admin@123456"
} | ConvertTo-Json

$auth = Invoke-RestMethod `
  -Uri "http://localhost:5088/api/Auth/login" `
  -Method Post `
  -ContentType "application/json" `
  -Body $login

Invoke-RestMethod `
  -Uri "http://localhost:5088/api/User" `
  -Headers @{ Authorization = "Bearer $($auth.accessToken)" }
```

Criar usuario:

```powershell
$body = @{
  name = "George Santos"
  email = "gmarcones@email.com"
  phone = "+5581997442241"
  password = "User@123456"
} | ConvertTo-Json

Invoke-WebRequest `
  -Uri "http://localhost:5088/api/User" `
  -Method Post `
  -ContentType "application/json" `
  -Headers @{ Authorization = "Bearer $($auth.accessToken)" } `
  -Body $body
```

### Comandos Docker uteis

```powershell
docker ps
docker logs coreflow_api
docker logs coreflow_sql
docker compose down
```

## Execucao Local

Para rodar a API fora do Docker:

```powershell
dotnet run --project CoreFlow.API/CoreFlow.API.csproj
```

O `appsettings.json` esta configurado para acessar o SQL Server exposto pelo container:

```text
Server=localhost,1433;Database=CoreFlowDb;User Id=sa;Password=Str0ngP@ssw0rd!;TrustServerCertificate=True;
```

## Endpoints do CRUD

Base URL local:

```text
http://localhost:5088
```

Rotas de autenticacao:

```text
POST   /api/Auth/login
GET    /api/Auth/authenticate
```

Rotas do CRUD protegidas por JWT:

```text
GET    /api/User
GET    /api/User/{id}
POST   /api/User
PUT    /api/User/{id}
DELETE /api/User/{id}
```

Payload de criacao:

```json
{
  "name": "George Marcone",
  "email": "gmarcone@gmail.com",
  "phone": "+5581997233344",
  "password": "User@123456"
}
```

Payload de atualizacao:

```json
{
  "id": "GUID_DO_USUARIO",
  "name": "George Marcone",
  "email": "gmarcone@gmail.com",
  "phone": "+5581997233344"
}
```

## EF Core

Versao em uso:

```text
Microsoft.EntityFrameworkCore 10.0.8
Microsoft.EntityFrameworkCore.SqlServer 10.0.8
Microsoft.EntityFrameworkCore.Design 10.0.8
Microsoft.EntityFrameworkCore.Tools 10.0.8
```

Arquivos principais:

- `CoreFlow.Infrastructure/Data/AppDbContext.cs`
- `CoreFlow.Infrastructure/Migrations/20260514_InitialCreate.cs`
- `CoreFlow.Infrastructure/Migrations/20260515_AddUserCreatedAt.cs`
- `CoreFlow.Infrastructure/Migrations/20260515_AddUserPasswordHash.cs`
- `CoreFlow.Infrastructure/Migrations/CoreFlow.InfrastructureModelSnapshot.cs`

Listar migrations:

```powershell
dotnet ef migrations list `
  --project CoreFlow.Infrastructure/CoreFlow.Infrastructure.csproj `
  --startup-project CoreFlow.API/CoreFlow.API.csproj
```

Aplicar migrations:

```powershell
dotnet ef database update `
  --project CoreFlow.Infrastructure/CoreFlow.Infrastructure.csproj `
  --startup-project CoreFlow.API/CoreFlow.API.csproj
```

Observacao: o `docker/sql/init.sql` tambem cria `dbo.Users`, popula 50 contatos e cria o usuario inicial de autenticacao. Em ambiente controlado, escolha um fluxo principal para criar o banco: migrations do EF Core ou script SQL de inicializacao.

## CQRS

O projeto usa CQRS com MediatR.

Commands:

- `CreateUserCommand`
- `UpdateUserCommand`
- `DeleteUserCommand`

Queries:

- `GetAllUsersQuery`
- `GetUserByIdQuery`

Handlers:

- `CreateUserHandler`
- `UpdateUserHandler`
- `DeleteUserHandler`
- `GetAllUsersHandler`
- `GetUserByIdHandler`

Validacao:

- `CreateUserCommandValidator`
- `UpdateUserCommandValidator`
- `ValidationBehavior<TRequest, TResponse>`

Os validators bloqueiam email e telefone repetidos em operacoes de criacao e atualizacao.

O `Program.cs` registra MediatR, FluentValidation, pipeline behavior, controllers e infraestrutura. Ele e necessario como composition root da API, mas a regra de CQRS fica na camada `CoreFlow.Application`.

## Testes Unitarios

Frameworks/pacotes:

```text
xUnit 2.9.3
xunit.runner.visualstudio 3.1.5
Microsoft.NET.Test.Sdk 18.5.1
coverlet.collector 10.0.0
```

Executar testes:

```powershell
dotnet test
```

Testes atuais:

- `CoreFlow.Tests/UserTests.cs`: valida entidade, ordenacao por contato mais recente e preservacao de `CreatedAt`.
- `CoreFlow.Tests/AuthTests.cs`: valida hash de senha e autenticacao em memoria.

## Build

Restaurar pacotes:

```powershell
dotnet restore
```

Compilar:

```powershell
dotnet build
```

Checar pacotes vulneraveis:

```powershell
dotnet list package --vulnerable --include-transitive
```

Checar pacotes desatualizados:

```powershell
dotnet list package --outdated
```
