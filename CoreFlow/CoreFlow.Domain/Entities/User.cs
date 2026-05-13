namespace CoreFlow.Domain.Entities;

public record User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; init; } = string.Empty;
}
