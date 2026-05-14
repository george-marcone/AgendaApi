using MediatR;
using CoreFlow.Application.Commands;
using CoreFlow.Application.Interfaces;

namespace CoreFlow.Application.Handlers;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserService _service;

    public DeleteUserHandler(IUserService service) => _service = service;

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(request.Id);
        return Unit.Value;
    }
}
