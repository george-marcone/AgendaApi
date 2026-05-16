using CoreFlow.Application.Interfaces;
using CoreFlow.Domain.Entities;
using CoreFlow.Infrastructure.Security;

namespace CoreFlow.Infrastructure.Services;

public class InMemoryAuthService : IAuthService
{
    private readonly List<User> _items = new();
    private readonly IPasswordHasher _passwordHasher;

    public InMemoryAuthService(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public Task<User> CreateAsync(string name, string email, string phone, string password, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var normalizedPhone = phone.Trim();
        if (_items.Any(x => x.Email == normalizedEmail || x.Phone == normalizedPhone))
        {
            throw new InvalidOperationException("User email or phone already exists.");
        }

        var now = DateTimeOffset.UtcNow;
        var user = new User
        {
            Name = name.Trim(),
            Email = normalizedEmail,
            Phone = normalizedPhone,
            PasswordHash = _passwordHasher.Hash(password),
            CreatedAt = now,
            UpdatedAt = now
        };

        _items.Add(user);
        return Task.FromResult(user);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = _items.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(user);
    }

    public Task<User?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = _items.FirstOrDefault(x => x.Email == normalizedEmail);
        if (user is null || !_passwordHasher.Verify(password, user.PasswordHash))
        {
            return Task.FromResult<User?>(null);
        }

        return Task.FromResult<User?>(user);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
