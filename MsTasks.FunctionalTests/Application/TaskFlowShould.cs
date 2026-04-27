using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MsTasks.Application.Models;
using MsTasks.Domain;

namespace MsTasks.FunctionalTests.Application;

[Collection(nameof(ServerFixtureCollection))]
public sealed class TaskFlowShould(ServerFixture fixture)
{
    private readonly HttpClient _client = fixture.CreateClient();
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task CompleteFullTaskFlow_FromCreationToDone()
    {
        _client.DefaultRequestHeaders.Add("X-User-Id", "10");

        var createRequest = new AddTaskRequest(
            ProjectId: 1,
            Title: "E2E Test Task",
            Description: "Full flow test task",
            Priority: TaskPriority.High,
            AssignedToUserId: 20,
            DueDate: DateTime.UtcNow.AddDays(7));
        
        var createResponse = await _client.PostAsJsonAsync("/tasks", createRequest);
        
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        createResponse.Headers.Location.Should().NotBeNull();
        
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskModel>(_jsonOptions);
        createdTask.Should().NotBeNull();
        createdTask!.IdTask.Should().BeGreaterThan(0);
        createdTask.Title.Should().Be("E2E Test Task");
        createdTask.Status.Should().Be(TaskStatus.Todo);
        createdTask.AssignedToUserId.Should().Be(20);
        
        var taskId = createdTask.IdTask;
        
        _client.DefaultRequestHeaders.Remove("X-User-Id");
        _client.DefaultRequestHeaders.Add("X-User-Id", "20"); // Member
        
        var statusRequest = new { status = "InProgress" };
        var statusResponse = await _client.PatchAsJsonAsync($"/tasks/{taskId}/status", statusRequest);
        
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedTask = await statusResponse.Content.ReadFromJsonAsync<TaskModel>(_jsonOptions);
        updatedTask!.Status.Should().Be(TaskStatus.InProgress);
        
        var commentRequest = new AddTaskCommentRequest(Comment: "Started working on this task");
        var commentResponse = await _client.PostAsJsonAsync($"/tasks/{taskId}/comments", commentRequest);
        
        commentResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdComment = await commentResponse.Content.ReadFromJsonAsync<TaskCommentModel>(_jsonOptions);
        createdComment.Should().NotBeNull();
        createdComment!.Comment.Should().Be("Started working on this task");
        createdComment.UserId.Should().Be(20);
        
        var doneRequest = new { status = "Done" };
        var doneResponse = await _client.PatchAsJsonAsync($"/tasks/{taskId}/status", doneRequest);
        
        doneResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var doneTask = await doneResponse.Content.ReadFromJsonAsync<TaskModel>(_jsonOptions);
        doneTask!.Status.Should().Be(TaskStatus.Done);
        
        var boardResponse = await _client.GetAsync("/tasks/board/1");
        
        boardResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var board = await boardResponse.Content.ReadFromJsonAsync<BoardResponse>(_jsonOptions);
        board.Should().NotBeNull();
        board!.Columns["Done"].Should().Contain(t => t.IdTask == taskId);
        board.Columns["Done"].First(t => t.IdTask == taskId).Title.Should().Be("E2E Test Task");
    }

    [Fact]
    public async Task AllowMemberToCreateAndManageOwnTask()
    {
        _client.DefaultRequestHeaders.Add("X-User-Id", "20");

        var createRequest = new AddTaskRequest(
            ProjectId: 1,
            Title: "Member's own task",
            Description: "Task created by member",
            Priority: TaskPriority.Medium,
            AssignedToUserId: 20,
            DueDate: DateTime.UtcNow.AddDays(3));
        
        var createResponse = await _client.PostAsJsonAsync("/tasks", createRequest);
        
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskModel>(_jsonOptions);
        createdTask.Should().NotBeNull();
        
        var taskId = createdTask!.IdTask;
        
        var updateRequest = new UpdateTaskRequest(
            Title: "Updated member task",
            Description: "Updated description",
            Priority: TaskPriority.High,
            AssignedToUserId: 20,
            DueDate: DateTime.UtcNow.AddDays(5),
            RowVersion: createdTask.RowVersion);
        
        var updateResponse = await _client.PutAsJsonAsync($"/tasks/{taskId}", updateRequest);
        
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedTask = await updateResponse.Content.ReadFromJsonAsync<TaskModel>(_jsonOptions);
        updatedTask!.Title.Should().Be("Updated member task");
        
        var deleteResponse = await _client.DeleteAsync($"/tasks/{taskId}");
        
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var getResponse = await _client.GetAsync($"/tasks/{taskId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task VerifyBoardContainsTasksInAllStatuses()
    {
        _client.DefaultRequestHeaders.Add("X-User-Id", "10");
        
        var boardResponse = await _client.GetAsync("/tasks/board/1");
        
        boardResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var board = await boardResponse.Content.ReadFromJsonAsync<BoardResponse>(_jsonOptions);
        board.Should().NotBeNull();
        
        board!.Columns["Todo"].Should().NotBeEmpty();
        board.Columns["InProgress"].Should().NotBeEmpty();
        board.Columns["Review"].Should().NotBeEmpty();
        board.Columns["Testing"].Should().NotBeEmpty();
        board.Columns["Done"].Should().NotBeEmpty();
        board.Columns["Blocked"].Should().NotBeEmpty();
        
        var totalTasks = board.Columns.Values.Sum(col => col.Count);
        totalTasks.Should().BeGreaterThanOrEqualTo(10); 
    }
}
