using CoreFlow.Domain.Entities;

namespace CoreFlow.Application.Interfaces;

public interface IUserService
{
    Task<User[]> GetAllAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task<bool> EmailExistsAsync(string email, Guid? ignoredUserId = null, CancellationToken cancellationToken = default);
    Task<bool> PhoneExistsAsync(string phone, Guid? ignoredUserId = null, CancellationToken cancellationToken = default);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task UpdatePasswordHashAsync(Guid id, string passwordHash);
    Task DeleteAsync(Guid id);
}
