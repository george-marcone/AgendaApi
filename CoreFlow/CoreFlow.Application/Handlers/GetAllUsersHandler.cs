using MediatR;
using CoreFlow.Application.Queries;
using CoreFlow.Application.Interfaces;
using CoreFlow.Domain.Entities;

namespace CoreFlow.Application.Handlers;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, User[]>
{
    private readonly IUserService _service;

    public GetAllUsersHandler(IUserService service) => _service = service;

    public Task<User[]> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        return _service.GetAllAsync();
    }
}
