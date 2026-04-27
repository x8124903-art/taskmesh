using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsTasks.Application.Models;
using MsTasks.Domain;
using MsTasks.Domain.Exceptions;
using MsTasks.Domain.Services;
using MsTasks.Infrastructure.HttpClients;
using MsTasks.Infrastructure.Repositories;

namespace MsTasks.Tests.Domain.Services;

public sealed class TaskServiceShould
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IProjectHttpClient> _projectHttpClientMock;
    private readonly Mock<ILogger<TaskService>> _loggerMock;
    private readonly TaskService _service;

    public TaskServiceShould()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _projectHttpClientMock = new Mock<IProjectHttpClient>();
        _loggerMock = new Mock<ILogger<TaskService>>();
        _service = new TaskService(_taskRepositoryMock.Object, _projectHttpClientMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_Success_WhenUserIsMember()
    {
        // Arrange
        const int GIVEN_PROJECT_ID = 1;
        const int GIVEN_USER_ID = 10;
        var request = new AddTaskRequest("Test Task", "Description", GIVEN_PROJECT_ID, null, TaskPriority.High, null);

        _projectHttpClientMock
            .Setup(x => x.GetUserRoleInProjectAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Member");

        var expectedTask = new TaskModel(1, "Test Task", "Description", GIVEN_PROJECT_ID, null, TaskPriority.High, TaskStatus.Todo, null, GIVEN_USER_ID, 0, DateTime.UtcNow, DateTime.UtcNow);
        _taskRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<TaskModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTask);

        // Act
        var result = await _service.CreateAsync(request, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Task");
        result.Status.Should().Be(TaskStatus.Todo);
        _taskRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<TaskModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_ThrowsUnauthorized_WhenUserIsViewer()
    {
        // Arrange
        const int GIVEN_PROJECT_ID = 1;
        const int GIVEN_USER_ID = 10;
        var request = new AddTaskRequest("Test Task", null, GIVEN_PROJECT_ID, null, null, null);

        _projectHttpClientMock
            .Setup(x => x.GetUserRoleInProjectAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Viewer");

        // Act
        Func<System.Threading.Tasks.Task> act = async () => await _service.CreateAsync(request, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*does not have permission to create tasks*");
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_ThrowsUnauthorized_WhenUserIsNotMember()
    {
        // Arrange
        const int GIVEN_PROJECT_ID = 1;
        const int GIVEN_USER_ID = 10;
        var request = new AddTaskRequest("Test Task", null, GIVEN_PROJECT_ID, null, null, null);

        _projectHttpClientMock
            .Setup(x => x.GetUserRoleInProjectAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null); // Not a member

        // Act
        Func<System.Threading.Tasks.Task> act = async () => await _service.CreateAsync(request, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateAsync_Success_WhenOwnerUpdatesAnyTask()
    {
        // Arrange
        const int GIVEN_TASK_ID = 1;
        const int GIVEN_USER_ID = 10;
        const int GIVEN_PROJECT_ID = 5;
        var existingTask = new TaskModel(GIVEN_TASK_ID, "Old Title", null, GIVEN_PROJECT_ID, null, TaskPriority.Medium, TaskStatus.Todo, null, 99, 0, DateTime.UtcNow, DateTime.UtcNow);
        var request = new UpdateTaskRequest("New Title", "New Description", null, TaskPriority.High, null, 0);

        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(GIVEN_TASK_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);

        _projectHttpClientMock
            .Setup(x => x.GetUserRoleInProjectAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Owner");

        _taskRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<TaskModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(GIVEN_TASK_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskModel(GIVEN_TASK_ID, "New Title", "New Description", GIVEN_PROJECT_ID, null, TaskPriority.High, TaskStatus.Todo, null, 99, 1, DateTime.UtcNow, DateTime.UtcNow));

        // Act
        var result = await _service.UpdateAsync(GIVEN_TASK_ID, request, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New Title");
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateAsync_ThrowsConcurrencyException_WhenRowVersionMismatch()
    {
        // Arrange
        const int GIVEN_TASK_ID = 1;
        const int GIVEN_USER_ID = 10;
        const int GIVEN_PROJECT_ID = 5;
        var existingTask = new TaskModel(GIVEN_TASK_ID, "Title", null, GIVEN_PROJECT_ID, null, TaskPriority.Medium, TaskStatus.Todo, null, GIVEN_USER_ID, 5, DateTime.UtcNow, DateTime.UtcNow);
        var request = new UpdateTaskRequest("New Title", null, null, TaskPriority.High, null, 4); // Old RowVersion

        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(GIVEN_TASK_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);

        _projectHttpClientMock
            .Setup(x => x.GetUserRoleInProjectAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Member");

        _taskRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<TaskModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0); // No rows affected = conflict

        // Act
        Func<System.Threading.Tasks.Task> act = async () => await _service.UpdateAsync(GIVEN_TASK_ID, request, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConcurrencyException>();
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteAsync_Success_WhenMemberDeletesOwnTask()
    {
        // Arrange
        const int GIVEN_TASK_ID = 1;
        const int GIVEN_USER_ID = 10;
        const int GIVEN_PROJECT_ID = 5;
        var existingTask = new TaskModel(GIVEN_TASK_ID, "Title", null, GIVEN_PROJECT_ID, null, TaskPriority.Medium, TaskStatus.Todo, null, GIVEN_USER_ID, 0, DateTime.UtcNow, DateTime.UtcNow);

        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(GIVEN_TASK_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);

        _projectHttpClientMock
            .Setup(x => x.GetUserRoleInProjectAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Member");

        _taskRepositoryMock
            .Setup(x => x.SoftDeleteAsync(GIVEN_TASK_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _service.DeleteAsync(GIVEN_TASK_ID, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        _taskRepositoryMock.Verify(x => x.SoftDeleteAsync(GIVEN_TASK_ID, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteAsync_ThrowsUnauthorized_WhenMemberDeletesOthersTask()
    {
        // Arrange
        const int GIVEN_TASK_ID = 1;
        const int GIVEN_USER_ID = 10;
        const int OTHER_USER_ID = 99;
        const int GIVEN_PROJECT_ID = 5;
        var existingTask = new TaskModel(GIVEN_TASK_ID, "Title", null, GIVEN_PROJECT_ID, null, TaskPriority.Medium, TaskStatus.Todo, null, OTHER_USER_ID, 0, DateTime.UtcNow, DateTime.UtcNow);

        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(GIVEN_TASK_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);

        _projectHttpClientMock
            .Setup(x => x.GetUserRoleInProjectAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Member");

        // Act
        Func<System.Threading.Tasks.Task> act = async () => await _service.DeleteAsync(GIVEN_TASK_ID, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*does not have permission to delete*");
    }

    [Fact]
    public async System.Threading.Tasks.Task AssignAsync_ValidatesMembershipOfAssignedUser()
    {
        // Arrange
        const int GIVEN_TASK_ID = 1;
        const int GIVEN_USER_ID = 10;
        const int ASSIGNED_USER_ID = 20;
        const int GIVEN_PROJECT_ID = 5;
        var existingTask = new TaskModel(GIVEN_TASK_ID, "Title", null, GIVEN_PROJECT_ID, null, TaskPriority.Medium, TaskStatus.Todo, null, GIVEN_USER_ID, 0, DateTime.UtcNow, DateTime.UtcNow);
        var request = new AssignTaskRequest(ASSIGNED_USER_ID);

        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(GIVEN_TASK_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);

        _projectHttpClientMock
            .Setup(x => x.GetUserRoleInProjectAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Admin");

        _projectHttpClientMock
            .Setup(x => x.GetUserRoleInProjectAsync(GIVEN_PROJECT_ID, ASSIGNED_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Member"); // Valid member

        _taskRepositoryMock
            .Setup(x => x.UpdateAssignmentAsync(GIVEN_TASK_ID, ASSIGNED_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(GIVEN_TASK_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskModel(GIVEN_TASK_ID, "Title", null, GIVEN_PROJECT_ID, ASSIGNED_USER_ID, TaskPriority.Medium, TaskStatus.Todo, null, GIVEN_USER_ID, 0, DateTime.UtcNow, DateTime.UtcNow));

        // Act
        var result = await _service.AssignAsync(GIVEN_TASK_ID, request, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AssignedToUserId.Should().Be(ASSIGNED_USER_ID);
        _projectHttpClientMock.Verify(x => x.GetUserRoleInProjectAsync(GIVEN_PROJECT_ID, ASSIGNED_USER_ID, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task ChangeStatusAsync_ValidatesPermission()
    {
        // Arrange
        const int GIVEN_TASK_ID = 1;
        const int GIVEN_USER_ID = 10;
        const int GIVEN_PROJECT_ID = 5;
        var existingTask = new TaskModel(GIVEN_TASK_ID, "Title", null, GIVEN_PROJECT_ID, GIVEN_USER_ID, TaskPriority.Medium, TaskStatus.Todo, null, GIVEN_USER_ID, 0, DateTime.UtcNow, DateTime.UtcNow);
        var request = new ChangeStatusRequest(TaskStatus.InProgress);

        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(GIVEN_TASK_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);

        _projectHttpClientMock
            .Setup(x => x.GetUserRoleInProjectAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Member");

        _taskRepositoryMock
            .Setup(x => x.UpdateStatusAsync(GIVEN_TASK_ID, TaskStatus.InProgress, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(GIVEN_TASK_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskModel(GIVEN_TASK_ID, "Title", null, GIVEN_PROJECT_ID, GIVEN_USER_ID, TaskPriority.Medium, TaskStatus.InProgress, null, GIVEN_USER_ID, 0, DateTime.UtcNow, DateTime.UtcNow));

        // Act
        var result = await _service.ChangeStatusAsync(GIVEN_TASK_ID, request, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(TaskStatus.InProgress);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetBoardAsync_ReturnsGroupedColumns()
    {
        // Arrange
        const int GIVEN_PROJECT_ID = 5;
        const int GIVEN_USER_ID = 10;

        var tasks = new List<TaskModel>
        {
            new(1, "Task 1", null, GIVEN_PROJECT_ID, null, TaskPriority.High, TaskStatus.Todo, null, GIVEN_USER_ID, 0, DateTime.UtcNow, DateTime.UtcNow),
            new(2, "Task 2", null, GIVEN_PROJECT_ID, null, TaskPriority.Medium, TaskStatus.InProgress, null, GIVEN_USER_ID, 0, DateTime.UtcNow, DateTime.UtcNow),
            new(3, "Task 3", null, GIVEN_PROJECT_ID, null, TaskPriority.Low, TaskStatus.Done, null, GIVEN_USER_ID, 0, DateTime.UtcNow, DateTime.UtcNow)
        };

        _projectHttpClientMock
            .Setup(x => x.GetUserRoleInProjectAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Member");

        _taskRepositoryMock
            .Setup(x => x.GetBoardByProjectIdAsync(GIVEN_PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        // Act
        var result = await _service.GetBoardAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Columns.Should().HaveCount(6); // All 6 statuses
        result.Columns["Todo"].Should().HaveCount(1);
        result.Columns["InProgress"].Should().HaveCount(1);
        result.Columns["Done"].Should().HaveCount(1);
        result.Columns["Review"].Should().BeEmpty();
        result.Columns["Testing"].Should().BeEmpty();
        result.Columns["Blocked"].Should().BeEmpty();
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_ThrowsInvalidOperation_WhenMsProjectsUnavailable()
    {
        // Arrange
        const int GIVEN_PROJECT_ID = 1;
        const int GIVEN_USER_ID = 10;
        var request = new AddTaskRequest("Test Task", null, GIVEN_PROJECT_ID, null, null, null);

        _projectHttpClientMock
            .Setup(x => x.GetUserRoleInProjectAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        // Act
        Func<System.Threading.Tasks.Task> act = async () => await _service.CreateAsync(request, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*project service unavailable*");
    }
}
