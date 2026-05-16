using CoreFlow.Domain.Entities;

namespace CoreFlow.Application.Events;

public enum ContactEventType
{
    Created,
    Updated,
    Deleted
}

public record ContactEventActor(Guid Id, string Name, string Email)
{
    public bool HasEmail => !string.IsNullOrWhiteSpace(Email);

    public static ContactEventActor Unknown { get; } = new(Guid.Empty, "Usuario autenticado", string.Empty);
}

public record ContactChangedEvent(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    ContactEventActor Actor,
    ContactEventType EventType,
    DateTimeOffset OccurredAt)
{
    public static ContactChangedEvent FromUser(
        User user,
        ContactEventType eventType,
        ContactEventActor? actor = null,
        DateTimeOffset? occurredAt = null)
    {
        return new ContactChangedEvent(
            user.Id,
            user.Name,
            user.Email,
            user.Phone,
            actor ?? ContactEventActor.Unknown,
            eventType,
            occurredAt ?? DateTimeOffset.UtcNow);
    }
}
