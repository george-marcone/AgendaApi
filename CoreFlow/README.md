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

A tabela usada pelo CRUD e:

```text
dbo.Users
```

Colunas:

```sql
Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY
Name NVARCHAR(200) NOT NULL
Email NVARCHAR(200) NOT NULL
Phone NVARCHAR(50) NOT NULL
```

Regras de unicidade:

- `Email` nao pode ser repetido.
- `Phone` nao pode ser repetido.

Essas regras sao validadas na camada de aplicacao com FluentValidation e tambem protegidas no banco com indices unicos:

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

Para consultar os registros:

```sql
USE CoreFlowDb;

SELECT COUNT(*) AS TotalUsers
FROM dbo.Users;

SELECT TOP 10 *
FROM dbo.Users;
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
Invoke-RestMethod http://localhost:5088/api/User
```

Criar usuario:

```powershell
$body = @{
  name = "George Santos"
  email = "gmarcones@email.com"
  phone = "+55 81 997442241"
} | ConvertTo-Json

Invoke-WebRequest `
  -Uri "http://localhost:5088/api/User" `
  -Method Post `
  -ContentType "application/json" `
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

Rotas:

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
  "phone": "+55 81 997233344"
}
```

Payload de atualizacao:

```json
{
  "id": "GUID_DO_USUARIO",
  "name": "George Marcone",
  "email": "gmarcone@gmail.com",
  "phone": "+55 81 997233344"
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

Observacao: o `docker/sql/init.sql` tambem cria a tabela `dbo.Users` e popula 50 registros. Em ambiente controlado, escolha um fluxo principal para criar o banco: migrations do EF Core ou script SQL de inicializacao.

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

Teste atual:

- `CoreFlow.Tests/UserTests.cs`: valida valores padrao da entidade `User`.

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
