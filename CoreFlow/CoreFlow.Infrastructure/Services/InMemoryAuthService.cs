using CoreFlow.Application.Interfaces;
using CoreFlow.Domain.Entities;
using CoreFlow.Infrastructure.Security;

namespace CoreFlow.Infrastructure.Services;

public class InMemoryAuthService : IAuthService
{
    private readonly List<AuthUser> _items = new();
    private readonly IPasswordHasher _passwordHasher;

    public InMemoryAuthService(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public Task<AuthUser> CreateAsync(string name, string email, string password, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        if (_items.Any(x => x.Email == normalizedEmail))
        {
            throw new InvalidOperationException("Auth user email already exists.");
        }

        var user = new AuthUser
        {
            Name = name.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(password)
        };

        _items.Add(user);
        return Task.FromResult(user);
    }

    public Task<AuthUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = _items.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(user);
    }

    public Task<AuthUser?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = _items.FirstOrDefault(x => x.Email == normalizedEmail);
        if (user is null || !_passwordHasher.Verify(password, user.PasswordHash))
        {
            return Task.FromResult<AuthUser?>(null);
        }

        return Task.FromResult<AuthUser?>(user);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
