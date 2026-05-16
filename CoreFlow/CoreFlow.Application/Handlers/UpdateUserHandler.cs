using MediatR;
using CoreFlow.Application.Commands;
using CoreFlow.Application.Events;
using CoreFlow.Application.Interfaces;

namespace CoreFlow.Application.Handlers;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Unit>
{
    private readonly IUserService _service;
    private readonly IContactEventPublisher _contactEventPublisher;

    public UpdateUserHandler(IUserService service, IContactEventPublisher contactEventPublisher)
    {
        _service = service;
        _contactEventPublisher = contactEventPublisher;
    }

    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _service.GetByIdAsync(request.Id);
        if (existingUser is null)
        {
            return Unit.Value;
        }

        var user = existingUser with
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone
        };

        await _service.UpdateAsync(user);
        await _contactEventPublisher.PublishAsync(
            ContactChangedEvent.FromUser(user, ContactEventType.Updated, request.Actor),
            cancellationToken);

        return Unit.Value;
    }
}
