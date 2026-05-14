using MsTasks.Application.Models;
using MsTasks.IntegrationTests.Fixtures;

namespace MsTasks.IntegrationTests.Application.Controllers;

[Collection(nameof(MsTasksFixtureCollection))]
public sealed class TaskCommentsControllerShould
{
    private readonly MsTasksFixture _fixture;
    private readonly HttpClient _client;

    public TaskCommentsControllerShould(MsTasksFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.HttpClient;
        _fixture.ResetMsProjectsMocks();
    }

    [Fact]
    public async Task AddComment_Returns201_WhenUserIsMember()
    {
        // Arrange
        var request = new AddTaskCommentRequest("This is a test comment from integration test");

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "20");

        var initialCount = await _fixture.CountTaskCommentsAsync(1);

        // Act
        var response = await _client.PostAsJsonAsync("/tasks/1/comments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().StartWith("/tasks/1/comments/");

        var createdComment = await response.Content.ReadFromJsonAsync<TaskCommentModel>();
        createdComment.Should().NotBeNull();
        createdComment!.Comment.Should().Be("This is a test comment from integration test");
        createdComment.UserId.Should().Be(20);
        createdComment.TaskId.Should().Be(1);

        var finalCount = await _fixture.CountTaskCommentsAsync(1);
        finalCount.Should().Be(initialCount + 1);
    }

    [Fact]
    public async Task AddComment_Returns403_WhenUserIsViewer()
    {
        // Arrange
        var request = new AddTaskCommentRequest("Viewer should not be able to comment");

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "30");

        // Act
        var response = await _client.PostAsJsonAsync("/tasks/1/comments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddComment_Returns404_WhenTaskNotFound()
    {
        // Arrange
        var request = new AddTaskCommentRequest("Comment on non-existent task");

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "10");

        // Act
        var response = await _client.PostAsJsonAsync("/tasks/99999/comments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetComments_Returns200WithList_WhenTaskHasComments()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "10");

        // Act
        var response = await _client.GetAsync("/tasks/1/comments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var comments = await response.Content.ReadFromJsonAsync<List<TaskCommentModel>>();
        comments.Should().NotBeNull();
        comments.Should().NotBeEmpty();
        comments.Should().OnlyContain(c => c.TaskId == 1);
        
        var orderedComments = comments!.OrderByDescending(c => c.CreatedAt).ToList();
        comments.Should().BeEquivalentTo(orderedComments, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetComments_Returns200WithEmptyList_WhenTaskHasNoComments()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "10");

        // Act
        var response = await _client.GetAsync("/tasks/4/comments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var comments = await response.Content.ReadFromJsonAsync<List<TaskCommentModel>>();
        comments.Should().NotBeNull();
        comments.Should().BeEmpty();
    }

    [Fact]
    public async Task GetComments_AllowsViewer_ToReadComments()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-User-Id", "30");

        // Act
        var response = await _client.GetAsync("/tasks/1/comments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var comments = await response.Content.ReadFromJsonAsync<List<TaskCommentModel>>();
        comments.Should().NotBeNull();
    }
}
