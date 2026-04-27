using Dapper;
using Microsoft.Extensions.DependencyInjection;
using MsTasks.Application.Models;
using MsTasks.Infrastructure.Repositories;
using MsTasks.IntegrationTests.Fixtures;
using MySqlConnector;

namespace MsTasks.IntegrationTests.Infrastructure.Repositories;

[Collection(nameof(MsTasksFixtureCollection))]
public sealed class TaskCommentSqlRepositoryShould
{
    private readonly MsTasksFixture _fixture;
    private readonly ITaskCommentRepository _repository;
    private readonly string _connectionString;

    public TaskCommentSqlRepositoryShould(MsTasksFixture fixture)
    {
        _fixture = fixture;
        _connectionString = _fixture.ConnectionString;

        var scope = _fixture.Services.CreateScope();
        _repository = scope.ServiceProvider.GetRequiredService<ITaskCommentRepository>();
    }

    [Fact]
    public async Task CreateAsync_InsertsCommentSuccessfully()
    {
        // Arrange
        const int TASK_ID = 1;
        const int USER_ID = 20;
        const string COMMENT_TEXT = "This is a repository test comment";

        var initialCount = await _fixture.CountTaskCommentsAsync(TASK_ID);

        // Act
        var createdComment = await _repository.CreateAsync(TASK_ID, USER_ID, COMMENT_TEXT, CancellationToken.None);

        // Assert
        createdComment.Should().NotBeNull();
        createdComment.IdTaskComment.Should().BeGreaterThan(0);
        createdComment.TaskId.Should().Be(TASK_ID);
        createdComment.UserId.Should().Be(USER_ID);
        createdComment.Comment.Should().Be(COMMENT_TEXT);

        var finalCount = await _fixture.CountTaskCommentsAsync(TASK_ID);
        finalCount.Should().Be(initialCount + 1);

        // Verify in database
        await using var connection = new MySqlConnection(_connectionString);
        var dbComment = await connection.QueryFirstOrDefaultAsync(
            "SELECT Comment FROM TaskComment WHERE IdTaskComment = @Id",
            new { Id = createdComment.IdTaskComment });

        Assert.NotNull(dbComment);
        string commentText = dbComment.Comment;
        commentText.Should().Be(COMMENT_TEXT);
    }

    [Fact]
    public async Task GetByTaskIdAsync_ReturnsCommentsOrderedByCreatedAtDesc()
    {
        // Arrange
        const int TASK_ID = 1;

        // Act
        var comments = await _repository.GetByTaskIdAsync(TASK_ID, CancellationToken.None);

        // Assert
        comments.Should().NotBeEmpty();
        comments.Should().OnlyContain(c => c.TaskId == TASK_ID);

        var orderedComments = comments.OrderByDescending(c => c.CreatedAt).ToList();
        comments.Should().BeEquivalentTo(orderedComments, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetByTaskIdAsync_ReturnsEmptyList_WhenTaskHasNoComments()
    {
        // Arrange
        const int TASK_ID = 4;

        // Act
        var comments = await _repository.GetByTaskIdAsync(TASK_ID, CancellationToken.None);

        // Assert
        comments.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByTaskIdAsync_ReturnsMultipleComments_WhenTaskHasMany()
    {
        // Arrange
        const int TASK_ID = 6;

        // Act
        var comments = await _repository.GetByTaskIdAsync(TASK_ID, CancellationToken.None);

        // Assert
        comments.Should().NotBeEmpty();
        comments.Should().HaveCountGreaterOrEqualTo(3);
        comments.Should().OnlyContain(c => c.TaskId == TASK_ID);
    }

    [Fact]
    public async Task CreateAsync_PreservesAllFields()
    {
        // Arrange
        const int TASK_ID = 10;
        const int USER_ID = 10;
        const string COMMENT_TEXT = "Detailed comment for field preservation test";

        // Act
        var createdComment = await _repository.CreateAsync(TASK_ID, USER_ID, COMMENT_TEXT, CancellationToken.None);

        // Assert
        createdComment.TaskId.Should().Be(TASK_ID);
        createdComment.UserId.Should().Be(USER_ID);
        createdComment.Comment.Should().Be(COMMENT_TEXT);
        createdComment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetByTaskIdAsync_ReturnsCorrectUserId()
    {
        // Arrange
        const int TASK_ID = 11;
        const int USER_ID = 20;
        const string COMMENT_TEXT = "Comment by user 20";

        await _repository.CreateAsync(TASK_ID, USER_ID, COMMENT_TEXT, CancellationToken.None);

        // Act
        var comments = await _repository.GetByTaskIdAsync(TASK_ID, CancellationToken.None);

        // Assert
        var createdComment = comments.FirstOrDefault(c => c.Comment == "Comment by user 20");
        createdComment.Should().NotBeNull();
        createdComment!.UserId.Should().Be(USER_ID);
    }
}
