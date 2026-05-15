using CoreFlow.Domain.Entities;

namespace CoreFlow.Application.Interfaces;

public interface IAuthService
{
    Task<AuthUser> CreateAsync(string name, string email, string password, CancellationToken cancellationToken = default);
    Task<AuthUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AuthUser?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);
}
