using MediatR;
using CoreFlow.Application.Events;

namespace CoreFlow.Application.Commands;

public record DeleteUserCommand(Guid Id, ContactEventActor? Actor = null) : IRequest<Unit>;
