FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CoreFlow.API/CoreFlow.API.csproj CoreFlow.API/
COPY CoreFlow.Application/CoreFlow.Application.csproj CoreFlow.Application/
COPY CoreFlow.Domain/CoreFlow.Domain.csproj CoreFlow.Domain/
COPY CoreFlow.Infrastructure/CoreFlow.Infrastructure.csproj CoreFlow.Infrastructure/

RUN dotnet restore CoreFlow.API/CoreFlow.API.csproj

COPY . .
RUN dotnet publish CoreFlow.API/CoreFlow.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV PORT=10000
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

EXPOSE 10000

ENTRYPOINT ["dotnet", "CoreFlow.API.dll"]
