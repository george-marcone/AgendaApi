using MediatR;
using CoreFlow.Application.Queries;
using CoreFlow.Application.Interfaces;
using CoreFlow.Domain.Entities;

namespace CoreFlow.Application.Handlers;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, User?>
{
    private readonly IUserService _service;

    public GetUserByIdHandler(IUserService service) => _service = service;

    public Task<User?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        return _service.GetByIdAsync(request.Id);
    }
}
