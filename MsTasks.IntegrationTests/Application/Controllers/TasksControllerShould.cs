using System.Net.Http.Headers;
using MsTasks.Application.Models;
using MsTasks.Domain;
using MsTasks.IntegrationTests.Fixtures;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace MsTasks.IntegrationTests.Application.Controllers;

[Collection(nameof(MsTasksFixtureCollection))]
public sealed class TasksControllerShould
{
    private readonly MsTasksFixture _fixture;
    private readonly HttpClient _client;

    public TasksControllerShould(MsTasksFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.HttpClient;
        _fixture.ResetMsProjectsMocks();
    }

    [Fact]
    public async Task Create_Returns201_WhenUserIsMember()
    {
        // Arrange
        var request = new AddTaskRequest(
            "Integration Test Task",
            "Created via integration test",
            ProjectId: 1,
            AssignedToUserId: 20,
            Priority: TaskPriority.High,
            DueDate: DateTime.UtcNow.AddDays(7));

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "20");

        // Act
        var response = await _client.PostAsJsonAsync("/tasks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var createdTask = await response.Content.ReadFromJsonAsync<TaskModel>();
        createdTask.Should().NotBeNull();
        createdTask!.Title.Should().Be("Integration Test Task");
        createdTask.Priority.Should().Be(TaskPriority.High);
        createdTask.Status.Should().Be(TaskStatus.Todo);
        createdTask.CreatedBy.Should().Be(20);
    }

    [Fact]
    public async Task Create_Returns403_WhenUserIsViewer()
    {
        // Arrange
        var request = new AddTaskRequest(
            "Viewer Task",
            "Should fail",
            ProjectId: 1,
            AssignedToUserId: null,
            Priority: TaskPriority.Low,
            DueDate: null);

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "30");

        // Act
        var response = await _client.PostAsJsonAsync("/tasks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_Returns403_WhenUserIsNotMember()
    {
        // Arrange
        var request = new AddTaskRequest(
            "Non-member Task",
            "Should fail",
            ProjectId: 1,
            AssignedToUserId: null,
            Priority: TaskPriority.Medium,
            DueDate: null);

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "99");

        // Act
        var response = await _client.PostAsJsonAsync("/tasks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAll_Returns200WithList_WhenFilterByProject()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "10");

        // Act
        var response = await _client.GetAsync("/tasks?projectId=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var tasks = await response.Content.ReadFromJsonAsync<List<TaskModel>>();
        tasks.Should().NotBeNull();
        tasks.Should().NotBeEmpty();
        tasks.Should().OnlyContain(t => t.ProjectId == 1);
    }

    [Fact]
    public async Task GetAll_FiltersCorrectly_WhenStatusProvided()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "10");

        // Act
        var response = await _client.GetAsync("/tasks?projectId=1&status=Done");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var tasks = await response.Content.ReadFromJsonAsync<List<TaskModel>>();
        tasks.Should().NotBeNull();
        tasks.Should().OnlyContain(t => t.Status == TaskStatus.Done);
        tasks.Should().HaveCountGreaterOrEqualTo(5);
    }

    [Fact]
    public async Task GetAll_FiltersCorrectly_WhenPriorityProvided()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "10");

        // Act
        var response = await _client.GetAsync("/tasks?projectId=1&priority=High");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var tasks = await response.Content.ReadFromJsonAsync<List<TaskModel>>();
        tasks.Should().NotBeNull();
        tasks.Should().OnlyContain(t => t.Priority == TaskPriority.High);
    }

    [Fact]
    public async Task GetById_Returns200_WhenTaskExists()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "10");

        // Act
        var response = await _client.GetAsync("/tasks/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var task = await response.Content.ReadFromJsonAsync<TaskModel>();
        task.Should().NotBeNull();
        task!.IdTask.Should().Be(1);
        task.ProjectId.Should().Be(1);
    }

    [Fact]
    public async Task GetById_Returns404_WhenTaskNotFound()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "10");

        // Act
        var response = await _client.GetAsync("/tasks/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_Returns200_WhenOwnerUpdatesTask()
    {
        // Arrange
        var updateRequest = new UpdateTaskRequest(
            "Updated Title",
            "Updated Description",
            AssignedToUserId: null,
            Priority: TaskPriority.Low,
            DueDate: DateTime.UtcNow.AddDays(10),
            RowVersion: 1);

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "10");

        // Act
        var response = await _client.PutAsJsonAsync("/tasks/1", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_Returns204_WhenOwnerDeletesTask()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "10");

        var initialCount = await _fixture.CountTasksAsync();

        // Act
        var response = await _client.DeleteAsync("/tasks/5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var finalCount = await _fixture.CountTasksAsync();
        finalCount.Should().Be(initialCount - 1);
    }

    [Fact]
    public async Task Delete_Returns403_WhenMemberDeletesOthersTask()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "20");

        // Act
        var response = await _client.DeleteAsync("/tasks/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_Returns204_WhenMemberDeletesOwnTask()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "20"); 

        // Act
        var response = await _client.DeleteAsync("/tasks/24");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AssignTask_Returns200_WhenOwnerAssigns()
    {
        // Arrange
        var assignRequest = new AssignTaskRequest(AssignedToUserId: 20);

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "10");

        // Act
        var response = await _client.PatchAsync("/tasks/2/assign", JsonContent.Create(assignRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify assignment
        var getResponse = await _client.GetAsync("/tasks/2");
        var task = await getResponse.Content.ReadFromJsonAsync<TaskModel>();
        task!.AssignedToUserId.Should().Be(20);
    }

    [Fact]
    public async Task AssignTask_Returns403_WhenMemberTriesToAssign()
    {
        // Arrange
        var assignRequest = new AssignTaskRequest(AssignedToUserId: 10);

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "20");

        // Act
        var response = await _client.PatchAsync("/tasks/3/assign", JsonContent.Create(assignRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ChangeStatus_Returns200_WhenValidStatus()
    {
        // Arrange
        var statusRequest = new ChangeStatusRequest(TaskStatus.InProgress);

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "20");

        // Act
        var response = await _client.PatchAsync("/tasks/25/status", JsonContent.Create(statusRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync("/tasks/25");
        var task = await getResponse.Content.ReadFromJsonAsync<TaskModel>();
        task!.Status.Should().Be(TaskStatus.InProgress);
    }

    [Fact]
    public async Task ChangeStatus_Returns400_WhenInvalidStatus()
    {
        var jsonContent = new StringContent(
            "{\"status\": \"InvalidStatus\"}",
            System.Text.Encoding.UTF8,
            "application/json");

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "10");

        // Act
        var response = await _client.PatchAsync("/tasks/1/status", jsonContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetBoard_Returns200WithAllColumns_WhenRequested()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "10");

        // Act
        var response = await _client.GetAsync("/tasks/board/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var boardResponse = await response.Content.ReadFromJsonAsync<BoardResponse>();
        boardResponse.Should().NotBeNull();
        boardResponse!.Columns.Should().HaveCount(6);
        boardResponse.Columns.Should().ContainKeys("Todo", "InProgress", "Review", "Testing", "Done", "Blocked");

        boardResponse.Columns["Todo"].Should().NotBeEmpty();
        boardResponse.Columns["InProgress"].Should().NotBeEmpty();
        boardResponse.Columns["Review"].Should().NotBeEmpty();
        boardResponse.Columns["Testing"].Should().NotBeEmpty();
        boardResponse.Columns["Done"].Should().NotBeEmpty();
        boardResponse.Columns["Blocked"].Should().NotBeEmpty();

        var totalTasks = boardResponse.Columns.Values.Sum(tasks => tasks.Count);
        totalTasks.Should().BeGreaterOrEqualTo(20);
    }
}
