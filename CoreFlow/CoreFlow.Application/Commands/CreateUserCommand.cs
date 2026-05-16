using MediatR;
using CoreFlow.Application.Events;

namespace CoreFlow.Application.Commands;

public record CreateUserCommand(
    string Name,
    string Email,
    string Phone,
    string Password,
    ContactEventActor? Actor = null) : IRequest<Guid>;
