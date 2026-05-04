using Microsoft.AspNetCore.Mvc;
using MsTasks.Application.Models;
using MsTasks.Application.UseCases.TaskComment;

namespace MsTasks.Application.Controllers;

[ApiController]
[Route("tasks/{taskId:int}/comments")]
public sealed class TaskCommentsController(
    IAddTaskCommentUseCase addComment,
    IGetTaskCommentsUseCase getComments) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Add([FromRoute] int taskId, [FromBody] AddTaskCommentRequest body, CancellationToken cancellationToken = default)
    {
        var userId = int.Parse(Request.Headers["X-User-Id"].ToString());
        var comment = await addComment.ExecuteAsync(taskId, body, userId, cancellationToken);
        return Created($"/tasks/{taskId}/comments/{comment.IdTaskComment}", comment);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromRoute] int taskId, CancellationToken cancellationToken = default)
    {
        var userId = int.Parse(Request.Headers["X-User-Id"].ToString());
        var comments = await getComments.ExecuteAsync(taskId, userId, cancellationToken);
        return Ok(comments);
    }
}
