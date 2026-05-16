namespace CoreFlow.Infrastructure.Messaging;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public bool Enabled { get; init; } = true;
    public string HostName { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string UserName { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string VirtualHost { get; init; } = "/";
    public string ExchangeName { get; init; } = "coreflow.contacts";
    public string QueueName { get; init; } = "coreflow.contact.email-notifications";
    public string RoutingKey { get; init; } = "contact.changed";
    public int RetryDelaySeconds { get; init; } = 5;
    public ushort PrefetchCount { get; init; } = 5;
}
