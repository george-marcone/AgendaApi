using MediatR;
using CoreFlow.Application.Commands;
using CoreFlow.Application.Interfaces;
using CoreFlow.Domain.Entities;

namespace CoreFlow.Application.Handlers;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Unit>
{
    private readonly IUserService _service;

    public UpdateUserHandler(IUserService service) => _service = service;

    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User { Id = request.Id, Name = request.Name, Email = request.Email, Phone = request.Phone };
        await _service.UpdateAsync(user);
        return Unit.Value;
    }
}
