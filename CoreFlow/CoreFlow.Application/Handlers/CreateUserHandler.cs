using MediatR;
using CoreFlow.Application.Commands;
using CoreFlow.Application.Events;
using CoreFlow.Domain.Entities;
using CoreFlow.Application.Interfaces;

namespace CoreFlow.Application.Handlers;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserService _service;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IContactEventPublisher _contactEventPublisher;

    public CreateUserHandler(
        IUserService service,
        IPasswordHasher passwordHasher,
        IContactEventPublisher contactEventPublisher)
    {
        _service = service;
        _passwordHasher = passwordHasher;
        _contactEventPublisher = contactEventPublisher;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var p = new User
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = _passwordHasher.Hash(request.Password),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _service.AddAsync(p);
        await _contactEventPublisher.PublishAsync(
            ContactChangedEvent.FromUser(p, ContactEventType.Created, request.Actor, p.CreatedAt),
            cancellationToken);

        return p.Id;
    }
}
