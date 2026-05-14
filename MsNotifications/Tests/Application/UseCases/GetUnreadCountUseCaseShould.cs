using MsNotifications.Application.UseCases.Notifications;

namespace MsNotifications.Tests.Application.UseCases;

public sealed class GetUnreadCountUseCaseShould
{
    [Fact]
    public async Task ExecuteAsync_DelegatesToService_AndReturnsCount()
    {
        // Arrange
        const int GIVEN_USER_ID = 1;
        const int EXPECTED_COUNT = 7;
        var serviceMock = new Mock<INotificationService>();
        serviceMock.Setup(x => x.GetUnreadCountByUserIdAsync(GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(EXPECTED_COUNT);

        var useCase = new GetUnreadCountUseCase(serviceMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(GIVEN_USER_ID);

        // Assert
        result.UnreadCount.Should().Be(EXPECTED_COUNT);
    }
}
