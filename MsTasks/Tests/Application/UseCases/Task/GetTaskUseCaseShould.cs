using FluentAssertions;
using Moq;
using MsTasks.Application.Models;
using MsTasks.Domain;
using MsTasks.Application.UseCases.Task;
using MsTasks.Domain.Services;

namespace MsTasks.Tests.Application.UseCases.Task;

public sealed class GetTaskUseCaseShould
{
    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_DelegatesToTaskService()
    {
        // Arrange
        const int GIVEN_TASK_ID = 1;
        const int GIVEN_USER_ID = 10;
        var expected = new TaskModel(GIVEN_TASK_ID, "Title", null, 1, null, TaskPriority.Medium, TaskStatus.Todo, null, GIVEN_USER_ID, 0, DateTime.UtcNow, DateTime.UtcNow);

        var serviceMock = new Mock<ITaskService>();
        serviceMock
            .Setup(x => x.GetByIdAsync(GIVEN_TASK_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var useCase = new GetTaskUseCase(serviceMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(GIVEN_TASK_ID, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
        serviceMock.Verify(x => x.GetByIdAsync(GIVEN_TASK_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()), Times.Once);
    }
}
