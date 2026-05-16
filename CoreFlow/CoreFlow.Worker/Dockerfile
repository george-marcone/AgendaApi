FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CoreFlow.Application/CoreFlow.Application.csproj CoreFlow.Application/
COPY CoreFlow.Domain/CoreFlow.Domain.csproj CoreFlow.Domain/
COPY CoreFlow.Infrastructure/CoreFlow.Infrastructure.csproj CoreFlow.Infrastructure/
COPY CoreFlow.Worker/CoreFlow.Worker.csproj CoreFlow.Worker/

RUN dotnet restore CoreFlow.Worker/CoreFlow.Worker.csproj

COPY . .
RUN dotnet publish CoreFlow.Worker/CoreFlow.Worker.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CoreFlow.Worker.dll"]
