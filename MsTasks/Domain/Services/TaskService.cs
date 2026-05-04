using MsTasks.Application.Models;
using MsTasks.Domain.Exceptions;
using MsTasks.Infrastructure.HttpClients;
using MsTasks.Infrastructure.Repositories;

namespace MsTasks.Domain.Services;

public sealed class TaskService(
    ITaskRepository taskRepository,
    IProjectHttpClient projectHttpClient,
    ILogger<TaskService> logger) : ITaskService
{
    private static readonly string[] OwnerAdminRoles = ["Owner", "Admin"];
    private static readonly string[] MemberRoles = ["Owner", "Admin", "Member"];

    public async Task<TaskModel> CreateAsync(AddTaskRequest request, int currentUserId, CancellationToken cancellationToken = default)
    {
        var role = await GetUserRoleInProjectAsync(request.ProjectId, currentUserId, cancellationToken);
        
        if (!MemberRoles.Contains(role))
        {
            throw new UnauthorizedAccessException($"User does not have permission to create tasks in project {request.ProjectId}");
        }

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 200)
        {
            throw new ArgumentException("Title is required and must not exceed 200 characters");
        }

        if (request.Priority.HasValue && !Enum.IsDefined(request.Priority.Value))
        {
            throw new ArgumentException($"Invalid priority: {request.Priority}. Must be one of: High, Medium, Low");
        }

        if (request.AssignedToUserId.HasValue)
        {
            var assignedRole = await GetUserRoleInProjectAsync(request.ProjectId, request.AssignedToUserId.Value, cancellationToken);
            if (assignedRole == null)
            {
                throw new ArgumentException($"User {request.AssignedToUserId.Value} is not a member of project {request.ProjectId}");
            }
        }

        var task = new TaskModel(
            IdTask: 0,
            Title: request.Title,
            Description: request.Description,
            ProjectId: request.ProjectId,
            AssignedToUserId: request.AssignedToUserId,
            Priority: request.Priority ?? TaskPriority.Medium,
            Status: TaskStatus.Todo,
            DueDate: request.DueDate,
            CreatedBy: currentUserId,
            RowVersion: 0,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow
        );

        var createdTask = await taskRepository.CreateAsync(task, cancellationToken);

        logger.LogInformation("Task {TaskId} created by user {UserId} in project {ProjectId}", 
            createdTask.IdTask, currentUserId, request.ProjectId);

        return createdTask;
    }

    public async Task<TaskModel?> GetByIdAsync(int taskId, int currentUserId, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null) return null;

        var role = await GetUserRoleInProjectAsync(task.ProjectId, currentUserId, cancellationToken);
        if (role == null)
        {
            throw new UnauthorizedAccessException($"User does not have access to task {taskId}");
        }

        return task;
    }

    public async Task<IEnumerable<TaskModel>> GetByProjectIdAsync(
        int projectId, 
        int currentUserId, 
        TaskStatus? status = null, 
        TaskPriority? priority = null, 
        int? assignedToUserId = null, 
        CancellationToken cancellationToken = default)
    {
        var role = await GetUserRoleInProjectAsync(projectId, currentUserId, cancellationToken);
        if (role == null)
        {
            throw new UnauthorizedAccessException($"User does not have access to project {projectId}");
        }

        if (status.HasValue && !Enum.IsDefined(status.Value))
        {
            throw new ArgumentException($"Invalid status: {status}");
        }

        if (priority.HasValue && !Enum.IsDefined(priority.Value))
        {
            throw new ArgumentException($"Invalid priority: {priority}");
        }

        return await taskRepository.GetByProjectIdAsync(projectId, status, priority, assignedToUserId, cancellationToken);
    }

    public async Task<TaskModel> UpdateAsync(int taskId, UpdateTaskRequest request, int currentUserId, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null)
        {
            throw new TaskNotFoundException(taskId);
        }

        var role = await GetUserRoleInProjectAsync(task.ProjectId, currentUserId, cancellationToken);

        if (!OwnerAdminRoles.Contains(role))
        {
            if (role != "Member" || (task.CreatedBy != currentUserId && task.AssignedToUserId != currentUserId))
            {
                throw new UnauthorizedAccessException($"User does not have permission to update task {taskId}");
            }
        }

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 200)
        {
            throw new ArgumentException("Title is required and must not exceed 200 characters");
        }

        if (!Enum.IsDefined(request.Priority))
        {
            throw new ArgumentException($"Invalid priority: {request.Priority}");
        }

        if (request.AssignedToUserId.HasValue)
        {
            var assignedRole = await GetUserRoleInProjectAsync(task.ProjectId, request.AssignedToUserId.Value, cancellationToken);
            if (assignedRole == null)
            {
                throw new ArgumentException($"User {request.AssignedToUserId.Value} is not a member of project {task.ProjectId}");
            }
        }

        try
        {
            var updatedTask = new TaskModel(
                IdTask: taskId,
                Title: request.Title,
                Description: request.Description,
                ProjectId: task.ProjectId,
                AssignedToUserId: request.AssignedToUserId,
                Priority: request.Priority,
                Status: task.Status,
                DueDate: request.DueDate,
                CreatedBy: task.CreatedBy,
                RowVersion: request.RowVersion,
                CreatedAt: task.CreatedAt,
                UpdatedAt: DateTime.UtcNow
            );

            var rowsAffected = await taskRepository.UpdateAsync(updatedTask, cancellationToken);
            
            if (rowsAffected == 0)
            {
                throw new InvalidOperationException("Task was modified by another user. RowVersion mismatch");
            }

            var result = await taskRepository.GetByIdAsync(taskId, cancellationToken);
            logger.LogInformation("Task {TaskId} updated by user {UserId}", taskId, currentUserId);
            return result!;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("RowVersion"))
        {
            throw new ConcurrencyException();
        }
    }

    public async Task DeleteAsync(int taskId, int currentUserId, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null)
        {
            throw new TaskNotFoundException(taskId);
        }

        var role = await GetUserRoleInProjectAsync(task.ProjectId, currentUserId, cancellationToken);

        if (!OwnerAdminRoles.Contains(role))
        {
            if (role != "Member" || (task.CreatedBy != currentUserId && task.AssignedToUserId != currentUserId))
            {
                throw new UnauthorizedAccessException($"User does not have permission to delete task {taskId}");
            }
        }

        await taskRepository.SoftDeleteAsync(taskId, cancellationToken);
        logger.LogInformation("Task {TaskId} deleted by user {UserId}", taskId, currentUserId);
    }

    public async Task<TaskModel> AssignAsync(int taskId, AssignTaskRequest request, int currentUserId, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null)
        {
            throw new TaskNotFoundException(taskId);
        }

        var role = await GetUserRoleInProjectAsync(task.ProjectId, currentUserId, cancellationToken);

        if (!OwnerAdminRoles.Contains(role))
        {
            throw new UnauthorizedAccessException($"Only Owner or Admin can assign tasks in project {task.ProjectId}");
        }

        if (request.AssignedToUserId.HasValue)
        {
            var assignedRole = await GetUserRoleInProjectAsync(task.ProjectId, request.AssignedToUserId.Value, cancellationToken);
            if (assignedRole == null)
            {
                throw new ArgumentException($"User {request.AssignedToUserId.Value} is not a member of project {task.ProjectId}");
            }
        }

        await taskRepository.UpdateAssignmentAsync(taskId, request.AssignedToUserId, cancellationToken);
        
        var updatedTask = await taskRepository.GetByIdAsync(taskId, cancellationToken);
        logger.LogInformation("Task {TaskId} assigned to user {AssignedToUserId} by user {UserId}", 
            taskId, request.AssignedToUserId, currentUserId);

        return updatedTask!;
    }

    public async Task<TaskModel> ChangeStatusAsync(int taskId, ChangeStatusRequest request, int currentUserId, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null)
        {
            throw new TaskNotFoundException(taskId);
        }

        if (!Enum.IsDefined(request.Status))
        {
            throw new ArgumentException($"Invalid status: {request.Status}. Must be one of: {string.Join(", ", Enum.GetNames<TaskStatus>())}");
        }

        var role = await GetUserRoleInProjectAsync(task.ProjectId, currentUserId, cancellationToken);

        if (!OwnerAdminRoles.Contains(role))
        {
            if (role == "Viewer")
            {
                throw new UnauthorizedAccessException($"Viewers cannot change task status in project {task.ProjectId}");
            }
            
            if (role == "Member" && task.CreatedBy != currentUserId && task.AssignedToUserId != currentUserId)
            {
                throw new UnauthorizedAccessException($"Members can only change status of own or assigned tasks");
            }
        }

        await taskRepository.UpdateStatusAsync(taskId, request.Status, cancellationToken);
        
        var updatedTask = await taskRepository.GetByIdAsync(taskId, cancellationToken);
        logger.LogInformation("Task {TaskId} status changed to {Status} by user {UserId}", 
            taskId, request.Status, currentUserId);

        return updatedTask!;
    }

    public async Task<BoardResponse> GetBoardAsync(int projectId, int currentUserId, CancellationToken cancellationToken = default)
    {
        var role = await GetUserRoleInProjectAsync(projectId, currentUserId, cancellationToken);
        if (role == null)
        {
            throw new UnauthorizedAccessException($"User does not have access to project {projectId}");
        }

        var tasks = await taskRepository.GetBoardByProjectIdAsync(projectId, cancellationToken);

        var columns = new Dictionary<string, List<TaskModel>>();
        foreach (var status in Enum.GetValues<TaskStatus>())
        {
            columns[status.ToString()] = tasks.Where(t => t.Status == status).ToList();
        }

        logger.LogInformation("Board for project {ProjectId} retrieved by user {UserId}", projectId, currentUserId);
        return new BoardResponse(columns);
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
