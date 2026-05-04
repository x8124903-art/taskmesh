using System.Net.Http.Json;
using System.Text.Json;
using MsTasks.Application.Models;
using MsTasks.Domain;

namespace MsTasks.FunctionalTests.Application;

[Collection(nameof(ServerFixtureCollection))]
public sealed class TaskPermissionsShould(ServerFixture fixture)
{
    private readonly HttpClient _client = fixture.CreateClient();
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task DenyAccessToNonMember_Returns403()
    {
        _client.DefaultRequestHeaders.Add("X-User-Id", "99");
        
        var getResponse = await _client.GetAsync("/tasks?projectId=1");
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        
        var createRequest = new AddTaskRequest(
            ProjectId: 1,
            Title: "Unauthorized task",
            Description: "This should fail",
            Priority: TaskPriority.Low,
            AssignedToUserId: null,
            DueDate: null);
        
        var createResponse = await _client.PostAsJsonAsync("/tasks", createRequest);
        
        createResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        
        var getTaskModel = await _client.GetAsync("/tasks/1");
        
        getTaskModel.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AllowViewerReadOnly_DenyWrites()
    {
        _client.DefaultRequestHeaders.Add("X-User-Id", "30");
        
        var getResponse = await _client.GetAsync("/tasks?projectId=1");
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tasks = await getResponse.Content.ReadFromJsonAsync<List<TaskModel>>(_jsonOptions);
        tasks.Should().NotBeNull();
        tasks!.Should().NotBeEmpty();
        
        var getTaskModel = await _client.GetAsync("/tasks/1");
        
        getTaskModel.StatusCode.Should().Be(HttpStatusCode.OK);
        var task = await getTaskModel.Content.ReadFromJsonAsync<TaskModel>(_jsonOptions);
        task.Should().NotBeNull();
        
        var boardResponse = await _client.GetAsync("/tasks/board/1");
        
        boardResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var createRequest = new AddTaskRequest(
            ProjectId: 1,
            Title: "Viewer task",
            Description: "This should fail",
            Priority: TaskPriority.Low,
            AssignedToUserId: null,
            DueDate: null);
        
        var createResponse = await _client.PostAsJsonAsync("/tasks", createRequest);
        
        createResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        
        var updateRequest = new UpdateTaskRequest(
            Title: "Updated by viewer",
            Description: "Should fail",
            Priority: TaskPriority.High,
            AssignedToUserId: null,
            DueDate: null,
            RowVersion: 1);
        
        var updateResponse = await _client.PutAsJsonAsync("/tasks/1", updateRequest);
        
        updateResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        
        var deleteResponse = await _client.DeleteAsync("/tasks/1");
        
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        
        var statusRequest = new { status = "InProgress" };
        var statusResponse = await _client.PatchAsJsonAsync("/tasks/1/status", statusRequest);
        
        statusResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        
        var assignRequest = new { assignedToUserId = 20 };
        var assignResponse = await _client.PatchAsJsonAsync("/tasks/1/assign", assignRequest);
        
        assignResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PreventConcurrentUpdates_Returns409()
    {
        _client.DefaultRequestHeaders.Add("X-User-Id", "10");
        
        var getResponse = await _client.GetAsync("/tasks/1");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var currentTask = await getResponse.Content.ReadFromJsonAsync<TaskModel>(_jsonOptions);
        currentTask.Should().NotBeNull();
        
        var currentRowVersion = currentTask!.RowVersion;
        
        var firstUpdate = new UpdateTaskRequest(
            Title: "First update",
            Description: currentTask.Description,
            Priority: currentTask.Priority,
            AssignedToUserId: currentTask.AssignedToUserId,
            DueDate: currentTask.DueDate,
            RowVersion: currentRowVersion);
        
        var firstResponse = await _client.PutAsJsonAsync("/tasks/1", firstUpdate);
        
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedTask = await firstResponse.Content.ReadFromJsonAsync<TaskModel>(_jsonOptions);
        updatedTask!.RowVersion.Should().BeGreaterThan(currentRowVersion);
        
        var secondUpdate = new UpdateTaskRequest(
            Title: "Second update with stale version",
            Description: currentTask.Description,
            Priority: currentTask.Priority,
            AssignedToUserId: currentTask.AssignedToUserId,
            DueDate: currentTask.DueDate,
            RowVersion: currentRowVersion);
        
        var secondResponse = await _client.PutAsJsonAsync("/tasks/1", secondUpdate);
        
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        
        var problemDetails = await secondResponse.Content.ReadAsStringAsync();
        problemDetails.Should().Contain("modified by another user");
    }

    [Fact]
    public async Task PreventMemberFromEditingOthersTask()
    {
        _client.DefaultRequestHeaders.Add("X-User-Id", "20");
        
        var getResponse = await _client.GetAsync("/tasks/1");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var task = await getResponse.Content.ReadFromJsonAsync<TaskModel>(_jsonOptions);
        task!.CreatedBy.Should().NotBe(20);
        task.AssignedToUserId.Should().NotBe(20);
        
        var updateRequest = new UpdateTaskRequest(
            Title: "Member trying to update",
            Description: task.Description,
            Priority: task.Priority,
            AssignedToUserId: task.AssignedToUserId,
            DueDate: task.DueDate,
            RowVersion: task.RowVersion);
        
        var updateResponse = await _client.PutAsJsonAsync("/tasks/1", updateRequest);
        
        updateResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        
        var deleteResponse = await _client.DeleteAsync("/tasks/1");
        
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        
        var statusRequest = new { status = "InProgress" };
        var statusResponse = await _client.PatchAsJsonAsync("/tasks/1/status", statusRequest);
        
        statusResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AllowMemberToEditAssignedTask()
    {
        _client.DefaultRequestHeaders.Add("X-User-Id", "20");
        
        var getResponse = await _client.GetAsync("/tasks/4");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var task = await getResponse.Content.ReadFromJsonAsync<TaskModel>(_jsonOptions);
        task!.AssignedToUserId.Should().Be(20);
        
        var updateRequest = new UpdateTaskRequest(
            Title: "Member updated assigned task",
            Description: "Updated by assignee",
            Priority: task.Priority,
            AssignedToUserId: 20,
            DueDate: task.DueDate,
            RowVersion: task.RowVersion);
        
        var updateResponse = await _client.PutAsJsonAsync("/tasks/4", updateRequest);
        
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedTask = await updateResponse.Content.ReadFromJsonAsync<TaskModel>(_jsonOptions);
        updatedTask!.Title.Should().Be("Member updated assigned task");
        
        var statusRequest = new { status = "Done" };
        var statusResponse = await _client.PatchAsJsonAsync("/tasks/4/status", statusRequest);
        
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

