using CoreFlow.Application.Interfaces;
using CoreFlow.Domain.Entities;
using CoreFlow.Infrastructure.Data;
using CoreFlow.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace CoreFlow.Infrastructure.Services;

public class EfAuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;

    public EfAuthService(AppDbContext db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthUser> CreateAsync(string name, string email, string password, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var existingUser = await _db.AuthUsers
            .AsNoTracking()
            .AnyAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (existingUser)
        {
            throw new InvalidOperationException("Auth user email already exists.");
        }

        var user = new AuthUser
        {
            Name = name.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(password)
        };

        _db.AuthUsers.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        return user;
    }

    public Task<AuthUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _db.AuthUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<AuthUser?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await _db.AuthUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (user is null || !_passwordHasher.Verify(password, user.PasswordHash))
        {
            return null;
        }

        return user;
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
