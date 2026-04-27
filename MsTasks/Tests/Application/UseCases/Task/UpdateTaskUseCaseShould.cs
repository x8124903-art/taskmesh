using FluentAssertions;
using Moq;
using MsTasks.Application.Models;
using MsTasks.Domain;
using MsTasks.Application.UseCases.Task;
using MsTasks.Domain.Services;

namespace MsTasks.Tests.Application.UseCases.Task;

public sealed class UpdateTaskUseCaseShould
{
    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_DelegatesToTaskService()
    {
        // Arrange
        const int GIVEN_TASK_ID = 1;
        const int GIVEN_USER_ID = 10;
        var request = new UpdateTaskRequest("Updated", null, null, TaskPriority.High, null, 0);
        var expected = new TaskModel(GIVEN_TASK_ID, "Updated", null, 1, null, TaskPriority.High, TaskStatus.Todo, null, GIVEN_USER_ID, 1, DateTime.UtcNow, DateTime.UtcNow);

        var serviceMock = new Mock<ITaskService>();
        serviceMock
            .Setup(x => x.UpdateAsync(GIVEN_TASK_ID, request, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var useCase = new UpdateTaskUseCase(serviceMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(GIVEN_TASK_ID, request, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
    }
}
