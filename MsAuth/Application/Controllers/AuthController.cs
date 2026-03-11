using Microsoft.AspNetCore.Mvc;
using MsAuth.Application.Models;
using MsAuth.Application.UseCases.Auth;

namespace MsAuth.Application.Controllers
{
    [ApiController]
    [Route("auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IRegisterUseCase _registerUseCase;
        private readonly ILoginUseCase _loginUseCase;
        private readonly IRefreshTokenUseCase _refreshTokenUseCase;
        private readonly ILogoutUseCase _logoutUseCase;

        public AuthController(
            IRegisterUseCase registerUseCase,
            ILoginUseCase loginUseCase,
            IRefreshTokenUseCase refreshTokenUseCase,
            ILogoutUseCase logoutUseCase)
        {
            _registerUseCase = registerUseCase;
            _loginUseCase = loginUseCase;
            _refreshTokenUseCase = refreshTokenUseCase;
            _logoutUseCase = logoutUseCase;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            var response = await _registerUseCase.ExecuteAsync(request, cancellationToken);
            return StatusCode(201, response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var response = await _loginUseCase.ExecuteAsync(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] Auth.RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var accessToken = await _refreshTokenUseCase.ExecuteAsync(request.RefreshToken, cancellationToken);
            return Ok(new { AccessToken = accessToken });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] Auth.LogoutRequest request, CancellationToken cancellationToken)
        {
            await _logoutUseCase.ExecuteAsync(request.RefreshToken, cancellationToken);
            return NoContent();
        }
    }
}