using WebApp.Models.Tasks;
using WebApp.Services;
using WebApp.Tests.Helpers;

namespace WebApp.Tests.Services;

public sealed class TaskApiServiceShould
{
    private static TaskApiService CreateService(HttpMessageHandler handler)
    {
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        return new TaskApiService(client);
    }

    [Fact]
    public async Task GetByProjectAsync_ReturnsTasks()
    {
        var tasks = new List<TaskModel>
        {
            new() { IdTask = 1, Title = "Task 1" },
            new() { IdTask = 2, Title = "Task 2" }
        };
        var service = CreateService(MockHttpMessageHandler.WithJsonResponse(tasks));

        var result = await service.GetByProjectAsync(5);

        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Task 1");
    }

    [Fact]
    public async Task GetByProjectAsync_SendsGetToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new List<TaskModel>());
        var service = CreateService(handler);

        await service.GetByProjectAsync(42);

        handler.Requests.Should().ContainSingle();
        handler.Requests[0].Method.Should().Be(HttpMethod.Get);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Contain("/api/v1/tasks");
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Contain("projectId=42");
    }

    [Fact]
    public async Task GetByProjectAsync_WithStatus_IncludesStatusInQuery()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new List<TaskModel>());
        var service = CreateService(handler);

        await service.GetByProjectAsync(1, status: "InProgress");

        handler.Requests[0].RequestUri!.PathAndQuery.Should().Contain("status=InProgress");
    }

    [Fact]
    public async Task GetByProjectAsync_WithPriority_IncludesPriorityInQuery()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new List<TaskModel>());
        var service = CreateService(handler);

        await service.GetByProjectAsync(1, priority: "High");

        handler.Requests[0].RequestUri!.PathAndQuery.Should().Contain("priority=High");
    }

    [Fact]
    public async Task GetByProjectAsync_WithAssignedTo_IncludesAssignedToInQuery()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new List<TaskModel>());
        var service = CreateService(handler);

        await service.GetByProjectAsync(1, assignedToUserId: 10);

        handler.Requests[0].RequestUri!.PathAndQuery.Should().Contain("assignedToUserId=10");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsTask()
    {
        var task = new TaskModel { IdTask = 5, Title = "Test Task" };
        var service = CreateService(MockHttpMessageHandler.WithJsonResponse(task));

        var result = await service.GetByIdAsync(5);

        result.Should().NotBeNull();
        result!.IdTask.Should().Be(5);
        result.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task GetByIdAsync_SendsGetToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new TaskModel());
        var service = CreateService(handler);

        await service.GetByIdAsync(99);

        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/tasks/99");
    }

    [Fact]
    public async Task CreateAsync_ReturnsTask_WhenSuccess()
    {
        var task = new TaskModel { IdTask = 10, Title = "New Task" };
        var service = CreateService(MockHttpMessageHandler.WithJsonResponse(task));

        var result = await service.CreateAsync(new AddTaskRequest { Title = "New Task", ProjectId = 1 });

        result.Should().NotBeNull();
        result!.Title.Should().Be("New Task");
    }

    [Fact]
    public async Task CreateAsync_ReturnsNull_WhenFails()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.BadRequest));

        var result = await service.CreateAsync(new AddTaskRequest { Title = "X", ProjectId = 1 });

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_SendsPostToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new TaskModel());
        var service = CreateService(handler);

        await service.CreateAsync(new AddTaskRequest { Title = "Test", ProjectId = 1 });

        handler.Requests[0].Method.Should().Be(HttpMethod.Post);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/tasks");
    }

    [Fact]
    public async Task UpdateAsync_ReturnsTrue_WhenSuccess()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK));

        var result = await service.UpdateAsync(5, new UpdateTaskRequest { Title = "Updated", Priority = "Medium" });

        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenFails()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.BadRequest));

        var result = await service.UpdateAsync(5, new UpdateTaskRequest { Title = "X", Priority = "Low" });

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_SendsPutToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK);
        var service = CreateService(handler);

        await service.UpdateAsync(77, new UpdateTaskRequest { Title = "T", Priority = "High" });

        handler.Requests[0].Method.Should().Be(HttpMethod.Put);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/tasks/77");
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenSuccess()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.NoContent));

        var result = await service.DeleteAsync(5);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenFails()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.NotFound));

        var result = await service.DeleteAsync(999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.NoContent);
        var service = CreateService(handler);

        await service.DeleteAsync(33);

        handler.Requests[0].Method.Should().Be(HttpMethod.Delete);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/tasks/33");
    }

    [Fact]
    public async Task AssignAsync_ReturnsTrue_WhenSuccess()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK));

        var result = await service.AssignAsync(5, 10);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task AssignAsync_ReturnsFalse_WhenFails()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.BadRequest));

        var result = await service.AssignAsync(5, 10);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AssignAsync_SendsPatchToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK);
        var service = CreateService(handler);

        await service.AssignAsync(15, 20);

        handler.Requests[0].Method.Should().Be(HttpMethod.Patch);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/tasks/15/assign");
    }

    [Fact]
    public async Task GetBoardAsync_ReturnsBoard()
    {
        var board = new BoardResponse(new Dictionary<string, List<TaskModel>>());
        var service = CreateService(MockHttpMessageHandler.WithJsonResponse(board));

        var result = await service.GetBoardAsync(5);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBoardAsync_SendsGetToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new BoardResponse(new Dictionary<string, List<TaskModel>>()));
        var service = CreateService(handler);

        await service.GetBoardAsync(42);

        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/tasks/board/42");
    }

    [Fact]
    public async Task ChangeStatusAsync_ReturnsTrue_WhenSuccess()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK));

        var result = await service.ChangeStatusAsync(5, "Done");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ChangeStatusAsync_ReturnsFalse_WhenFails()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.BadRequest));

        var result = await service.ChangeStatusAsync(5, "Invalid");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ChangeStatusAsync_SendsPatchToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK);
        var service = CreateService(handler);

        await service.ChangeStatusAsync(25, "InProgress");

        handler.Requests[0].Method.Should().Be(HttpMethod.Patch);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/tasks/25/status");
    }

    [Fact]
    public async Task GetCommentsAsync_ReturnsComments()
    {
        var comments = new List<TaskCommentModel>
        {
            new(1, 5, 1, "User1", "Comment 1", DateTime.UtcNow),
            new(2, 5, 2, "User2", "Comment 2", DateTime.UtcNow)
        };
        var service = CreateService(MockHttpMessageHandler.WithJsonResponse(comments));

        var result = await service.GetCommentsAsync(5);

        result.Should().HaveCount(2);
        result[0].Comment.Should().Be("Comment 1");
    }

    [Fact]
    public async Task GetCommentsAsync_SendsGetToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new List<TaskCommentModel>());
        var service = CreateService(handler);

        await service.GetCommentsAsync(88);

        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/tasks/88/comments");
    }

    [Fact]
    public async Task AddCommentAsync_ReturnsComment_WhenSuccess()
    {
        var comment = new TaskCommentModel(10, 5, 1, "User", "New comment", DateTime.UtcNow);
        var service = CreateService(MockHttpMessageHandler.WithJsonResponse(comment));

        var result = await service.AddCommentAsync(5, new AddTaskCommentRequest("New comment"));

        result.Should().NotBeNull();
        result!.Comment.Should().Be("New comment");
    }

    [Fact]
    public async Task AddCommentAsync_ReturnsNull_WhenFails()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.BadRequest));

        var result = await service.AddCommentAsync(5, new AddTaskCommentRequest("X"));

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddCommentAsync_SendsPostToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new TaskCommentModel(1, 12, 1, "User", "Test", DateTime.UtcNow));
        var service = CreateService(handler);

        await service.AddCommentAsync(12, new AddTaskCommentRequest("Test"));

        handler.Requests[0].Method.Should().Be(HttpMethod.Post);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/tasks/12/comments");
    }
}
