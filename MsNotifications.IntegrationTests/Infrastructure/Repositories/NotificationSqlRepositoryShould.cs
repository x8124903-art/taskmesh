using MsNotifications.IntegrationTests.Fixtures;

namespace MsNotifications.IntegrationTests.Infrastructure.Repositories;

[Collection("Database collection")]
public sealed class NotificationSqlRepositoryShould
{
    private readonly DatabaseFixture _fixture;
    private readonly INotificationRepository _repository;

    public NotificationSqlRepositoryShould(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new NotificationSqlRepository(_fixture.Context);
        _fixture.Cleanup();
    }

    [Fact]
    public async Task CreateAsync_InsertsNotification_AndReturnsNewId()
    {
        // Arrange
        var notification = new NotificationModel(
            IdNotification: 0,
            UserId: 1,
            Type: "TaskAssigned",
            Title: "Test Title",
            Message: "Test Message",
            RelatedEntityType: "Task",
            RelatedEntityId: 1,
            RelatedProjectId: 1,
            IsRead: false,
            CreatedAt: DateTime.UtcNow
        );

        // Act
        var newId = await _repository.CreateAsync(notification);

        // Assert
        newId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsUserNotifications()
    {
        // Arrange
        const int GIVEN_USER_ID = 1;
        var notification = new NotificationModel(0, GIVEN_USER_ID, "TaskAssigned", "Title", "Message", "Task", 1, 1, false, DateTime.UtcNow);
        await _repository.CreateAsync(notification);

        // Act
        var results = await _repository.GetByUserIdAsync(GIVEN_USER_ID, null, null, 1, 20);

        // Assert
        results.Should().HaveCountGreaterThan(0);
        results.All(n => n.UserId == GIVEN_USER_ID).Should().BeTrue();
    }

    [Fact]
    public async Task GetUnreadCountByUserIdAsync_ReturnsCorrectCount()
    {
        // Arrange
        const int GIVEN_USER_ID = 1;
        await _repository.CreateAsync(new NotificationModel(0, GIVEN_USER_ID, "Type1", "Title", "Msg", null, null, null, false, DateTime.UtcNow));
        await _repository.CreateAsync(new NotificationModel(0, GIVEN_USER_ID, "Type2", "Title", "Msg", null, null, null, false, DateTime.UtcNow));
        await _repository.CreateAsync(new NotificationModel(0, GIVEN_USER_ID, "Type3", "Title", "Msg", null, null, null, true, DateTime.UtcNow));

        // Act
        var count = await _repository.GetUnreadCountByUserIdAsync(GIVEN_USER_ID);

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public async Task MarkAsReadAsync_UpdatesNotification()
    {
        // Arrange
        const int GIVEN_USER_ID = 1;
        var notification = new NotificationModel(0, GIVEN_USER_ID, "TaskAssigned", "Title", "Message", null, null, null, false, DateTime.UtcNow);
        var id = await _repository.CreateAsync(notification);

        // Act
        var success = await _repository.MarkAsReadAsync(id, GIVEN_USER_ID);

        // Assert
        success.Should().BeTrue();
        var updated = await _repository.GetByIdAsync(id);
        updated!.IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAllAsReadAsync_UpdatesAllUnread()
    {
        // Arrange
        const int GIVEN_USER_ID = 1;
        await _repository.CreateAsync(new NotificationModel(0, GIVEN_USER_ID, "Type1", "T", "M", null, null, null, false, DateTime.UtcNow));
        await _repository.CreateAsync(new NotificationModel(0, GIVEN_USER_ID, "Type2", "T", "M", null, null, null, false, DateTime.UtcNow));

        // Act
        var updated = await _repository.MarkAllAsReadAsync(GIVEN_USER_ID);

        // Assert
        updated.Should().Be(2);
        var count = await _repository.GetUnreadCountByUserIdAsync(GIVEN_USER_ID);
        count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteAsync_RemovesNotification()
    {
        // Arrange
        const int GIVEN_USER_ID = 1;
        var notification = new NotificationModel(0, GIVEN_USER_ID, "TaskAssigned", "Title", "Message", null, null, null, false, DateTime.UtcNow);
        var id = await _repository.CreateAsync(notification);

        // Act
        var success = await _repository.DeleteAsync(id, GIVEN_USER_ID);

        // Assert
        success.Should().BeTrue();
        var deleted = await _repository.GetByIdAsync(id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_WithFilters_ReturnsFilteredResults()
    {
        // Arrange
        const int GIVEN_USER_ID = 1;
        await _repository.CreateAsync(new NotificationModel(0, GIVEN_USER_ID, "TaskAssigned", "T", "M", null, null, null, false, DateTime.UtcNow));
        await _repository.CreateAsync(new NotificationModel(0, GIVEN_USER_ID, "TaskAssigned", "T", "M", null, null, null, true, DateTime.UtcNow));
        await _repository.CreateAsync(new NotificationModel(0, GIVEN_USER_ID, "MemberRemoved", "T", "M", null, null, null, false, DateTime.UtcNow));

        // Act
        var taskNotifications = await _repository.GetByUserIdAsync(GIVEN_USER_ID, null, "TaskAssigned", 1, 20);

        // Assert
        taskNotifications.Should().HaveCount(2);
        taskNotifications.All(n => n.Type == "TaskAssigned").Should().BeTrue();
    }
}
