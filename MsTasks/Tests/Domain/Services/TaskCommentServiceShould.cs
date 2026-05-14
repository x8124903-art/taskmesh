using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsTasks.Application.Models;
using MsTasks.Domain;
using MsTasks.Domain.Exceptions;
using MsTasks.Domain.Services;
using MsTasks.Infrastructure.EventBus;
using MsTasks.Infrastructure.HttpClients;
using MsTasks.Infrastructure.Repositories;

namespace MsTasks.Tests.Domain.Services;

public sealed class TaskCommentServiceShould
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<ITaskCommentRepository> _taskCommentRepositoryMock;
    private readonly Mock<IProjectHttpClient> _projectHttpClientMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly Mock<ILogger<TaskCommentService>> _loggerMock;
    private readonly TaskCommentService _service;

    public TaskCommentServiceShould()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _taskCommentRepositoryMock = new Mock<ITaskCommentRepository>();
        _projectHttpClientMock = new Mock<IProjectHttpClient>();
        _eventBusMock = new Mock<IEventBus>();
        _loggerMock = new Mock<ILogger<TaskCommentService>>();
        _service = new TaskCommentService(_taskRepositoryMock.Object, _taskCommentRepositoryMock.Object, _projectHttpClientMock.Object, _eventBusMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddCommentAsync_Success_WhenUserIsMember()
    {
        // Arrange
        const int GIVEN_TASK_ID = 1;
        const int GIVEN_USER_ID = 10;
        const int GIVEN_PROJECT_ID = 5;
        var task = new TaskModel(GIVEN_TASK_ID, "Task", null, GIVEN_PROJECT_ID, null, TaskPriority.Medium, TaskStatus.Todo, null, GIVEN_USER_ID, 0, DateTime.UtcNow, DateTime.UtcNow);
        var request = new AddTaskCommentRequest("This is a comment");
        var expectedComment = new TaskCommentModel(1, GIVEN_TASK_ID, GIVEN_USER_ID, "This is a comment", DateTime.UtcNow);

        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(GIVEN_TASK_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        _projectHttpClientMock
            .Setup(x => x.GetUserRoleInProjectAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Member");

        _taskCommentRepositoryMock
            .Setup(x => x.CreateAsync(GIVEN_TASK_ID, GIVEN_USER_ID, "This is a comment", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedComment);

        // Act
        var result = await _service.AddCommentAsync(GIVEN_TASK_ID, request, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Comment.Should().Be("This is a comment");
        result.UserId.Should().Be(GIVEN_USER_ID);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddCommentAsync_ThrowsUnauthorized_WhenUserIsViewer()
    {
        // Arrange
        const int GIVEN_TASK_ID = 1;
        const int GIVEN_USER_ID = 10;
        const int GIVEN_PROJECT_ID = 5;
        var task = new TaskModel(GIVEN_TASK_ID, "Task", null, GIVEN_PROJECT_ID, null, TaskPriority.Medium, TaskStatus.Todo, null, GIVEN_USER_ID, 0, DateTime.UtcNow, DateTime.UtcNow);
        var request = new AddTaskCommentRequest("This is a comment");

        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(GIVEN_TASK_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        _projectHttpClientMock
            .Setup(x => x.GetUserRoleInProjectAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Viewer");

        // Act
        Func<System.Threading.Tasks.Task> act = async () => await _service.AddCommentAsync(GIVEN_TASK_ID, request, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*does not have permission to add comments*");
    }

    [Fact]
    public async System.Threading.Tasks.Task AddCommentAsync_ThrowsTaskNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        const int GIVEN_TASK_ID = 999;
        const int GIVEN_USER_ID = 10;
        var request = new AddTaskCommentRequest("Comment");

        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(GIVEN_TASK_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskModel?)null);

        // Act
        Func<System.Threading.Tasks.Task> act = async () => await _service.AddCommentAsync(GIVEN_TASK_ID, request, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TaskNotFoundException>();
    }

    [Fact]
    public async System.Threading.Tasks.Task GetCommentsAsync_ReturnsOrderedList()
    {
        // Arrange
        const int GIVEN_TASK_ID = 1;
        const int GIVEN_USER_ID = 10;
        const int GIVEN_PROJECT_ID = 5;
        var task = new TaskModel(GIVEN_TASK_ID, "Task", null, GIVEN_PROJECT_ID, null, TaskPriority.Medium, TaskStatus.Todo, null, GIVEN_USER_ID, 0, DateTime.UtcNow, DateTime.UtcNow);
        
        var comments = new List<TaskCommentModel>
        {
            new(1, GIVEN_TASK_ID, GIVEN_USER_ID, "First comment", DateTime.UtcNow.AddHours(-2)),
            new(2, GIVEN_TASK_ID, GIVEN_USER_ID, "Second comment", DateTime.UtcNow.AddHours(-1)),
            new(3, GIVEN_TASK_ID, GIVEN_USER_ID, "Third comment", DateTime.UtcNow)
        };

        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(GIVEN_TASK_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        _projectHttpClientMock
            .Setup(x => x.GetUserRoleInProjectAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Viewer"); 

        _taskCommentRepositoryMock
            .Setup(x => x.GetByTaskIdAsync(GIVEN_TASK_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comments);

        // Act
        var result = await _service.GetCommentsAsync(GIVEN_TASK_ID, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().ContainInOrder(comments);
    }
}
