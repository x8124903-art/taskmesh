using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MsTasks.Application.Controllers;
using MsTasks.Application.Models;
using MsTasks.Application.UseCases.TaskComment;

namespace MsTasks.Tests.Application.Controllers;

public sealed class TaskCommentsControllerShould
{
    [Fact]
    public async Task Add_Returns201WithLocationHeader_WhenUseCaseSucceeds()
    {
        // Arrange
        const int GIVEN_TASK_ID = 1;
        const int GIVEN_USER_ID = 10;
        var request = new AddTaskCommentRequest("This is a comment");
        var expected = new TaskCommentModel(1, GIVEN_TASK_ID, GIVEN_USER_ID, "This is a comment", DateTime.UtcNow);

        var useCaseMock = new Mock<IAddTaskCommentUseCase>();
        useCaseMock
            .Setup(x => x.ExecuteAsync(GIVEN_TASK_ID, request, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = CreateController(addTaskCommentUseCase: useCaseMock.Object);
        controller.ControllerContext = CreateControllerContext(GIVEN_USER_ID);

        // Act
        var result = await controller.Add(GIVEN_TASK_ID, request, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.Location.Should().Be($"/tasks/{GIVEN_TASK_ID}/comments/{expected.IdTaskComment}");
        createdResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetAll_Returns200WithList_WhenUseCaseSucceeds()
    {
        // Arrange
        const int GIVEN_TASK_ID = 1;
        const int GIVEN_USER_ID = 10;
        var expected = new List<TaskCommentModel>
        {
            new(1, GIVEN_TASK_ID, GIVEN_USER_ID, "Comment 1", DateTime.UtcNow.AddMinutes(-10)),
            new(2, GIVEN_TASK_ID, GIVEN_USER_ID, "Comment 2", DateTime.UtcNow.AddMinutes(-5)),
            new(3, GIVEN_TASK_ID, GIVEN_USER_ID, "Comment 3", DateTime.UtcNow)
        };

        var useCaseMock = new Mock<IGetTaskCommentsUseCase>();
        useCaseMock
            .Setup(x => x.ExecuteAsync(GIVEN_TASK_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = CreateController(getTaskCommentsUseCase: useCaseMock.Object);
        controller.ControllerContext = CreateControllerContext(GIVEN_USER_ID);

        // Act
        var result = await controller.GetAll(GIVEN_TASK_ID, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var comments = okResult.Value.Should().BeAssignableTo<IEnumerable<TaskCommentModel>>().Subject;
        comments.Should().HaveCount(3);
        comments.Should().BeEquivalentTo(expected);
    }

    private static TaskCommentsController CreateController(
        IAddTaskCommentUseCase? addTaskCommentUseCase = null,
        IGetTaskCommentsUseCase? getTaskCommentsUseCase = null)
    {
        return new TaskCommentsController(
            addTaskCommentUseCase ?? Mock.Of<IAddTaskCommentUseCase>(),
            getTaskCommentsUseCase ?? Mock.Of<IGetTaskCommentsUseCase>()
        );
    }

    private static ControllerContext CreateControllerContext(int userId)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-User-Id"] = userId.ToString();

        return new ControllerContext
        {
            HttpContext = httpContext
        };
    }
}
