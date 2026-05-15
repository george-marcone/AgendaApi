using CoreFlow.Domain.Entities;

namespace CoreFlow.Application.Interfaces;

public interface IAuthService
{
    Task<User> CreateAsync(string name, string email, string phone, string password, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);
}
