using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Messaging.ServiceBus;
using CoreFlow.Application.Events;
using CoreFlow.Infrastructure.Messaging;
using CoreFlow.Worker.Email;
using Microsoft.Extensions.Options;

namespace CoreFlow.Worker.Messaging;

public class AzureServiceBusContactEmailNotificationWorker : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly AzureServiceBusOptions _options;
    private readonly SmtpEmailSender _emailSender;
    private readonly ILogger<AzureServiceBusContactEmailNotificationWorker> _logger;

    public AzureServiceBusContactEmailNotificationWorker(
        IOptions<AzureServiceBusOptions> options,
        SmtpEmailSender emailSender,
        ILogger<AzureServiceBusContactEmailNotificationWorker> logger)
    {
        _options = options.Value;
        _emailSender = emailSender;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Azure Service Bus notifications are disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.ConnectionString) ||
            string.IsNullOrWhiteSpace(_options.QueueName))
        {
            _logger.LogError(
                "Azure Service Bus worker is enabled, but ConnectionString or QueueName is not configured.");
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
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Azure Service Bus worker stopped unexpectedly. Retrying in {RetryDelaySeconds} seconds.",
                    _options.RetryDelaySeconds);
                await DelayBeforeRetryAsync(stoppingToken);
            }
        }
    }

    private async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        await using var client = new ServiceBusClient(_options.ConnectionString);
        await using var processor = client.CreateProcessor(
            _options.QueueName,
            new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = Math.Max(1, _options.MaxConcurrentCalls),
                PrefetchCount = Math.Max(0, _options.PrefetchCount)
            });

        processor.ProcessMessageAsync += HandleMessageAsync;
        processor.ProcessErrorAsync += HandleErrorAsync;

        await processor.StartProcessingAsync(stoppingToken);

        _logger.LogInformation(
            "Listening for contact notification events on Azure Service Bus queue {QueueName}.",
            _options.QueueName);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        finally
        {
            await processor.StopProcessingAsync(CancellationToken.None);
        }
    }

    private async Task HandleMessageAsync(ProcessMessageEventArgs eventArgs)
    {
        try
        {
            var json = eventArgs.Message.Body.ToString();
            var contactEvent = JsonSerializer.Deserialize<ContactChangedEvent>(json, JsonOptions)
                ?? throw new InvalidOperationException("Contact event message is empty.");

            _logger.LogInformation(
                "Received Azure Service Bus contact notification message {MessageId}: {EventType} contact {ContactId}, actor {ActorEmail}, contact {ContactEmail}.",
                eventArgs.Message.MessageId,
                contactEvent.EventType,
                contactEvent.Id,
                contactEvent.Actor.Email,
                contactEvent.Email);

            await _emailSender.SendContactNotificationAsync(contactEvent, eventArgs.CancellationToken);
            await eventArgs.CompleteMessageAsync(eventArgs.Message, eventArgs.CancellationToken);

            _logger.LogInformation(
                "Completed Azure Service Bus contact notification message {MessageId} for contact {ContactId}.",
                eventArgs.Message.MessageId,
                contactEvent.Id);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to process Azure Service Bus contact notification message {MessageId}.",
                eventArgs.Message.MessageId);

            await eventArgs.DeadLetterMessageAsync(
                eventArgs.Message,
                "ProcessingFailed",
                TrimDeadLetterDescription(exception.Message),
                eventArgs.CancellationToken);

            _logger.LogWarning(
                "Dead-lettered Azure Service Bus contact notification message {MessageId}.",
                eventArgs.Message.MessageId);
        }
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs eventArgs)
    {
        _logger.LogError(
            eventArgs.Exception,
            "Azure Service Bus processor error. Entity {EntityPath}, source {ErrorSource}, namespace {FullyQualifiedNamespace}.",
            eventArgs.EntityPath,
            eventArgs.ErrorSource,
            eventArgs.FullyQualifiedNamespace);

        return Task.CompletedTask;
    }

    private Task DelayBeforeRetryAsync(CancellationToken stoppingToken)
    {
        var delay = TimeSpan.FromSeconds(Math.Max(1, _options.RetryDelaySeconds));
        return Task.Delay(delay, stoppingToken);
    }

    private static string TrimDeadLetterDescription(string value)
    {
        const int maxLength = 4096;
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
