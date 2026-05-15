using CoreFlow.Infrastructure.Security;
using CoreFlow.Infrastructure.Services;

namespace CoreFlow.Tests;

public class AuthTests
{
    [Fact]
    public void PasswordHasher_VerifiesValidPassword()
    {
        var passwordHasher = new PasswordHasher();

        var hash = passwordHasher.Hash("Admin@123456");

        Assert.True(passwordHasher.Verify("Admin@123456", hash));
        Assert.False(passwordHasher.Verify("wrong-password", hash));
    }

    [Fact]
    public void PasswordHasher_VerifiesSeededAdminPasswordHash()
    {
        var passwordHasher = new PasswordHasher();
        const string seededHash = "PBKDF2-SHA256.100000.AQIDBAUGBwgJCgsMDQ4PEA==.qcFegJie06o8c1nvLR19oaltyyqxYCeEEOBZYppGVW8=";

        Assert.True(passwordHasher.Verify("Admin@123456", seededHash));
        Assert.False(passwordHasher.Verify("wrong-password", seededHash));
    }

    [Fact]
    public async Task AuthService_ValidatesCredentialsByEmailAndPassword()
    {
        var service = new InMemoryAuthService(new PasswordHasher());
        var user = await service.CreateAsync("Admin", "Admin@CoreFlow.Local", "+5511900000000", "Admin@123456");

        var authenticatedUser = await service.ValidateCredentialsAsync("admin@coreflow.local", "Admin@123456");
        var rejectedUser = await service.ValidateCredentialsAsync("admin@coreflow.local", "wrong-password");

        Assert.NotNull(authenticatedUser);
        Assert.Equal(user.Id, authenticatedUser.Id);
        Assert.Null(rejectedUser);
    }
}
