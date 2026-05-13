using CoreFlow.Domain.Entities;

namespace CoreFlow.Application.Interfaces;

public interface IUserService
{
    Task<User[]> GetAllAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);
}
