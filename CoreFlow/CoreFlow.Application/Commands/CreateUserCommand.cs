using MediatR;
using CoreFlow.Domain.Entities;

namespace CoreFlow.Application.Commands;

public record CreateUserCommand(string Name, string Email, string Phone, string Password) : IRequest<Guid>;
