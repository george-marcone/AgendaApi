using CoreFlow.Application.Interfaces;
using CoreFlow.Domain.Entities;
using CoreFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoreFlow.Infrastructure.Services;

public class EfUserService : IUserService
{
    private readonly AppDbContext _db;

    public EfUserService(AppDbContext db) => _db = db;

    public async Task AddAsync(User user)
    {
        var now = DateTimeOffset.UtcNow;
        var createdAt = user.CreatedAt == default ? now : user.CreatedAt;
        var updatedAt = user.UpdatedAt == default ? createdAt : user.UpdatedAt;

        _db.Users.Add(user with
        {
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        });

        await _db.SaveChangesAsync();
    }

    public async Task<User[]> GetAllAsync()
    {
        return await _db.Users
            .AsNoTracking()
            .OrderByDescending(x => x.UpdatedAt)
            .ThenByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .ToArrayAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> EmailExistsAsync(string email, Guid? ignoredUserId = null, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim();
        return await _db.Users
            .AsNoTracking()
            .AnyAsync(x => x.Email == normalizedEmail && (!ignoredUserId.HasValue || x.Id != ignoredUserId.Value), cancellationToken);
    }

    public async Task<bool> PhoneExistsAsync(string phone, Guid? ignoredUserId = null, CancellationToken cancellationToken = default)
    {
        var normalizedPhone = phone.Trim();
        return await _db.Users
            .AsNoTracking()
            .AnyAsync(x => x.Phone == normalizedPhone && (!ignoredUserId.HasValue || x.Id != ignoredUserId.Value), cancellationToken);
    }

    public async Task UpdateAsync(User user)
    {
        var existing = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == user.Id);
        if (existing is null) return;

        _db.Users.Update(existing with
        {
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    public async Task UpdatePasswordHashAsync(Guid id, string passwordHash)
    {
        var existing = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (existing is null) return;

        _db.Users.Update(existing with { PasswordHash = passwordHash });
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _db.Users.FindAsync(id);
        if (entity is null) return;
        _db.Users.Remove(entity);
        await _db.SaveChangesAsync();
    }
}
