using CoreFlow.Application.Interfaces;
using CoreFlow.Domain.Entities;

namespace CoreFlow.Infrastructure.Services;

public class InMemoryCoreFlowStore : IUserService, IAuthService
{
    private const string SeedPassword = "Admin@123456";
    private readonly object _gate = new();
    private readonly IPasswordHasher _passwordHasher;
    private readonly List<User> _items = new();

    public InMemoryCoreFlowStore(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;
        SeedUsers();
    }

    public Task<User> CreateAsync(
        string name,
        string email,
        string phone,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var normalizedPhone = NormalizePhone(phone);

        lock (_gate)
        {
            if (Exists(normalizedEmail, normalizedPhone))
            {
                throw new InvalidOperationException("User email or phone already exists.");
            }

            var now = DateTimeOffset.UtcNow;
            var user = new User
            {
                Name = name.Trim(),
                Email = normalizedEmail,
                Phone = normalizedPhone,
                PasswordHash = _passwordHasher.Hash(password),
                CreatedAt = now,
                UpdatedAt = now
            };

            _items.Add(user);
            return Task.FromResult(user);
        }
    }

    public Task<User[]> GetAllAsync()
    {
        lock (_gate)
        {
            return Task.FromResult(_items
                .OrderByDescending(x => x.UpdatedAt)
                .ThenByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id)
                .ToArray());
        }
    }

    public Task<User?> GetByIdAsync(Guid id)
    {
        return GetByIdCoreAsync(id);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return GetByIdCoreAsync(id);
    }

    public Task<User?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);

        lock (_gate)
        {
            var user = _items.FirstOrDefault(x => x.Email == normalizedEmail);
            if (user is null || !_passwordHasher.Verify(password, user.PasswordHash))
            {
                return Task.FromResult<User?>(null);
            }

            return Task.FromResult<User?>(user);
        }
    }

    public Task<bool> EmailExistsAsync(
        string email,
        Guid? ignoredUserId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);

        lock (_gate)
        {
            return Task.FromResult(_items.Any(x =>
                x.Email == normalizedEmail &&
                (!ignoredUserId.HasValue || x.Id != ignoredUserId.Value)));
        }
    }

    public Task<bool> PhoneExistsAsync(
        string phone,
        Guid? ignoredUserId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedPhone = NormalizePhone(phone);

        lock (_gate)
        {
            return Task.FromResult(_items.Any(x =>
                string.Equals(x.Phone, normalizedPhone, StringComparison.OrdinalIgnoreCase) &&
                (!ignoredUserId.HasValue || x.Id != ignoredUserId.Value)));
        }
    }

    public Task AddAsync(User user)
    {
        var normalizedEmail = NormalizeEmail(user.Email);
        var normalizedPhone = NormalizePhone(user.Phone);

        lock (_gate)
        {
            if (Exists(normalizedEmail, normalizedPhone))
            {
                throw new InvalidOperationException("User email or phone already exists.");
            }

            var now = DateTimeOffset.UtcNow;
            var createdAt = user.CreatedAt == default ? now : user.CreatedAt;
            var updatedAt = user.UpdatedAt == default ? createdAt : user.UpdatedAt;

            _items.Add(user with
            {
                Email = normalizedEmail,
                Phone = normalizedPhone,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            });
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user)
    {
        var normalizedEmail = NormalizeEmail(user.Email);
        var normalizedPhone = NormalizePhone(user.Phone);

        lock (_gate)
        {
            var index = _items.FindIndex(x => x.Id == user.Id);
            if (index >= 0)
            {
                _items[index] = _items[index] with
                {
                    Name = user.Name.Trim(),
                    Email = normalizedEmail,
                    Phone = normalizedPhone,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
            }
        }

        return Task.CompletedTask;
    }

    public Task UpdatePasswordHashAsync(Guid id, string passwordHash)
    {
        lock (_gate)
        {
            var index = _items.FindIndex(x => x.Id == id);
            if (index >= 0)
            {
                _items[index] = _items[index] with
                {
                    PasswordHash = passwordHash,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
            }
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        lock (_gate)
        {
            _items.RemoveAll(x => x.Id == id);
        }

        return Task.CompletedTask;
    }

    private Task<User?> GetByIdCoreAsync(Guid id)
    {
        lock (_gate)
        {
            return Task.FromResult(_items.FirstOrDefault(x => x.Id == id));
        }
    }

    private bool Exists(string normalizedEmail, string normalizedPhone)
    {
        return _items.Any(x =>
            x.Email == normalizedEmail ||
            string.Equals(x.Phone, normalizedPhone, StringComparison.OrdinalIgnoreCase));
    }

    private void SeedUsers()
    {
        var now = DateTimeOffset.UtcNow;
        var seededPasswordHash = _passwordHasher.Hash(SeedPassword);

        _items.Add(new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000101"),
            Name = "Admin",
            Email = "admin@coreflow.local",
            Phone = "+5511900000000",
            PasswordHash = seededPasswordHash,
            CreatedAt = now,
            UpdatedAt = now
        });

        for (var i = 1; i <= 50; i++)
        {
            var createdAt = now.AddMinutes(-i);
            _items.Add(new User
            {
                Name = $"User {i}",
                Email = $"user{i}@example.com",
                Phone = $"+55119{i:00000000}",
                PasswordHash = seededPasswordHash,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            });
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string NormalizePhone(string phone) => phone.Trim();
}
