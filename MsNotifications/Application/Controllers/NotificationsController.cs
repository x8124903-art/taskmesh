using Microsoft.AspNetCore.Mvc;
using MsNotifications.Application.UseCases.Notifications;

namespace MsNotifications.Application.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class NotificationsController(
    IGetNotificationsUseCase getNotifications,
    IGetUnreadCountUseCase getUnreadCount,
    IMarkNotificationAsReadUseCase markAsRead,
    IMarkAllNotificationsAsReadUseCase markAllAsRead,
    IDeleteNotificationUseCase deleteNotification) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? isRead,
        [FromQuery] string? type,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserIdFromHeaders();
        var result = await getNotifications.ExecuteAsync(userId, isRead, type, pageNumber, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        var userId = GetUserIdFromHeaders();
        var result = await getUnreadCount.ExecuteAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserIdFromHeaders();
        await markAsRead.ExecuteAsync(id, userId, cancellationToken);
        return NoContent();
    }

    [HttpPatch("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken = default)
    {
        var userId = GetUserIdFromHeaders();
        var updated = await markAllAsRead.ExecuteAsync(userId, cancellationToken);
        return Ok(new { UpdatedCount = updated });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserIdFromHeaders();
        await deleteNotification.ExecuteAsync(id, userId, cancellationToken);
        return NoContent();
    }

    private int GetUserIdFromHeaders()
    {
        var userIdHeader = Request.Headers["X-User-Id"].ToString();
        if (string.IsNullOrEmpty(userIdHeader) || !int.TryParse(userIdHeader, out var userId))
        {
            throw new UnauthorizedAccessException("User ID is missing or invalid");
        }
        return userId;
    }
}
