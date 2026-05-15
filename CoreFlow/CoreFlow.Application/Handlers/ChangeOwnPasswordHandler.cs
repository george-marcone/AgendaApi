using CoreFlow.Application.Commands;
using CoreFlow.Application.Interfaces;
using MediatR;

namespace CoreFlow.Application.Handlers;

public class ChangeOwnPasswordHandler
    : IRequestHandler<ChangeOwnPasswordCommand, ChangeOwnPasswordResult>
{
    private readonly IUserService _userService;
    private readonly IPasswordHasher _passwordHasher;

    public ChangeOwnPasswordHandler(IUserService userService, IPasswordHasher passwordHasher)
    {
        _userService = userService;
        _passwordHasher = passwordHasher;
    }

    public async Task<ChangeOwnPasswordResult> Handle(
        ChangeOwnPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(request.UserId);

        if (user is null)
        {
            return ChangeOwnPasswordResult.UserNotFound;
        }

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return ChangeOwnPasswordResult.InvalidCurrentPassword;
        }

        await _userService.UpdatePasswordHashAsync(request.UserId, _passwordHasher.Hash(request.NewPassword));

        return ChangeOwnPasswordResult.Updated;
    }
}
