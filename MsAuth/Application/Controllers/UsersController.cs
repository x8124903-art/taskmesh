using Microsoft.AspNetCore.Mvc;
using MsAuth.Domain.Services;

namespace MsAuth.Application.Controllers;

[ApiController]
[Route("users")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet("by-email")]
    public async Task<IActionResult> GetByEmail([FromQuery] string email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest();

        var user = await userService.GetByEmailAsync(email, cancellationToken);
        if (user == null)
            return NotFound();

        return Ok(new UserIdResponse(user.IdUser));
    }

    public sealed record UserIdResponse(int IdUser);
}
