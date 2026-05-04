using FluentAssertions;
using Moq;
using MsTasks.Application.Models;
using MsTasks.Domain;
using MsTasks.Application.UseCases.Task;
using MsTasks.Domain.Services;

namespace MsTasks.Tests.Application.UseCases.Task;

public sealed class CreateTaskUseCaseShould
{
    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_DelegatesToTaskService()
    {
        // Arrange
        const int GIVEN_USER_ID = 10;
        var request = new AddTaskRequest("Title", null, 1, null, null, null);
        var expected = new TaskModel(1, "Title", null, 1, null, TaskPriority.Medium, TaskStatus.Todo, null, GIVEN_USER_ID, 0, DateTime.UtcNow, DateTime.UtcNow);

        var serviceMock = new Mock<ITaskService>();
        serviceMock
            .Setup(x => x.CreateAsync(request, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var useCase = new CreateTaskUseCase(serviceMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(request, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
        serviceMock.Verify(x => x.CreateAsync(request, GIVEN_USER_ID, It.IsAny<CancellationToken>()), Times.Once);
    }
}
