using CoreFlow.Application.Interfaces;
using CoreFlow.Domain.Entities;

namespace CoreFlow.Infrastructure.Services;

public class InMemoryUserService : IUserService
{
    private readonly List<User> _items = new();

    public Task AddAsync(User user)
    {
        var now = DateTimeOffset.UtcNow;
        var createdAt = user.CreatedAt == default ? now : user.CreatedAt;
        var updatedAt = user.UpdatedAt == default ? createdAt : user.UpdatedAt;

        _items.Add(user with
        {
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        });

        return Task.CompletedTask;
    }

    public Task<User[]> GetAllAsync()
    {
        var users = _items
            .OrderByDescending(x => x.UpdatedAt)
            .ThenByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .ToArray();

        return Task.FromResult(users);
    }

    public Task<User?> GetByIdAsync(Guid id)
    {
        var p = _items.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(p);
    }

    public Task<bool> EmailExistsAsync(string email, Guid? ignoredUserId = null, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim();
        var exists = _items.Any(x =>
            string.Equals(x.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase)
            && (!ignoredUserId.HasValue || x.Id != ignoredUserId.Value));

        return Task.FromResult(exists);
    }

    public Task<bool> PhoneExistsAsync(string phone, Guid? ignoredUserId = null, CancellationToken cancellationToken = default)
    {
        var normalizedPhone = phone.Trim();
        var exists = _items.Any(x =>
            string.Equals(x.Phone, normalizedPhone, StringComparison.OrdinalIgnoreCase)
            && (!ignoredUserId.HasValue || x.Id != ignoredUserId.Value));

        return Task.FromResult(exists);
    }

    public Task UpdateAsync(User user)
    {
        var idx = _items.FindIndex(x => x.Id == user.Id);
        if (idx >= 0)
        {
            _items[idx] = _items[idx] with
            {
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                UpdatedAt = DateTimeOffset.UtcNow
            };
        }

        return Task.CompletedTask;
    }

    public Task UpdatePasswordHashAsync(Guid id, string passwordHash)
    {
        var idx = _items.FindIndex(x => x.Id == id);
        if (idx >= 0)
        {
            _items[idx] = _items[idx] with { PasswordHash = passwordHash };
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _items.RemoveAll(x => x.Id == id);
        return Task.CompletedTask;
    }
}
