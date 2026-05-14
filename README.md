Backend do projeto agenda de contatos em .NET 10 + CQRS + Migrations + Ef Core + Fluent Validations

CoreFlow — Resumo do trabalho
Este repositório contém a solução CoreFlow usada pelo AgendaApi. Abaixo estão os projetos presentes, artefatos importantes e comandos específicos para build, execução e testes.

Projetos
CoreFlow.Domain
CoreFlow.Application
CoreFlow.Infrastructure
CoreFlow.Api
CoreFlow.Tests
Artefatos principais
Domain: CoreFlow.Domain/Entities/User.cs
API: CoreFlow.Api/Controllers/UserController.cs, CoreFlow.Api/Program.cs
Tests: CoreFlow.Tests (xUnit)
Requisitos
.NET 10 SDK instalado (verifique com dotnet --version)
Visual Studio 2026 ou dotnet CLI
Comandos específicos
Execute a partir da raiz do repositório (ex: C:\George Marcone\GitHub\Blue-dev\AgendaApi\CoreFlow):

Restaurar pacotes

dotnet restore

Compilar a solução

dotnet build --no-restore

Executar a Web API (projeto CoreFlow.Api)

dotnet run --project CoreFlow.Api\CoreFlow.Api.csproj

Executar os testes do projeto de testes

dotnet test CoreFlow.Tests\CoreFlow.Tests.csproj

Listar projetos na solution

dotnet sln CoreFlow.slnx list

OBS: se a solução estiver nomeada como CoreFlow.sln em vez de CoreFlow.slnx, ajuste o comando acima.

Comandos úteis
Ver versão do .NET

dotnet --version

Formatar código

dotnet format

Docker (exemplo genérico)
Build da imagem

docker build -t coreflow:latest .

Executar a imagem

docker run --rm -p 5000:80 coreflow:latest

Substitua porta e Dockerfile conforme necessário para seu projeto.

Próximos passos sugeridos
Adicionar um arquivo .gitignore (VisualStudio) e remover bin/, obj/ e .vs/ do repositório.
Extrair repositório (IUserRepository) e integrar EF Core para persistência se necessário.
Adicionar pipelines CI para build e testes automáticos.
Arquivo atualizado para incluir comandos específicos para CoreFlow.Api e CoreFlow.Tests.
