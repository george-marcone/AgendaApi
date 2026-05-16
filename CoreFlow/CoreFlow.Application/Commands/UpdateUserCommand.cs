using MediatR;
using CoreFlow.Application.Events;

namespace CoreFlow.Application.Commands;

public record UpdateUserCommand(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    ContactEventActor? Actor = null) : IRequest<Unit>;
