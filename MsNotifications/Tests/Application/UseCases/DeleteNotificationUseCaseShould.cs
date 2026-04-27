using MsNotifications.Application.UseCases.Notifications;

namespace MsNotifications.Tests.Application.UseCases;

public sealed class DeleteNotificationUseCaseShould
{
    [Fact]
    public async Task ExecuteAsync_DelegatesToService()
    {
        // Arrange
        const int GIVEN_ID = 1;
        const int GIVEN_USER_ID = 1;
        var serviceMock = new Mock<INotificationService>();
        var useCase = new DeleteNotificationUseCase(serviceMock.Object);

        // Act
        await useCase.ExecuteAsync(GIVEN_ID, GIVEN_USER_ID);

        // Assert
        serviceMock.Verify(x => x.DeleteAsync(GIVEN_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()), Times.Once);
    }
}
