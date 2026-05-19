namespace CoreFlow.Infrastructure.Messaging;

public class AzureServiceBusOptions
{
    public const string SectionName = "AzureServiceBus";

    public bool Enabled { get; init; } = true;
    public string ConnectionString { get; init; } = string.Empty;
    public string QueueName { get; init; } = "coreflow-contact-email-notifications";
    public string Subject { get; init; } = "contact.changed";
    public string ContentType { get; init; } = "application/json";
    public int RetryDelaySeconds { get; init; } = 5;
    public int MaxConcurrentCalls { get; init; } = 5;
    public int PrefetchCount { get; init; } = 5;
}
