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

    public async Task<User> CreateAsync(string name, string email, string phone, string password, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var normalizedPhone = phone.Trim();
        var existingUser = await _db.Users
            .AsNoTracking()
            .AnyAsync(x => x.Email == normalizedEmail || x.Phone == normalizedPhone, cancellationToken);

        if (existingUser)
        {
            throw new InvalidOperationException("User email or phone already exists.");
        }

        var user = new User
        {
            Name = name.Trim(),
            Email = normalizedEmail,
            Phone = normalizedPhone,
            PasswordHash = _passwordHasher.Hash(password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        return user;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<User?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await _db.Users
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
