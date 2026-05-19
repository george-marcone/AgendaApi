using CoreFlow.Application.Events;
using CoreFlow.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CoreFlow.Infrastructure.Messaging;

public class DisabledContactEventPublisher : IContactEventPublisher
{
    private readonly ILogger<DisabledContactEventPublisher> _logger;

    public DisabledContactEventPublisher(ILogger<DisabledContactEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync(ContactChangedEvent contactEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Contact notification publishing is disabled. Skipping {EventType} event for contact {ContactId}.",
            contactEvent.EventType,
            contactEvent.Id);
        return Task.CompletedTask;
    }
}
