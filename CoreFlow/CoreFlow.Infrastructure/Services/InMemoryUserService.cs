using CoreFlow.Application.Interfaces;
using CoreFlow.Domain.Entities;

namespace CoreFlow.Infrastructure.Services;

public class InMemoryUserService : IUserService
{
    private readonly List<User> _items = new();

    public Task AddAsync(User user)
    {
        _items.Add(user);
        return Task.CompletedTask;
    }

    public Task<User[]> GetAllAsync()
    {
        return Task.FromResult(_items.ToArray());
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
        if (idx >= 0) _items[idx] = user;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _items.RemoveAll(x => x.Id == id);
        return Task.CompletedTask;
    }
}
