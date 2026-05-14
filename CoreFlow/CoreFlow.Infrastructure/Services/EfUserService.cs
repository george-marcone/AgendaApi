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
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    public async Task<User[]> GetAllAsync()
    {
        return await _db.Users.AsNoTracking().ToArrayAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task UpdateAsync(User user)
    {
        _db.Users.Update(user);
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
