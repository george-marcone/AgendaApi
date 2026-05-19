namespace CoreFlow.Infrastructure.Messaging;

public static class MessagingProviders
{
    public const string None = "None";
    public const string RabbitMq = "RabbitMq";
    public const string AzureServiceBus = "AzureServiceBus";

    public static bool Is(string? provider, string expected) =>
        string.Equals(provider, expected, StringComparison.OrdinalIgnoreCase);
}

public class MessagingOptions
{
    public const string SectionName = "Messaging";

    public string Provider { get; init; } = MessagingProviders.RabbitMq;
}
