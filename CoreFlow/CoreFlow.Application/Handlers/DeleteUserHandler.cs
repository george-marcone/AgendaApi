using MediatR;
using CoreFlow.Application.Commands;
using CoreFlow.Application.Events;
using CoreFlow.Application.Interfaces;

namespace CoreFlow.Application.Handlers;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly IUserService _service;
    private readonly IContactEventPublisher _contactEventPublisher;

    public DeleteUserHandler(IUserService service, IContactEventPublisher contactEventPublisher)
    {
        _service = service;
        _contactEventPublisher = contactEventPublisher;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _service.GetByIdAsync(request.Id);
        if (user is null)
        {
            return Unit.Value;
        }

        await _service.DeleteAsync(request.Id);
        await _contactEventPublisher.PublishAsync(
            ContactChangedEvent.FromUser(user, ContactEventType.Deleted, request.Actor),
            cancellationToken);

        return Unit.Value;
    }
}
