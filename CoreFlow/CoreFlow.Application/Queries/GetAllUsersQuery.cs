using MediatR;
using CoreFlow.Domain.Entities;

namespace CoreFlow.Application.Queries;

public record GetAllUsersQuery() : IRequest<User[]>;
