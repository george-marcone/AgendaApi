using CoreFlow.Domain.Entities;
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
}
