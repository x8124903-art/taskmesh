using Dapper;
using Microsoft.Extensions.DependencyInjection;
using MsTasks.Application.Models;
using MsTasks.Domain;
using MsTasks.Infrastructure.Repositories;
using MsTasks.IntegrationTests.Fixtures;
using MySqlConnector;

namespace MsTasks.IntegrationTests.Infrastructure.Repositories;

[Collection(nameof(MsTasksFixtureCollection))]
public sealed class TaskSqlRepositoryShould
{
    private readonly MsTasksFixture _fixture;
    private readonly ITaskRepository _repository;
    private readonly string _connectionString;

    public TaskSqlRepositoryShould(MsTasksFixture fixture)
    {
        _fixture = fixture;
        _connectionString = _fixture.ConnectionString;

        var scope = _fixture.Services.CreateScope();
        _repository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
    }

    [Fact]
    public async Task CreateAsync_InsertsTaskSuccessfully()
    {
        // Arrange
        var task = new TaskModel(
            IdTask: 0,
            Title: "Repository Test Task",
            Description: "Created via repository test",
            ProjectId: 1,
            AssignedToUserId: 20,
            Priority: TaskPriority.High,
            Status: TaskStatus.Todo,
            DueDate: DateTime.UtcNow.AddDays(5),
            CreatedBy: 10,
            CreatedAt: DateTime.UtcNow,
            RowVersion: 1,
            UpdatedAt: DateTime.UtcNow);

        // Act
        var createdTask = await _repository.CreateAsync(task, CancellationToken.None);

        // Assert
        createdTask.Should().NotBeNull();
        createdTask.IdTask.Should().BeGreaterThan(0);
        createdTask.Title.Should().Be("Repository Test Task");
        createdTask.Priority.Should().Be(TaskPriority.High);
        createdTask.Status.Should().Be(TaskStatus.Todo);
        createdTask.CreatedBy.Should().Be(10);
        createdTask.AssignedToUserId.Should().Be(20);
        createdTask.RowVersion.Should().Be(1);

        // Verify in database
        await using var connection = new MySqlConnection(_connectionString);
        var dbTask = await connection.QueryFirstOrDefaultAsync(
            "SELECT Title FROM Task WHERE IdTask = @Id",
            new { Id = createdTask.IdTask });

        Assert.NotNull(dbTask);
        string title = dbTask.Title;
        title.Should().Be("Repository Test Task");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsTask_WhenTaskExists()
    {
        // Use task 6 which is not mutated by other tests (task 1 is modified by controller integration tests)
        // Act
        var task = await _repository.GetByIdAsync(6, CancellationToken.None);

        // Assert
        task.Should().NotBeNull();
        task!.IdTask.Should().Be(6);
        task.Title.Should().Be("Implement authentication");
        task.ProjectId.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenTaskNotFound()
    {
        // Act
        var task = await _repository.GetByIdAsync(99999, CancellationToken.None);

        // Assert
        task.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenTaskIsDeleted()
    {
        // Arrange - Mark task as deleted
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "UPDATE Task SET IsDeleted = 1 WHERE IdTask = @Id",
            new { Id = 2 });

        // Act
        var task = await _repository.GetByIdAsync(2, CancellationToken.None);

        // Assert
        task.Should().BeNull();

        // Cleanup
        await connection.ExecuteAsync(
            "UPDATE Task SET IsDeleted = 0 WHERE IdTask = @Id",
            new { Id = 2 });
    }

    [Fact]
    public async Task GetByProjectAsync_ReturnsFilteredTasks()
    {
        // Act
        var tasks = await _repository.GetByProjectIdAsync(
            projectId: 1,
            status: null,
            priority: null,
            assignedToUserId: null,
            CancellationToken.None);

        // Assert
        tasks.Should().NotBeEmpty();
        tasks.Should().OnlyContain(t => t.ProjectId == 1);
    }

    [Fact]
    public async Task GetByProjectAsync_FiltersCorrectly_WhenStatusProvided()
    {
        // Act
        var tasks = await _repository.GetByProjectIdAsync(
            projectId: 1,
            status: TaskStatus.Done,
            priority: null,
            assignedToUserId: null,
            CancellationToken.None);

        // Assert
        tasks.Should().NotBeEmpty();
        tasks.Should().OnlyContain(t => t.Status == TaskStatus.Done);
    }

    [Fact]
    public async Task GetByProjectAsync_FiltersCorrectly_WhenPriorityProvided()
    {
        // Act
        var tasks = await _repository.GetByProjectIdAsync(
            projectId: 1,
            status: null,
            priority: TaskPriority.High,
            assignedToUserId: null,
            CancellationToken.None);

        // Assert
        tasks.Should().NotBeEmpty();
        tasks.Should().OnlyContain(t => t.Priority == TaskPriority.High);
    }

    [Fact]
    public async Task GetByProjectAsync_FiltersCorrectly_WhenAssignedToProvided()
    {
        // Act
        var tasks = await _repository.GetByProjectIdAsync(
            projectId: 1,
            status: null,
            priority: null,
            assignedToUserId: 20,
            CancellationToken.None);

        // Assert
        tasks.Should().NotBeEmpty();
        tasks.Should().OnlyContain(t => t.AssignedToUserId == 20);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsRowsAffected()
    {
        // Arrange
        var task = await _repository.GetByIdAsync(3, CancellationToken.None);
        task.Should().NotBeNull();

        var updatedTask = task! with 
        { 
            Title = "Updated Title Repository Test",
            Priority = TaskPriority.Low
        };

        // Act
        var rowsAffected = await _repository.UpdateAsync(updatedTask, CancellationToken.None);

        // Assert
        rowsAffected.Should().Be(1);

        // Verify update
        var reloadedTask = await _repository.GetByIdAsync(3, CancellationToken.None);
        reloadedTask!.Title.Should().Be("Updated Title Repository Test");
        reloadedTask.Priority.Should().Be(TaskPriority.Low);
        reloadedTask.RowVersion.Should().Be(task.RowVersion + 1);
    }

    [Fact]
    public async Task UpdateAsync_Returns0_WhenRowVersionMismatch()
    {
        // Arrange
        var task = await _repository.GetByIdAsync(4, CancellationToken.None);
        task.Should().NotBeNull();

        var updatedTask = task! with 
        { 
            Title = "Updated with wrong version",
            RowVersion = 999  // Incorrect version
        };

        // Act
        var rowsAffected = await _repository.UpdateAsync(updatedTask, CancellationToken.None);

        // Assert
        rowsAffected.Should().Be(0);

        // Verify no update occurred
        var reloadedTask = await _repository.GetByIdAsync(4, CancellationToken.None);
        reloadedTask!.Title.Should().NotBe("Updated with wrong version");
    }

    [Fact]
    public async Task DeleteAsync_SetsIsDeletedFlag()
    {
        // Arrange
        const int TASK_ID = 7;

        // Act
        await _repository.SoftDeleteAsync(TASK_ID, CancellationToken.None);

        // Assert - GetByIdAsync should return null (excludes deleted)
        var task = await _repository.GetByIdAsync(TASK_ID, CancellationToken.None);
        task.Should().BeNull();

        // Verify IsDeleted flag in database
        await using var connection = new MySqlConnection(_connectionString);
        var isDeleted = await connection.ExecuteScalarAsync<bool>(
            "SELECT IsDeleted FROM Task WHERE IdTask = @Id",
            new { Id = TASK_ID });

        isDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task ChangeStatusAsync_UpdatesStatusSuccessfully()
    {
        // Arrange
        const int TASK_ID = 8;
        const TaskStatus NEW_STATUS = TaskStatus.Review;

        // Act
        await _repository.UpdateStatusAsync(TASK_ID, NEW_STATUS, CancellationToken.None);

        // Assert
        var task = await _repository.GetByIdAsync(TASK_ID, CancellationToken.None);
        task.Should().NotBeNull();
        task!.Status.Should().Be(NEW_STATUS);
    }

    [Fact]
    public async Task AssignAsync_UpdatesAssignedUserSuccessfully()
    {
        // Arrange
        const int TASK_ID = 9;
        const int NEW_ASSIGNED_USER_ID = 10;

        // Act
        await _repository.UpdateAssignmentAsync(TASK_ID, NEW_ASSIGNED_USER_ID, CancellationToken.None);

        // Assert
        var task = await _repository.GetByIdAsync(TASK_ID, CancellationToken.None);
        task.Should().NotBeNull();
        task!.AssignedToUserId.Should().Be(NEW_ASSIGNED_USER_ID);
    }

    [Fact]
    public async Task GetBoardAsync_ReturnsTasksForProject()
    {
        // Act
        var tasks = await _repository.GetBoardByProjectIdAsync(projectId: 1, CancellationToken.None);

        // Assert
        tasks.Should().NotBeEmpty();
        tasks.Should().OnlyContain(t => t.ProjectId == 1);

        var grouped = tasks.GroupBy(t => t.Status).ToDictionary(g => g.Key, g => g.ToList());
        grouped.Should().ContainKey(TaskStatus.Todo);
        grouped.Should().ContainKey(TaskStatus.InProgress);
        grouped.Should().ContainKey(TaskStatus.Done);
    }
}

