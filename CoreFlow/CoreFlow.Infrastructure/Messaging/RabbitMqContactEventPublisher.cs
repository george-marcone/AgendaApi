using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoreFlow.Application.Events;
using CoreFlow.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CoreFlow.Infrastructure.Messaging;

public class RabbitMqContactEventPublisher : IContactEventPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqContactEventPublisher> _logger;

    public RabbitMqContactEventPublisher(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqContactEventPublisher> logger)
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

        try
        {
            await Task.Run(() => Publish(contactEvent), cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to publish {EventType} notification event for contact {ContactId}.",
                contactEvent.EventType,
                contactEvent.Id);
        }
    }

    private void Publish(ContactChangedEvent contactEvent)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(
            exchange: _options.ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        channel.QueueDeclare(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        channel.QueueBind(
            queue: _options.QueueName,
            exchange: _options.ExchangeName,
            routingKey: _options.RoutingKey);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(contactEvent, JsonOptions));
        var properties = channel.CreateBasicProperties();
        properties.ContentType = "application/json";
        properties.DeliveryMode = 2;
        properties.Type = contactEvent.EventType.ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        channel.BasicPublish(
            exchange: _options.ExchangeName,
            routingKey: _options.RoutingKey,
            basicProperties: properties,
            body: body);

        _logger.LogInformation(
            "Published {EventType} notification event for contact {ContactId}.",
            contactEvent.EventType,
            contactEvent.Id);
    }
}
