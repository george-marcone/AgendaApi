using CoreFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreFlow.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<AuthUser> AuthUsers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(u => u.Id);
            b.Property(u => u.Name).IsRequired().HasMaxLength(200);
            b.Property(u => u.Email).IsRequired().HasMaxLength(200);
            b.Property(u => u.Phone).IsRequired().HasMaxLength(50);
            b.Property(u => u.CreatedAt).IsRequired().HasColumnType("datetimeoffset");
            b.HasIndex(u => u.Email).IsUnique();
            b.HasIndex(u => u.Phone).IsUnique();
            b.HasIndex(u => u.CreatedAt);
        });

        modelBuilder.Entity<AuthUser>(b =>
        {
            b.HasKey(u => u.Id);
            b.Property(u => u.Name).IsRequired().HasMaxLength(200);
            b.Property(u => u.Email).IsRequired().HasMaxLength(200);
            b.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
            b.Property(u => u.CreatedAt).IsRequired().HasColumnType("datetimeoffset");
            b.HasIndex(u => u.Email).IsUnique();
        });
    }
}
