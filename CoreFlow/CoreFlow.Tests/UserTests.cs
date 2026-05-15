using CoreFlow.Domain.Entities;
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
    public async Task GetAllAsync_ReturnsNewestUsersFirst()
    {
        var service = new InMemoryUserService();
        var olderId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var newerId = Guid.Parse("00000000-0000-0000-0000-000000000002");

        await service.AddAsync(new User
        {
            Id = olderId,
            Name = "Older",
            Email = "older@example.com",
            Phone = "+5511900000001",
            CreatedAt = new DateTimeOffset(2026, 5, 14, 10, 0, 0, TimeSpan.Zero)
        });

        await service.AddAsync(new User
        {
            Id = newerId,
            Name = "Newer",
            Email = "newer@example.com",
            Phone = "+5511900000002",
            CreatedAt = new DateTimeOffset(2026, 5, 15, 10, 0, 0, TimeSpan.Zero)
        });

        var users = await service.GetAllAsync();

        Assert.Equal(new[] { newerId, olderId }, users.Select(x => x.Id));
    }

    [Fact]
    public async Task UpdateAsync_PreservesCreatedAt()
    {
        var service = new InMemoryUserService();
        var id = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var createdAt = new DateTimeOffset(2026, 5, 14, 10, 0, 0, TimeSpan.Zero);

        await service.AddAsync(new User
        {
            Id = id,
            Name = "Original",
            Email = "original@example.com",
            Phone = "+5511900000001",
            CreatedAt = createdAt
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
    }
}
