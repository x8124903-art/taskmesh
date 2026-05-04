using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MsTasks.Application.Controllers;
using MsTasks.Application.Models;
using MsTasks.Domain;
using MsTasks.Application.UseCases.Task;

namespace MsTasks.Tests.Application.Controllers;

public sealed class TasksControllerShould
{
    [Fact]
    public async Task GetAll_Returns200WithList_WhenUseCaseSucceeds()
    {
        // Arrange
        const int GIVEN_PROJECT_ID = 1;
        const int GIVEN_USER_ID = 10;

        var expected = new List<TaskModel>
        {
            new(1, "Task 1", "Description 1", GIVEN_PROJECT_ID, 10, TaskPriority.High, TaskStatus.Todo, DateTime.UtcNow, 10, 1, DateTime.UtcNow, DateTime.UtcNow)
        };

        var useCaseMock = new Mock<IGetTasksByProjectUseCase>();
        useCaseMock
            .Setup(x => x.ExecuteAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = CreateController(getTasksByProjectUseCase: useCaseMock.Object);
        controller.ControllerContext = CreateControllerContext(GIVEN_USER_ID);

        // Act
        var result = await controller.GetAll(GIVEN_PROJECT_ID, null, null, null, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetAsync_Returns200WithTask_WhenTaskExists()
    {
        // Arrange
        const int GIVEN_ID = 1;
        const int GIVEN_USER_ID = 10;
        var expected = new TaskModel(GIVEN_ID, "Task 1", "Description", 1, 10, TaskPriority.High, TaskStatus.Todo, DateTime.UtcNow, 10, 1, DateTime.UtcNow, DateTime.UtcNow);

        var useCaseMock = new Mock<IGetTaskUseCase>();
        useCaseMock
            .Setup(x => x.ExecuteAsync(GIVEN_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = CreateController(getTaskUseCase: useCaseMock.Object);
        controller.ControllerContext = CreateControllerContext(GIVEN_USER_ID);

        // Act
        var result = await controller.GetAsync(GIVEN_ID, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetAsync_Returns404_WhenTaskNotFound()
    {
        // Arrange
        const int GIVEN_ID = 999;
        const int GIVEN_USER_ID = 10;

        var useCaseMock = new Mock<IGetTaskUseCase>();
        useCaseMock
            .Setup(x => x.ExecuteAsync(GIVEN_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskModel?)null);

        var controller = CreateController(getTaskUseCase: useCaseMock.Object);
        controller.ControllerContext = CreateControllerContext(GIVEN_USER_ID);

        // Act
        var result = await controller.GetAsync(GIVEN_ID, CancellationToken.None);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Add_Returns201WithLocationHeader_WhenUseCaseSucceeds()
    {
        // Arrange
        const int GIVEN_USER_ID = 10;
        var request = new AddTaskRequest("New Task", "Description", 1, null, TaskPriority.High, null);
        var expected = new TaskModel(1, "New Task", "Description", 1, null, TaskPriority.High, TaskStatus.Todo, DateTime.UtcNow, GIVEN_USER_ID, 1, DateTime.UtcNow, DateTime.UtcNow);

        var useCaseMock = new Mock<ICreateTaskUseCase>();
        useCaseMock
            .Setup(x => x.ExecuteAsync(request, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = CreateController(createTaskUseCase: useCaseMock.Object);
        controller.ControllerContext = CreateControllerContext(GIVEN_USER_ID);

        // Act
        var result = await controller.Add(request, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.Location.Should().Be("/tasks/1");
        createdResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Update_Returns200_WhenUseCaseSucceeds()
    {
        // Arrange
        const int GIVEN_ID = 1;
        const int GIVEN_USER_ID = 10;
        var request = new UpdateTaskRequest("Updated Title", "Updated Description", null, TaskPriority.Medium, null, 1);
        var expected = new TaskModel(GIVEN_ID, "Updated Title", "Updated Description", 1, null, TaskPriority.Medium, TaskStatus.Todo, null, GIVEN_USER_ID, 2, DateTime.UtcNow, DateTime.UtcNow);

        var useCaseMock = new Mock<IUpdateTaskUseCase>();
        useCaseMock
            .Setup(x => x.ExecuteAsync(GIVEN_ID, request, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = CreateController(updateTaskUseCase: useCaseMock.Object);
        controller.ControllerContext = CreateControllerContext(GIVEN_USER_ID);

        // Act
        var result = await controller.Update(GIVEN_ID, request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Delete_Returns204_WhenUseCaseSucceeds()
    {
        // Arrange
        const int GIVEN_ID = 1;
        const int GIVEN_USER_ID = 10;

        var useCaseMock = new Mock<IDeleteTaskUseCase>();
        useCaseMock
            .Setup(x => x.ExecuteAsync(GIVEN_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        var controller = CreateController(deleteTaskUseCase: useCaseMock.Object);
        controller.ControllerContext = CreateControllerContext(GIVEN_USER_ID);

        // Act
        var result = await controller.Delete(GIVEN_ID, CancellationToken.None);

        // Assert
        var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
        noContentResult.StatusCode.Should().Be(204);
    }

    private static TasksController CreateController(
        ICreateTaskUseCase? createTaskUseCase = null,
        IGetTaskUseCase? getTaskUseCase = null,
        IGetTasksByProjectUseCase? getTasksByProjectUseCase = null,
        IUpdateTaskUseCase? updateTaskUseCase = null,
        IDeleteTaskUseCase? deleteTaskUseCase = null,
        IAssignTaskUseCase? assignTaskUseCase = null,
        IChangeTaskStatusUseCase? changeTaskStatusUseCase = null,
        IGetBoardTasksUseCase? getBoardTasksUseCase = null)
    {
        return new TasksController(
            createTaskUseCase ?? Mock.Of<ICreateTaskUseCase>(),
            getTaskUseCase ?? Mock.Of<IGetTaskUseCase>(),
            getTasksByProjectUseCase ?? Mock.Of<IGetTasksByProjectUseCase>(),
            updateTaskUseCase ?? Mock.Of<IUpdateTaskUseCase>(),
            deleteTaskUseCase ?? Mock.Of<IDeleteTaskUseCase>(),
            assignTaskUseCase ?? Mock.Of<IAssignTaskUseCase>(),
            changeTaskStatusUseCase ?? Mock.Of<IChangeTaskStatusUseCase>(),
            getBoardTasksUseCase ?? Mock.Of<IGetBoardTasksUseCase>()
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
