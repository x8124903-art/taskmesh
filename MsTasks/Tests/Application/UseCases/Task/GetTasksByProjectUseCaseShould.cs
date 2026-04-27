using FluentAssertions;
using Moq;
using MsTasks.Application.Models;
using MsTasks.Domain;
using MsTasks.Application.UseCases.Task;
using MsTasks.Domain.Services;

namespace MsTasks.Tests.Application.UseCases.Task;

public sealed class GetTasksByProjectUseCaseShould
{
    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_DelegatesToTaskService()
    {
        // Arrange
        const int GIVEN_PROJECT_ID = 1;
        const int GIVEN_USER_ID = 10;
        var expected = new List<TaskModel>
        {
            new(1, "Task 1", null, GIVEN_PROJECT_ID, null, TaskPriority.High, TaskStatus.Todo, null, GIVEN_USER_ID, 0, DateTime.UtcNow, DateTime.UtcNow),
            new(2, "Task 2", null, GIVEN_PROJECT_ID, null, TaskPriority.Medium, TaskStatus.Done, null, GIVEN_USER_ID, 0, DateTime.UtcNow, DateTime.UtcNow)
        };

        var serviceMock = new Mock<ITaskService>();
        serviceMock
            .Setup(x => x.GetByProjectIdAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var useCase = new GetTasksByProjectUseCase(serviceMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, null, null, null, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }
}
