using MsNotifications.Application.UseCases.Notifications;

namespace MsNotifications.Tests.Application.UseCases;

public sealed class GetNotificationsUseCaseShould
{
    [Fact]
    public async Task ExecuteAsync_DelegatesToService_AndMapsToResponse()
    {
        // Arrange
        const int GIVEN_USER_ID = 1;
        var serviceMock = new Mock<INotificationService>();
        var notifications = new List<NotificationModel>
        {
            new(1, GIVEN_USER_ID, "TaskAssigned", "Title", "Message", "Task", 1, 1, false, DateTime.UtcNow)
        };
        serviceMock.Setup(x => x.GetByUserIdAsync(GIVEN_USER_ID, null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications);

        var useCase = new GetNotificationsUseCase(serviceMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(GIVEN_USER_ID, null, null, 1, 20);

        // Assert
        result.Notifications.Should().HaveCount(1);
        result.Notifications[0].IdNotification.Should().Be(1);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(20);
    }
}
