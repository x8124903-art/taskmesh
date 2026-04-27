using Microsoft.AspNetCore.Mvc;
using MsTasks.Application.Models;
using MsTasks.Application.UseCases.Task;
using MsTasks.Domain;

namespace MsTasks.Application.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class TasksController(
    ICreateTaskUseCase createTask,
    IGetTaskUseCase getTask,
    IGetTasksByProjectUseCase getTasksByProject,
    IUpdateTaskUseCase updateTask,
    IDeleteTaskUseCase deleteTask,
    IAssignTaskUseCase assignTask,
    IChangeTaskStatusUseCase changeTaskStatus,
    IGetBoardTasksUseCase getBoardTasks) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int projectId,
        [FromQuery] TaskStatus? status = null,
        [FromQuery] TaskPriority? priority = null,
        [FromQuery] int? assignedToUserId = null,
        CancellationToken cancellationToken = default)
    {
        var userId = int.Parse(Request.Headers["X-User-Id"].ToString());
        var tasks = await getTasksByProject.ExecuteAsync(projectId, userId, status, priority, assignedToUserId, cancellationToken);
        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAsync([FromRoute] int id, CancellationToken cancellationToken = default)
    {
        var userId = int.Parse(Request.Headers["X-User-Id"].ToString());
        var task = await getTask.ExecuteAsync(id, userId, cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddTaskRequest body, CancellationToken cancellationToken = default)
    {
        var userId = int.Parse(Request.Headers["X-User-Id"].ToString());
        var created = await createTask.ExecuteAsync(body, userId, cancellationToken);
        return Created($"/tasks/{created.IdTask}", created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateTaskRequest body, CancellationToken cancellationToken = default)
    {
        var userId = int.Parse(Request.Headers["X-User-Id"].ToString());
        var updated = await updateTask.ExecuteAsync(id, body, userId, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken = default)
    {
        var userId = int.Parse(Request.Headers["X-User-Id"].ToString());
        await deleteTask.ExecuteAsync(id, userId, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus([FromRoute] int id, [FromBody] ChangeStatusRequest body, CancellationToken cancellationToken = default)
    {
        var userId = int.Parse(Request.Headers["X-User-Id"].ToString());
        var updated = await changeTaskStatus.ExecuteAsync(id, body, userId, cancellationToken);
        return Ok(updated);
    }

    [HttpPatch("{id:int}/assign")]
    public async Task<IActionResult> Assign([FromRoute] int id, [FromBody] AssignTaskRequest body, CancellationToken cancellationToken = default)
    {
        var userId = int.Parse(Request.Headers["X-User-Id"].ToString());
        var updated = await assignTask.ExecuteAsync(id, body, userId, cancellationToken);
        return Ok(updated);
    }

    [HttpGet("board/{projectId:int}")]
    public async Task<IActionResult> GetBoard([FromRoute] int projectId, CancellationToken cancellationToken = default)
    {
        var userId = int.Parse(Request.Headers["X-User-Id"].ToString());
        var board = await getBoardTasks.ExecuteAsync(projectId, userId, cancellationToken);
        return Ok(board);
    }
}
