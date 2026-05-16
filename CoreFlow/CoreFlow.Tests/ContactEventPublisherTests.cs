using CoreFlow.Application.Commands;
using CoreFlow.Application.Events;
using CoreFlow.Application.Handlers;
using CoreFlow.Application.Interfaces;
using CoreFlow.Domain.Entities;
using CoreFlow.Infrastructure.Security;
using CoreFlow.Infrastructure.Services;

namespace CoreFlow.Tests;

public class ContactEventPublisherTests
{
    [Fact]
    public async Task CreateUserHandler_PublishesCreatedContactEvent()
    {
        var service = new InMemoryUserService();
        var publisher = new RecordingContactEventPublisher();
        var handler = new CreateUserHandler(service, new PasswordHasher(), publisher);
        var actor = new ContactEventActor(
            Guid.Parse("00000000-0000-0000-0000-000000000101"),
            "Admin",
            "admin@coreflow.local");

        var id = await handler.Handle(
            new CreateUserCommand("George", "george@example.com", "+5511900000001", "User@123456", actor),
            CancellationToken.None);

        var user = await service.GetByIdAsync(id);

        Assert.NotNull(user);
        var contactEvent = Assert.Single(publisher.Events);
        Assert.Equal(ContactEventType.Created, contactEvent.EventType);
        Assert.Equal(id, contactEvent.Id);
        Assert.Equal("george@example.com", contactEvent.Email);
        Assert.Equal(actor, contactEvent.Actor);
    }

    [Fact]
    public async Task UpdateUserHandler_PublishesUpdatedContactEvent()
    {
        var service = new InMemoryUserService();
        var publisher = new RecordingContactEventPublisher();
        var id = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var actor = new ContactEventActor(
            Guid.Parse("00000000-0000-0000-0000-000000000101"),
            "Admin",
            "admin@coreflow.local");

        await service.AddAsync(new User
        {
            Id = id,
            Name = "Original",
            Email = "original@example.com",
            Phone = "+5511900000001"
        });

        var handler = new UpdateUserHandler(service, publisher);

        await handler.Handle(
            new UpdateUserCommand(id, "Updated", "updated@example.com", "+5511900000002", actor),
            CancellationToken.None);

        var contactEvent = Assert.Single(publisher.Events);
        Assert.Equal(ContactEventType.Updated, contactEvent.EventType);
        Assert.Equal(id, contactEvent.Id);
        Assert.Equal("Updated", contactEvent.Name);
        Assert.Equal("updated@example.com", contactEvent.Email);
        Assert.Equal(actor, contactEvent.Actor);
    }

    [Fact]
    public async Task DeleteUserHandler_PublishesDeletedContactEvent()
    {
        var service = new InMemoryUserService();
        var publisher = new RecordingContactEventPublisher();
        var id = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var actor = new ContactEventActor(
            Guid.Parse("00000000-0000-0000-0000-000000000101"),
            "Admin",
            "admin@coreflow.local");

        await service.AddAsync(new User
        {
            Id = id,
            Name = "Original",
            Email = "original@example.com",
            Phone = "+5511900000001"
        });

        var handler = new DeleteUserHandler(service, publisher);

        await handler.Handle(new DeleteUserCommand(id, actor), CancellationToken.None);

        var user = await service.GetByIdAsync(id);
        var contactEvent = Assert.Single(publisher.Events);

        Assert.Null(user);
        Assert.Equal(ContactEventType.Deleted, contactEvent.EventType);
        Assert.Equal(id, contactEvent.Id);
        Assert.Equal("original@example.com", contactEvent.Email);
        Assert.Equal(actor, contactEvent.Actor);
    }

    private sealed class RecordingContactEventPublisher : IContactEventPublisher
    {
        private readonly List<ContactChangedEvent> _events = new();

        public IReadOnlyList<ContactChangedEvent> Events => _events;

        public Task PublishAsync(ContactChangedEvent contactEvent, CancellationToken cancellationToken = default)
        {
            _events.Add(contactEvent);
            return Task.CompletedTask;
        }
    }
}
