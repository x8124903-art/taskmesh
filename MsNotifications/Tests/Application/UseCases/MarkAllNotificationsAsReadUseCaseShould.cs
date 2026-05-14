using MsNotifications.Application.UseCases.Notifications;

namespace MsNotifications.Tests.Application.UseCases;

public sealed class MarkAllNotificationsAsReadUseCaseShould
{
    [Fact]
    public async Task ExecuteAsync_DelegatesToService_AndReturnsUpdatedCount()
    {
        // Arrange
        const int GIVEN_USER_ID = 1;
        const int EXPECTED_UPDATED = 5;
        var serviceMock = new Mock<INotificationService>();
        serviceMock.Setup(x => x.MarkAllAsReadAsync(GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(EXPECTED_UPDATED);

        var useCase = new MarkAllNotificationsAsReadUseCase(serviceMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(GIVEN_USER_ID);

        // Assert
        result.Should().Be(EXPECTED_UPDATED);
    }
}
