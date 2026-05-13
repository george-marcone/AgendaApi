using MediatR;
using CoreFlow.Application.Commands;
using CoreFlow.Domain.Entities;
using CoreFlow.Application.Interfaces;

namespace CoreFlow.Application.Handlers;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserService _service;

    public CreateUserHandler(IUserService service) => _service = service;

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var p = new User { Name = request.Name };
        await _service.AddAsync(p);
        return p.Id;
    }
}
