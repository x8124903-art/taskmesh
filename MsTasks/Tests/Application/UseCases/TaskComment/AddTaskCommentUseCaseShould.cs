using FluentAssertions;
using Moq;
using MsTasks.Application.Models;
using MsTasks.Application.UseCases.TaskComment;
using MsTasks.Domain.Services;

namespace MsTasks.Tests.Application.UseCases.TaskComment;

public sealed class AddTaskCommentUseCaseShould
{
    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_DelegatesToTaskCommentService()
    {
        // Arrange
        const int GIVEN_TASK_ID = 1;
        const int GIVEN_USER_ID = 10;
        var request = new AddTaskCommentRequest("Test comment");
        var expected = new TaskCommentModel(1, GIVEN_TASK_ID, GIVEN_USER_ID, "Test comment", DateTime.UtcNow);

        var serviceMock = new Mock<ITaskCommentService>();
        serviceMock
            .Setup(x => x.AddCommentAsync(GIVEN_TASK_ID, request, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var useCase = new AddTaskCommentUseCase(serviceMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(GIVEN_TASK_ID, request, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
        result.Comment.Should().Be("Test comment");
    }
}
