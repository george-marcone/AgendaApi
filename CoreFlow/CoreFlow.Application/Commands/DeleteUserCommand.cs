using MediatR;

namespace CoreFlow.Application.Commands;

public record DeleteUserCommand(Guid Id) : IRequest<Unit>;
