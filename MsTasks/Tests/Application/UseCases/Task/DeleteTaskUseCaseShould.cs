using Moq;
using MsTasks.Application.UseCases.Task;
using MsTasks.Domain.Services;

namespace MsTasks.Tests.Application.UseCases.Task;

public sealed class DeleteTaskUseCaseShould
{
    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_DelegatesToTaskService()
    {
        // Arrange
        const int GIVEN_TASK_ID = 1;
        const int GIVEN_USER_ID = 10;

        var serviceMock = new Mock<ITaskService>();
        serviceMock
            .Setup(x => x.DeleteAsync(GIVEN_TASK_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        var useCase = new DeleteTaskUseCase(serviceMock.Object);

        // Act
        await useCase.ExecuteAsync(GIVEN_TASK_ID, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        serviceMock.Verify(x => x.DeleteAsync(GIVEN_TASK_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()), Times.Once);
    }
}
