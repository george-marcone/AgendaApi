# CoreFlow

Backend .NET para CRUD de usuarios usando ASP.NET Core, CQRS com MediatR, FluentValidation, EF Core, SQL Server, RabbitMQ e Worker de notificacoes por e-mail em Docker.

## Estrutura do Projeto

- `CoreFlow.API`: Web API ASP.NET Core, controllers e configuracao de DI.
- `CoreFlow.Application`: commands, queries, handlers, validators, interfaces e behaviors.
- `CoreFlow.Domain`: entidades de dominio.
- `CoreFlow.Infrastructure`: EF Core, `AppDbContext`, servicos de persistencia e migrations.
- `CoreFlow.Worker`: worker em background que consome eventos do RabbitMQ e envia e-mails.
- `CoreFlow.Tests`: testes automatizados com xUnit.
- `docker/sql/init.sql`: script para criar o banco, tabela e seed inicial.
- `Dockerfile`: build da imagem do backend.
- `docker-compose.yml`: orquestra backend, Worker, RabbitMQ, Mailpit, SQL Server e inicializacao do banco.

## Requisitos

- .NET SDK 10
- Docker Desktop
- SQL Server client opcional: SSMS, Azure Data Studio ou `sqlcmd`

## RabbitMQ e e-mails

O projeto publica eventos no RabbitMQ quando um contato da agenda e criado, atualizado ou removido.
O Worker consome esses eventos e envia dois e-mails por operacao:

- um e-mail para o usuario logado que executou a acao;
- um e-mail para o contato que foi criado, editado ou removido.

Eventos publicados:

```text
ContactCreated
ContactUpdated
ContactDeleted
```

No codigo, esses eventos sao representados por:

```text
CoreFlow.Application/Events/ContactChangedEvent.cs
```

Fluxo:

```text
UserController
  -> MediatR
    -> Create/Update/Delete Handler
      -> SQL Server
      -> IContactEventPublisher
        -> RabbitMQ
          -> CoreFlow.Worker
            -> SMTP
              -> E-mail para usuario logado
              -> E-mail para contato afetado
```

O envio de e-mail e feito pelo Worker:

```text
CoreFlow.Worker
```

Em desenvolvimento, o `docker-compose.yml` sobe o Mailpit como servidor SMTP de teste. Ele recebe os e-mails enviados pelo Worker e exibe tudo em uma interface web. Nessa configuracao padrao, os e-mails nao chegam em Gmail/Outlook real; eles aparecem em `http://localhost:8025`.

URLs locais:

```text
RabbitMQ AMQP: amqp://localhost:5672
RabbitMQ Management: http://localhost:15672
Mailpit UI: http://localhost:8025
Mailpit SMTP: localhost:1025
```

Credenciais RabbitMQ:

```text
User: guest
Password: guest
```

Fila usada:

```text
coreflow.contact.email-notifications
```

Exchange:

```text
coreflow.contacts
```

Routing key:

```text
contact.changed
```

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
UpdatedAt DATETIMEOFFSET NOT NULL
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
- `coreflow_worker`: consome eventos do RabbitMQ e envia e-mails.
- `coreflow_rabbitmq`: broker RabbitMQ com painel de gerenciamento.
- `coreflow_mailpit`: SMTP de desenvolvimento e caixa de entrada web.
- `coreflow_sql`: SQL Server 2022.
- `db-init`: executa `docker/sql/init.sql` para criar/popular o banco.

Portas:

```text
API: http://localhost:5088
SQL Server: localhost,1433
RabbitMQ AMQP: localhost,5672
RabbitMQ Management: http://localhost:15672
Mailpit UI: http://localhost:8025
Mailpit SMTP: localhost,1025
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

Depois de criar, editar ou excluir um contato, veja os e-mails gerados em:

```text
http://localhost:8025
```

### Testar notificacoes por e-mail

Com os containers ativos, faca login e crie um contato:

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

$email = "contato.rabbitmq@example.com"

$body = @{
  name = "Contato RabbitMQ"
  email = $email
  phone = "+5581997442299"
  password = "User@123456"
} | ConvertTo-Json

Invoke-WebRequest `
  -Uri "http://localhost:5088/api/User" `
  -Method Post `
  -ContentType "application/json" `
  -Headers @{ Authorization = "Bearer $($auth.accessToken)" } `
  -Body $body
```

Abra o Mailpit e confirme os dois e-mails de cadastro:

- `Voce cadastrou um novo contato`, enviado ao usuario logado.
- `Voce foi adicionado a agenda`, enviado ao contato cadastrado.

```text
http://localhost:8025
```

Para testar e-mail de edicao, localize o contato criado e envie um `PUT`:

```powershell
$contacts = Invoke-RestMethod `
  -Uri "http://localhost:5088/api/User" `
  -Headers @{ Authorization = "Bearer $($auth.accessToken)" }

$contact = $contacts | Where-Object { $_.email -eq $email } | Select-Object -First 1

$updateBody = @{
  id = $contact.id
  name = "Contato RabbitMQ Atualizado"
  email = $contact.email
  phone = "+5581997442200"
} | ConvertTo-Json

Invoke-WebRequest `
  -Uri "http://localhost:5088/api/User/$($contact.id)" `
  -Method Put `
  -ContentType "application/json" `
  -Headers @{ Authorization = "Bearer $($auth.accessToken)" } `
  -Body $updateBody
```

Para testar e-mail de remocao:

```powershell
Invoke-WebRequest `
  -Uri "http://localhost:5088/api/User/$($contact.id)" `
  -Method Delete `
  -Headers @{ Authorization = "Bearer $($auth.accessToken)" }
```

O RabbitMQ pode ser acompanhado pelo painel:

```text
http://localhost:15672
User: guest
Password: guest
```

Para usar SMTP real em vez de Mailpit, crie um arquivo `.env` a partir de `.env.example` e ajuste as variaveis `CORE_FLOW_SMTP_*`.

Exemplo com Gmail SMTP:

```env
CORE_FLOW_SMTP_HOST=smtp.gmail.com
CORE_FLOW_SMTP_PORT=587
CORE_FLOW_SMTP_ENABLE_SSL=true
CORE_FLOW_SMTP_USERNAME=gmarcone@gmail.com
CORE_FLOW_SMTP_PASSWORD=SUA_APP_PASSWORD_DO_GMAIL
CORE_FLOW_SMTP_FROM_ADDRESS=gmarcone@gmail.com
CORE_FLOW_SMTP_FROM_NAME=CoreFlow Agenda
```

Importante: para Gmail, use uma app password do Google. A senha normal da conta nao funciona para SMTP.

Tambem observe que o e-mail do usuario logado vem das claims do JWT. Se voce fizer login com o usuario seedado `admin@coreflow.local`, o e-mail do usuario logado sera enviado para `admin@coreflow.local`. Para receber essa notificacao em `gmarcone@gmail.com`, faca login com um usuario cujo e-mail seja `gmarcone@gmail.com` ou altere o usuario autenticado no banco.

### Comandos Docker uteis

```powershell
docker ps
docker logs coreflow_api
docker logs coreflow_worker
docker logs coreflow_rabbitmq
docker logs coreflow_mailpit
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

Para rodar o Worker fora do Docker, mantenha RabbitMQ e Mailpit ativos pelo Docker Compose e execute:

```powershell
dotnet run --project CoreFlow.Worker/CoreFlow.Worker.csproj
```

O Worker local usa `CoreFlow.Worker/appsettings.json`, que aponta para:

```text
RabbitMQ: localhost:5672
SMTP/Mailpit: localhost:1025
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
PATCH  /api/User/me/password
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

Payload para troca da propria senha:

```json
{
  "currentPassword": "Admin@123456",
  "newPassword": "User@123456"
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
- `CoreFlow.Infrastructure/Migrations/20260516_AddUserUpdatedAt.cs`
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

Eventos:

- `ContactChangedEvent`
- `IContactEventPublisher`
- `RabbitMqContactEventPublisher`

O `CreateUserHandler` publica evento de contato criado, o `UpdateUserHandler` publica evento de contato atualizado e o `DeleteUserHandler` publica evento de contato removido. O `CoreFlow.Worker` consome esses eventos e envia e-mail para o usuario logado e para o contato afetado.

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

- `CoreFlow.Tests/UserTests.cs`: valida entidade, ordenacao por contato criado/editado mais recentemente e preservacao de `CreatedAt`.
- `CoreFlow.Tests/AuthTests.cs`: valida hash de senha e autenticacao em memoria.
- `CoreFlow.Tests/UserCommandValidatorTests.cs`: valida telefone, senha e validators de comandos.
- `CoreFlow.Tests/ContactEventPublisherTests.cs`: valida publicacao de eventos ao criar, editar e remover contatos.

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
