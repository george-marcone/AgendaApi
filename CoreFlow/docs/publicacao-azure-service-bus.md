# Publicacao na Azure com Azure Service Bus

Este documento descreve a branch `feature/azure-service-bus`, que troca o broker de producao para Azure Service Bus sem remover o RabbitMQ usado no Docker local.

## Arquitetura alvo

```text
AgendaFront
  -> Azure Static Web Apps

CoreFlow.API
  -> Azure Container Apps
  -> publica ContactChangedEvent no Azure Service Bus

Azure Service Bus
  -> queue coreflow-contact-email-notifications

CoreFlow.Worker
  -> Azure Container Apps sem ingress publico
  -> escala por quantidade de mensagens na fila
  -> envia e-mails via SMTP/Mailtrap
```

## Providers de mensagens

A aplicacao agora usa `Messaging:Provider`:

| Valor | Uso |
| --- | --- |
| `RabbitMq` | Desenvolvimento local e Docker Compose. |
| `AzureServiceBus` | Producao na Azure. |
| `None` | Desativa a publicacao de eventos. |

O Docker local continua usando RabbitMQ:

```yaml
Messaging__Provider: "RabbitMq"
```

Na Azure, API e Worker devem usar:

```text
Messaging__Provider=AzureServiceBus
AzureServiceBus__ConnectionString=<connection string do Service Bus>
AzureServiceBus__QueueName=coreflow-contact-email-notifications
```

## Recursos Azure

Recursos recomendados:

| Recurso | Sugestao |
| --- | --- |
| Frontend | Azure Static Web Apps Free. |
| API | Azure Container Apps, Consumption, ingress externo. |
| Worker | Azure Container Apps, sem ingress, escala por Azure Service Bus. |
| Fila | Azure Service Bus Queue. |
| Banco | Para demo, `Storage__Provider=InMemory`; para dados reais, Azure SQL Database Free Offer ou SQL pago. |
| Email | Mailtrap SMTP ou outro SMTP real. |

## Variaveis da API

```text
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080
ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
Storage__Provider=InMemory
Messaging__Provider=AzureServiceBus
AzureServiceBus__ConnectionString=<secret>
AzureServiceBus__QueueName=coreflow-contact-email-notifications
Jwt__Issuer=CoreFlow.Api
Jwt__Audience=CoreFlow.Frontend
Jwt__Key=<secret com pelo menos 32 bytes>
Jwt__ExpiresMinutes=60
Cors__AllowedOrigins__0=https://<seu-front>.azurestaticapps.net
```

## Variaveis do Worker

```text
DOTNET_ENVIRONMENT=Production
Messaging__Provider=AzureServiceBus
AzureServiceBus__ConnectionString=<secret>
AzureServiceBus__QueueName=coreflow-contact-email-notifications
AzureServiceBus__MaxConcurrentCalls=5
AzureServiceBus__PrefetchCount=5
Email__Host=sandbox.smtp.mailtrap.io
Email__Port=2525
Email__EnableSsl=false
Email__UserName=<usuario Mailtrap>
Email__Password=<senha Mailtrap>
Email__FromAddress=noreply@coreflow.local
Email__FromName=CoreFlow Agenda
```

## Exemplo com Azure CLI

> Ajuste nomes, regiao, imagens e secrets antes de executar.
> As imagens `:latest` abaixo assumem que a branch foi mergeada em `main` e que o workflow Docker Publish publicou novos pacotes no GHCR. Para publicar direto desta branch, rode o workflow manualmente em `feature/azure-service-bus` e use a tag gerada para a branch, por exemplo `ghcr.io/george-marcone/agendaapi-api:feature-azure-service-bus`.

```powershell
$RG="rg-agenda"
$LOCATION="eastus"
$ENV="agenda-env"
$SERVICEBUS_NAMESPACE="agenda-sb-$((Get-Random))"
$QUEUE="coreflow-contact-email-notifications"
$API_APP="agendaapi-api"
$WORKER_APP="agendaapi-worker"

az group create --name $RG --location $LOCATION
az servicebus namespace create --resource-group $RG --name $SERVICEBUS_NAMESPACE --location $LOCATION --sku Basic
az servicebus queue create --resource-group $RG --namespace-name $SERVICEBUS_NAMESPACE --name $QUEUE

$SERVICEBUS_CONNECTION = az servicebus namespace authorization-rule keys list `
  --resource-group $RG `
  --namespace-name $SERVICEBUS_NAMESPACE `
  --name RootManageSharedAccessKey `
  --query primaryConnectionString `
  --output tsv

az containerapp env create --name $ENV --resource-group $RG --location $LOCATION
```

API:

```powershell
az containerapp create `
  --name $API_APP `
  --resource-group $RG `
  --environment $ENV `
  --image ghcr.io/george-marcone/agendaapi-api:latest `
  --target-port 8080 `
  --ingress external `
  --min-replicas 0 `
  --max-replicas 1 `
  --secrets "servicebus-connection-string=$SERVICEBUS_CONNECTION" "jwt-key=<jwt-secret>" `
  --env-vars `
    "ASPNETCORE_ENVIRONMENT=Production" `
    "ASPNETCORE_URLS=http://0.0.0.0:8080" `
    "ASPNETCORE_FORWARDEDHEADERS_ENABLED=true" `
    "Storage__Provider=InMemory" `
    "Messaging__Provider=AzureServiceBus" `
    "AzureServiceBus__ConnectionString=secretref:servicebus-connection-string" `
    "AzureServiceBus__QueueName=$QUEUE" `
    "Jwt__Issuer=CoreFlow.Api" `
    "Jwt__Audience=CoreFlow.Frontend" `
    "Jwt__Key=secretref:jwt-key" `
    "Jwt__ExpiresMinutes=60"
```

Worker com escala por fila:

```powershell
az containerapp create `
  --name $WORKER_APP `
  --resource-group $RG `
  --environment $ENV `
  --image ghcr.io/george-marcone/agendaapi-worker:latest `
  --min-replicas 0 `
  --max-replicas 1 `
  --secrets "servicebus-connection-string=$SERVICEBUS_CONNECTION" "smtp-password=<mailtrap-password>" `
  --env-vars `
    "DOTNET_ENVIRONMENT=Production" `
    "Messaging__Provider=AzureServiceBus" `
    "AzureServiceBus__ConnectionString=secretref:servicebus-connection-string" `
    "AzureServiceBus__QueueName=$QUEUE" `
    "Email__Host=sandbox.smtp.mailtrap.io" `
    "Email__Port=2525" `
    "Email__EnableSsl=false" `
    "Email__UserName=<mailtrap-user>" `
    "Email__Password=secretref:smtp-password" `
    "Email__FromAddress=noreply@coreflow.local" `
    "Email__FromName=CoreFlow Agenda" `
  --scale-rule-name azure-servicebus-queue-rule `
  --scale-rule-type azure-servicebus `
  --scale-rule-metadata "queueName=$QUEUE" "namespace=$SERVICEBUS_NAMESPACE" "messageCount=1" `
  --scale-rule-auth "connection=servicebus-connection-string"
```

## Custos e limites

Azure Static Web Apps tem plano Free para o frontend. Azure Container Apps Consumption tem franquia mensal gratuita de consumo e requisicoes, mas pode cobrar se passar dos limites. O Worker deve ficar com `min-replicas 0` e regra de escala por Azure Service Bus para evitar replica sempre ligada.

O Azure Service Bus nao e um substituto gratuito perfeito do RabbitMQ. Ele e a opcao mais natural dentro da Azure, mas deve ser monitorado com Budget/Cost Management.

## Validacao

Depois do deploy:

```powershell
az containerapp show --name $API_APP --resource-group $RG --query properties.configuration.ingress.fqdn --output tsv
az containerapp logs show --name $API_APP --resource-group $RG --follow
az containerapp logs show --name $WORKER_APP --resource-group $RG --follow
```

Teste:

```text
https://<fqdn-da-api>/swagger
```

Ao cadastrar, editar ou remover contato, a API publica um evento na fila e o Worker envia os e-mails via SMTP.
