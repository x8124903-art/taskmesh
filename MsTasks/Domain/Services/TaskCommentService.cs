using MsTasks.Application.Models;
using MsTasks.Domain.Events;
using MsTasks.Domain.Exceptions;
using MsTasks.Infrastructure.EventBus;
using MsTasks.Infrastructure.HttpClients;
using MsTasks.Infrastructure.Repositories;

namespace MsTasks.Domain.Services;

public sealed class TaskCommentService(
    ITaskRepository taskRepository,
    ITaskCommentRepository taskCommentRepository,
    IProjectHttpClient projectHttpClient,
    IEventBus eventBus,
    ILogger<TaskCommentService> logger) : ITaskCommentService
{
    private static readonly string[] AllowedRolesForCreation = ["Owner", "Admin", "Member"];

    public async Task<TaskCommentModel> AddCommentAsync(int taskId, AddTaskCommentRequest request, int currentUserId, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null)
        {
            throw new TaskNotFoundException(taskId);
        }

        var role = await GetUserRoleInProjectAsync(task.ProjectId, currentUserId, cancellationToken);
        
        if (!AllowedRolesForCreation.Contains(role))
        {
            throw new UnauthorizedAccessException($"User does not have permission to add comments in project {task.ProjectId}");
        }

        if (string.IsNullOrWhiteSpace(request.Comment) || request.Comment.Length > 2000)
        {
            throw new ArgumentException("Comment is required and must not exceed 2000 characters");
        }

        var comment = await taskCommentRepository.CreateAsync(taskId, currentUserId, request.Comment, cancellationToken);
        logger.LogInformation("Comment {CommentId} added to task {TaskId} by user {UserId}", 
            comment.IdTaskComment, taskId, currentUserId);

        try
        {
            var evt = new TaskCommentAddedEvent(
                EventId: Guid.NewGuid().ToString(),
                OccurredAt: DateTime.UtcNow,
                TaskId: taskId,
                ProjectId: task.ProjectId,
                CommentId: comment.IdTaskComment,
                AuthorUserId: currentUserId,
                TaskTitle: task.Title,
                TaskCreatedByUserId: task.CreatedBy,
                TaskAssignedToUserId: task.AssignedToUserId
            );
            await eventBus.PublishAsync(evt, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish TaskCommentAddedEvent for task {TaskId}", taskId);
        }

        return comment;
    }

    public async Task<IEnumerable<TaskCommentModel>> GetCommentsAsync(int taskId, int currentUserId, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null)
        {
            throw new TaskNotFoundException(taskId);
        }

        var role = await GetUserRoleInProjectAsync(task.ProjectId, currentUserId, cancellationToken);
        if (role == null)
        {
            throw new UnauthorizedAccessException($"User does not have access to task {taskId}");
        }

        return await taskCommentRepository.GetByTaskIdAsync(taskId, cancellationToken);
    }

    private async Task<string?> GetUserRoleInProjectAsync(int projectId, int userId, CancellationToken cancellationToken)
    {
        try
        {
            return await projectHttpClient.GetUserRoleInProjectAsync(projectId, userId, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to verify user {UserId} role in project {ProjectId}. ms-projects unavailable", 
                userId, projectId);
            throw new InvalidOperationException("Cannot verify permissions: project service unavailable", ex);
        }
    }
}
