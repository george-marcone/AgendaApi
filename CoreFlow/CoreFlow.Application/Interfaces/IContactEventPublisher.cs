using CoreFlow.Application.Events;

namespace CoreFlow.Application.Interfaces;

public interface IContactEventPublisher
{
    Task PublishAsync(ContactChangedEvent contactEvent, CancellationToken cancellationToken = default);
}
