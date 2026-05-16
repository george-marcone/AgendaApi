using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoreFlow.Application.Events;
using CoreFlow.Infrastructure.Messaging;
using CoreFlow.Worker.Email;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace CoreFlow.Worker.Messaging;

public class ContactEmailNotificationWorker : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly RabbitMqOptions _options;
    private readonly SmtpEmailSender _emailSender;
    private readonly ILogger<ContactEmailNotificationWorker> _logger;

    public ContactEmailNotificationWorker(
        IOptions<RabbitMqOptions> options,
        SmtpEmailSender emailSender,
        ILogger<ContactEmailNotificationWorker> logger)
    {
        _options = options.Value;
        _emailSender = emailSender;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("RabbitMQ notifications are disabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (BrokerUnreachableException exception)
            {
                _logger.LogWarning(
                    exception,
                    "RabbitMQ is not reachable. Retrying in {RetryDelaySeconds} seconds.",
                    _options.RetryDelaySeconds);
                await DelayBeforeRetryAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Contact email notification worker stopped unexpectedly. Retrying in {RetryDelaySeconds} seconds.",
                    _options.RetryDelaySeconds);
                await DelayBeforeRetryAsync(stoppingToken);
            }
        }
    }

    private async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            DispatchConsumersAsync = true
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        DeclareTopology(channel);
        channel.BasicQos(prefetchSize: 0, prefetchCount: _options.PrefetchCount, global: false);

        var shutdown = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.ConnectionShutdown += (_, _) => shutdown.TrySetResult();
        channel.ModelShutdown += (_, _) => shutdown.TrySetResult();

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, eventArgs) =>
        {
            await HandleMessageAsync(channel, eventArgs, stoppingToken);
        };

        channel.BasicConsume(
            queue: _options.QueueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation(
            "Listening for contact notification events on RabbitMQ queue {QueueName}.",
            _options.QueueName);

        await Task.WhenAny(Task.Delay(Timeout.Infinite, stoppingToken), shutdown.Task);
    }

    private void DeclareTopology(IModel channel)
    {
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
    }

    private async Task HandleMessageAsync(
        IModel channel,
        BasicDeliverEventArgs eventArgs,
        CancellationToken cancellationToken)
    {
        try
        {
            var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            var contactEvent = JsonSerializer.Deserialize<ContactChangedEvent>(json, JsonOptions)
                ?? throw new InvalidOperationException("Contact event message is empty.");

            await _emailSender.SendContactNotificationAsync(contactEvent, cancellationToken);
            channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to process contact notification message {DeliveryTag}.",
                eventArgs.DeliveryTag);

            channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
        }
    }

    private Task DelayBeforeRetryAsync(CancellationToken stoppingToken)
    {
        var delay = TimeSpan.FromSeconds(Math.Max(1, _options.RetryDelaySeconds));
        return Task.Delay(delay, stoppingToken);
    }
}
