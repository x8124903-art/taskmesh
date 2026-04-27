using MsNotifications.Application.UseCases.Notifications;

namespace MsNotifications.Tests.Application.UseCases;

public sealed class CreateNotificationUseCaseShould
{
    [Fact]
    public async Task ExecuteAsync_CreatesNotification_AndReturnsNewId()
    {
        // Arrange
        const int EXPECTED_ID = 10;
        var request = new CreateNotificationRequest(
            UserId: 1,
            Type: "TaskAssigned",
            Title: "Test",
            Message: "Test message",
            RelatedEntityType: "Task",
            RelatedEntityId: 1,
            RelatedProjectId: 1
        );

        var serviceMock = new Mock<INotificationService>();
        serviceMock.Setup(x => x.CreateAsync(It.IsAny<NotificationModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EXPECTED_ID);

        var useCase = new CreateNotificationUseCase(serviceMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(request);

        // Assert
        result.Should().Be(EXPECTED_ID);
        serviceMock.Verify(x => x.CreateAsync(
            It.Is<NotificationModel>(n =>
                n.UserId == request.UserId &&
                n.Type == request.Type &&
                n.Title == request.Title &&
                n.Message == request.Message &&
                n.IsRead == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
