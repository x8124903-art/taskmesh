using FluentAssertions;
using Moq;
using MsTasks.Application.Models;
using MsTasks.Domain;
using MsTasks.Application.UseCases.Task;
using MsTasks.Domain.Services;

namespace MsTasks.Tests.Application.UseCases.Task;

public sealed class AssignTaskUseCaseShould
{
    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_DelegatesToTaskService()
    {
        // Arrange
        const int GIVEN_TASK_ID = 1;
        const int GIVEN_USER_ID = 10;
        const int ASSIGNED_USER_ID = 20;
        var request = new AssignTaskRequest(ASSIGNED_USER_ID);
        var expected = new TaskModel(GIVEN_TASK_ID, "Task", null, 1, ASSIGNED_USER_ID, TaskPriority.Medium, TaskStatus.Todo, null, GIVEN_USER_ID, 0, DateTime.UtcNow, DateTime.UtcNow);

        var serviceMock = new Mock<ITaskService>();
        serviceMock
            .Setup(x => x.AssignAsync(GIVEN_TASK_ID, request, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var useCase = new AssignTaskUseCase(serviceMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(GIVEN_TASK_ID, request, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
        result.AssignedToUserId.Should().Be(ASSIGNED_USER_ID);
    }
}
