using MediatR;
using CoreFlow.Domain.Entities;

namespace CoreFlow.Application.Commands;

public record UpdateUserCommand(Guid Id, string Name, string Email, string Phone) : IRequest;
