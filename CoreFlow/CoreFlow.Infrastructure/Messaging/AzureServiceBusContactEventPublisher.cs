using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Messaging.ServiceBus;
using CoreFlow.Application.Events;
using CoreFlow.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoreFlow.Infrastructure.Messaging;

public class AzureServiceBusContactEventPublisher : IContactEventPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly AzureServiceBusOptions _options;
    private readonly ILogger<AzureServiceBusContactEventPublisher> _logger;

    public AzureServiceBusContactEventPublisher(
        IOptions<AzureServiceBusOptions> options,
        ILogger<AzureServiceBusContactEventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync(ContactChangedEvent contactEvent, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.ConnectionString) ||
            string.IsNullOrWhiteSpace(_options.QueueName))
        {
            _logger.LogError(
                "Azure Service Bus publishing is enabled, but ConnectionString or QueueName is not configured.");
            return;
        }

        try
        {
            await using var client = new ServiceBusClient(_options.ConnectionString);
            await using var sender = client.CreateSender(_options.QueueName);

            var body = JsonSerializer.Serialize(contactEvent, JsonOptions);
            var message = new ServiceBusMessage(BinaryData.FromString(body))
            {
                ContentType = _options.ContentType,
                Subject = _options.Subject,
                MessageId = $"{contactEvent.EventType}-{contactEvent.Id}-{contactEvent.OccurredAt.ToUnixTimeMilliseconds()}"
            };

            message.ApplicationProperties["eventType"] = contactEvent.EventType.ToString();
            message.ApplicationProperties["contactId"] = contactEvent.Id.ToString();
            message.ApplicationProperties["occurredAt"] = contactEvent.OccurredAt.ToString("O");

            await sender.SendMessageAsync(message, cancellationToken);

            _logger.LogInformation(
                "Published {EventType} notification event for contact {ContactId} to Azure Service Bus queue {QueueName}.",
                contactEvent.EventType,
                contactEvent.Id,
                _options.QueueName);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to publish {EventType} notification event for contact {ContactId} to Azure Service Bus queue {QueueName}.",
                contactEvent.EventType,
                contactEvent.Id,
                _options.QueueName);
        }
    }
}
