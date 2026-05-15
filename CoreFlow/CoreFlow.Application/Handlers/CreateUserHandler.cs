using MediatR;
using CoreFlow.Application.Commands;
using CoreFlow.Domain.Entities;
using CoreFlow.Application.Interfaces;

namespace CoreFlow.Application.Handlers;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserService _service;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserHandler(IUserService service, IPasswordHasher passwordHasher)
    {
        _service = service;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var p = new User
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        await _service.AddAsync(p);
        return p.Id;
    }
}
