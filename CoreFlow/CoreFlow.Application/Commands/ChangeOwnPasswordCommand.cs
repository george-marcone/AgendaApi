using MediatR;

namespace CoreFlow.Application.Commands;

public record ChangeOwnPasswordCommand(Guid UserId, string CurrentPassword, string NewPassword)
    : IRequest<ChangeOwnPasswordResult>;

public enum ChangeOwnPasswordResult
{
    Updated,
    UserNotFound,
    InvalidCurrentPassword
}
