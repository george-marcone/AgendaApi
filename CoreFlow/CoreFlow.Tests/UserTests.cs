using CoreFlow.Application.Commands;
using CoreFlow.Application.Handlers;
using CoreFlow.Domain.Entities;
using CoreFlow.Infrastructure.Security;
using CoreFlow.Infrastructure.Services;
using Xunit;

namespace CoreFlow.Tests;

public class UserTests
{
    [Fact]
    public void User_HasDefaultIdAndName()
    {
        var p = new User();
        Assert.NotEqual(Guid.Empty, p.Id);
        Assert.Equal(string.Empty, p.Name);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMostRecentlyCreatedOrUpdatedUsersFirst()
    {
        var service = new InMemoryUserService();
        var olderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var newerId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var olderCreatedAt = new DateTimeOffset(2020, 5, 14, 10, 0, 0, TimeSpan.Zero);
        var newerCreatedAt = new DateTimeOffset(2020, 5, 15, 10, 0, 0, TimeSpan.Zero);

        await service.AddAsync(new User
        {
            Id = olderId,
            Name = "Older",
            Email = "older@example.com",
            Phone = "+5511900000001",
            CreatedAt = olderCreatedAt,
            UpdatedAt = olderCreatedAt
        });

        await service.AddAsync(new User
        {
            Id = newerId,
            Name = "Newer",
            Email = "newer@example.com",
            Phone = "+5511900000002",
            CreatedAt = newerCreatedAt,
            UpdatedAt = newerCreatedAt
        });

        await service.UpdateAsync(new User
        {
            Id = olderId,
            Name = "Older Updated",
            Email = "older.updated@example.com",
            Phone = "+5511900000003"
        });

        var users = await service.GetAllAsync();

        Assert.Equal(new[] { olderId, newerId }, users.Select(x => x.Id));
    }

    [Fact]
    public async Task UpdateAsync_PreservesCreatedAtAndRefreshesUpdatedAt()
    {
        var service = new InMemoryUserService();
        var id = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var createdAt = new DateTimeOffset(2020, 5, 14, 10, 0, 0, TimeSpan.Zero);

        await service.AddAsync(new User
        {
            Id = id,
            Name = "Original",
            Email = "original@example.com",
            Phone = "+5511900000001",
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        });

        await service.UpdateAsync(new User
        {
            Id = id,
            Name = "Updated",
            Email = "updated@example.com",
            Phone = "+5511900000002"
        });

        var user = await service.GetByIdAsync(id);

        Assert.NotNull(user);
        Assert.Equal("Updated", user.Name);
        Assert.Equal(createdAt, user.CreatedAt);
        Assert.True(user.UpdatedAt > createdAt);
    }

    [Fact]
    public async Task ChangeOwnPasswordHandler_UpdatesOnlyAuthenticatedUserPassword()
    {
        var service = new InMemoryUserService();
        var passwordHasher = new PasswordHasher();
        var id = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var createdAt = new DateTimeOffset(2026, 5, 14, 10, 0, 0, TimeSpan.Zero);

        await service.AddAsync(new User
        {
            Id = id,
            Name = "Original",
            Email = "original@example.com",
            Phone = "+5511900000001",
            PasswordHash = passwordHasher.Hash("Admin@123456"),
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        });

        var handler = new ChangeOwnPasswordHandler(service, passwordHasher);
        var result = await handler.Handle(
            new ChangeOwnPasswordCommand(id, "Admin@123456", "User@123456"),
            CancellationToken.None);

        var user = await service.GetByIdAsync(id);

        Assert.Equal(ChangeOwnPasswordResult.Updated, result);
        Assert.NotNull(user);
        Assert.Equal("Original", user.Name);
        Assert.Equal(createdAt, user.CreatedAt);
        Assert.True(passwordHasher.Verify("User@123456", user.PasswordHash));
        Assert.False(passwordHasher.Verify("Admin@123456", user.PasswordHash));
    }

    [Fact]
    public async Task ChangeOwnPasswordHandler_RejectsInvalidCurrentPassword()
    {
        var service = new InMemoryUserService();
        var passwordHasher = new PasswordHasher();
        var id = Guid.Parse("00000000-0000-0000-0000-000000000001");

        await service.AddAsync(new User
        {
            Id = id,
            Name = "Original",
            Email = "original@example.com",
            Phone = "+5511900000001",
            PasswordHash = passwordHasher.Hash("Admin@123456")
        });

        var handler = new ChangeOwnPasswordHandler(service, passwordHasher);
        var result = await handler.Handle(
            new ChangeOwnPasswordCommand(id, "wrong-password", "User@123456"),
            CancellationToken.None);

        var user = await service.GetByIdAsync(id);

        Assert.Equal(ChangeOwnPasswordResult.InvalidCurrentPassword, result);
        Assert.NotNull(user);
        Assert.True(passwordHasher.Verify("Admin@123456", user.PasswordHash));
    }
}
