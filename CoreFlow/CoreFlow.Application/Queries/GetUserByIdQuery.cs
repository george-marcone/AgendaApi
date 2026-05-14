using MediatR;
using CoreFlow.Domain.Entities;

namespace CoreFlow.Application.Queries;

public record GetUserByIdQuery(Guid Id) : IRequest<User?>;
