using MsNotifications.Domain.Services.Exceptions;

namespace MsNotifications.Tests.Domain.Services;

public sealed class NotificationServiceShould
{
    private readonly Mock<INotificationRepository> _notificationRepoMock;
    private readonly Mock<IProcessedEventRepository> _processedEventRepoMock;
    private readonly NotificationService _service;

    public NotificationServiceShould()
    {
        _notificationRepoMock = new Mock<INotificationRepository>();
        _processedEventRepoMock = new Mock<IProcessedEventRepository>();
        _service = new NotificationService(_notificationRepoMock.Object, _processedEventRepoMock.Object);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsNotifications()
    {
        // Arrange
        const int GIVEN_USER_ID = 1;
        var expected = new List<NotificationModel>
        {
            new(1, GIVEN_USER_ID, "TaskAssigned", "Title", "Message", "Task", 1, 1, false, DateTime.UtcNow)
        };
        _notificationRepoMock.Setup(x => x.GetByUserIdAsync(GIVEN_USER_ID, null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _service.GetByUserIdAsync(GIVEN_USER_ID, null, null, 1, 20);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetUnreadCountByUserIdAsync_ReturnsCount()
    {
        // Arrange
        const int GIVEN_USER_ID = 1;
        const int EXPECTED_COUNT = 5;
        _notificationRepoMock.Setup(x => x.GetUnreadCountByUserIdAsync(GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(EXPECTED_COUNT);

        // Act
        var result = await _service.GetUnreadCountByUserIdAsync(GIVEN_USER_ID);

        // Assert
        result.Should().Be(EXPECTED_COUNT);
    }

    [Fact]
    public async Task MarkAsReadAsync_Success_CompletesWithoutException()
    {
        // Arrange
        const int GIVEN_ID = 1;
        const int GIVEN_USER_ID = 1;
        _notificationRepoMock.Setup(x => x.MarkAsReadAsync(GIVEN_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _service.MarkAsReadAsync(GIVEN_ID, GIVEN_USER_ID);

        // Assert
        _notificationRepoMock.Verify(x => x.MarkAsReadAsync(GIVEN_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        const int GIVEN_ID = 999;
        const int GIVEN_USER_ID = 1;
        _notificationRepoMock.Setup(x => x.MarkAsReadAsync(GIVEN_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var act = async () => await _service.MarkAsReadAsync(GIVEN_ID, GIVEN_USER_ID);

        // Assert
        await act.Should().ThrowAsync<NotificationNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        const int GIVEN_ID = 999;
        const int GIVEN_USER_ID = 1;
        _notificationRepoMock.Setup(x => x.DeleteAsync(GIVEN_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var act = async () => await _service.DeleteAsync(GIVEN_ID, GIVEN_USER_ID);

        // Assert
        await act.Should().ThrowAsync<NotificationNotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_ReturnsNewId()
    {
        // Arrange
        const int EXPECTED_ID = 10;
        var notification = new NotificationModel(0, 1, "TaskAssigned", "Title", "Message", "Task", 1, 1, false, DateTime.UtcNow);
        _notificationRepoMock.Setup(x => x.CreateAsync(notification, It.IsAny<CancellationToken>()))
            .ReturnsAsync(EXPECTED_ID);

        // Act
        var result = await _service.CreateAsync(notification);

        // Assert
        result.Should().Be(EXPECTED_ID);
    }

    [Fact]
    public async Task IsEventProcessedAsync_ReturnsTrue_WhenEventExists()
    {
        // Arrange
        const string GIVEN_EVENT_ID = "event-123";
        _processedEventRepoMock.Setup(x => x.IsEventProcessedAsync(GIVEN_EVENT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.IsEventProcessedAsync(GIVEN_EVENT_ID);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task MarkEventAsProcessedAsync_CallsRepository()
    {
        // Arrange
        const string GIVEN_EVENT_ID = "event-123";
        const string GIVEN_EVENT_TYPE = "TaskAssignedEvent";

        // Act
        await _service.MarkEventAsProcessedAsync(GIVEN_EVENT_ID, GIVEN_EVENT_TYPE);

        // Assert
        _processedEventRepoMock.Verify(x => x.MarkEventAsProcessedAsync(GIVEN_EVENT_ID, GIVEN_EVENT_TYPE, It.IsAny<CancellationToken>()), Times.Once);
    }
}
