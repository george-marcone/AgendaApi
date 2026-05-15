using CoreFlow.Application.Commands;
using CoreFlow.Application.Interfaces;
using CoreFlow.Application.Validators;
using CoreFlow.Domain.Entities;

namespace CoreFlow.Tests;

public class UserCommandValidatorTests
{
    [Fact]
    public async Task CreateUserCommandValidator_AcceptsPhoneWithPlusAndThirteenDigits()
    {
        var validator = new CreateUserCommandValidator(new EmptyUserService());
        var command = new CreateUserCommand("George Marcone", "gmarcone@example.com", "+5511900000001", "User@123456");

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("+55 11 900001")]
    [InlineData("5511900000001")]
    [InlineData("+551190000001")]
    public async Task CreateUserCommandValidator_RejectsPhonesOutsideExpectedFormat(string phone)
    {
        var validator = new CreateUserCommandValidator(new EmptyUserService());
        var command = new CreateUserCommand("George Marcone", "gmarcone@example.com", phone, "User@123456");

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateUserCommand.Phone));
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234567")]
    public async Task CreateUserCommandValidator_RejectsInvalidPassword(string password)
    {
        var validator = new CreateUserCommandValidator(new EmptyUserService());
        var command = new CreateUserCommand("George Marcone", "gmarcone@example.com", "+5511900000001", password);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateUserCommand.Password));
    }

    [Fact]
    public async Task UpdateUserCommandValidator_AcceptsPhoneWithPlusAndThirteenDigits()
    {
        var validator = new UpdateUserCommandValidator(new EmptyUserService());
        var command = new UpdateUserCommand(Guid.NewGuid(), "George Marcone", "gmarcone@example.com", "+5511900000001");

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "User@123456", "CurrentPassword")]
    [InlineData("Admin@123456", "", "NewPassword")]
    [InlineData("Admin@123456", "1234567", "NewPassword")]
    public async Task ChangeOwnPasswordCommandValidator_RejectsInvalidPasswords(
        string currentPassword,
        string newPassword,
        string expectedPropertyName)
    {
        var validator = new ChangeOwnPasswordCommandValidator();
        var command = new ChangeOwnPasswordCommand(Guid.NewGuid(), currentPassword, newPassword);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == expectedPropertyName);
    }

    private sealed class EmptyUserService : IUserService
    {
        public Task<User[]> GetAllAsync() => Task.FromResult(Array.Empty<User>());

        public Task<User?> GetByIdAsync(Guid id) => Task.FromResult<User?>(null);

        public Task<bool> EmailExistsAsync(string email, Guid? ignoredUserId = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<bool> PhoneExistsAsync(string phone, Guid? ignoredUserId = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task AddAsync(User user) => Task.CompletedTask;

        public Task UpdateAsync(User user) => Task.CompletedTask;

        public Task UpdatePasswordHashAsync(Guid id, string passwordHash) => Task.CompletedTask;

        public Task DeleteAsync(Guid id) => Task.CompletedTask;
    }
}
