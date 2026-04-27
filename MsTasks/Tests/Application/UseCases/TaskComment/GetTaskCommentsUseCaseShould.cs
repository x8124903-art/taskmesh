using FluentAssertions;
using Moq;
using MsTasks.Application.Models;
using MsTasks.Application.UseCases.TaskComment;
using MsTasks.Domain.Services;

namespace MsTasks.Tests.Application.UseCases.TaskComment;

public sealed class GetTaskCommentsUseCaseShould
{
    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_DelegatesToTaskCommentService()
    {
        // Arrange
        const int GIVEN_TASK_ID = 1;
        const int GIVEN_USER_ID = 10;
        var expected = new List<TaskCommentModel>
        {
            new(1, GIVEN_TASK_ID, GIVEN_USER_ID, "Comment 1", DateTime.UtcNow),
            new(2, GIVEN_TASK_ID, GIVEN_USER_ID, "Comment 2", DateTime.UtcNow)
        };

        var serviceMock = new Mock<ITaskCommentService>();
        serviceMock
            .Setup(x => x.GetCommentsAsync(GIVEN_TASK_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var useCase = new GetTaskCommentsUseCase(serviceMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(GIVEN_TASK_ID, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expected);
        result.Should().HaveCount(2);
    }
}
