using Microsoft.AspNetCore.Mvc;
using MsAuth.Application.Models;

namespace MsAuth.Application.Controllers
{
    [ApiController]
    [Route("auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly MsAuth.Domain.Services.IUserService _service;
        private readonly MsAuth.Domain.Services.IJwtService _jwtService;
        private readonly MsAuth.Domain.Services.IRefreshTokenService _refreshTokenService;

        public AuthController(
            MsAuth.Domain.Services.IUserService service,
            MsAuth.Domain.Services.IJwtService jwtService,
            MsAuth.Domain.Services.IRefreshTokenService refreshTokenService)
        {
            _service = service;
            _jwtService = jwtService;
            _refreshTokenService = refreshTokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                var user = await _service.RegisterAsync(request, cancellationToken);
                var accessToken = _jwtService.GenerateToken(user);
                var refreshToken = Guid.NewGuid().ToString();
                var expiresAt = DateTime.UtcNow.AddDays(7);
                await _refreshTokenService.AddAsync(
                    new Application.Models.RefreshTokenModel(
                        0, refreshToken, user.IdUser, expiresAt, false, DateTime.UtcNow, null
                    ), cancellationToken);
                var userResponse = new UserResponse(user.IdUser, user.Email, user.Name, user.CreatedAt);
                var response = new Application.Models.RegisterResponse(accessToken, refreshToken, userResponse);
                return StatusCode(201, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var user = await _service.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
            if (user == null)
                return Unauthorized();
            var accessToken = _jwtService.GenerateToken(user);
            var refreshToken = Guid.NewGuid().ToString();
            var expiresAt = DateTime.UtcNow.AddDays(7);
            await _refreshTokenService.AddAsync(
                new Application.Models.RefreshTokenModel(
                    0, refreshToken, user.IdUser, expiresAt, false, DateTime.UtcNow, null
                ), cancellationToken);
            var userResponse = new UserResponse(user.IdUser, user.Email, user.Name, user.CreatedAt);
            var response = new Application.Models.LoginResponse(accessToken, refreshToken, userResponse);
            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] Application.Controllers.Auth.RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var token = await _refreshTokenService.GetByTokenAsync(request.RefreshToken, cancellationToken);
            if (token == null || token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
                return Unauthorized();
            var user = await _service.GetByIdAsync(token.UserId, cancellationToken);
            if (user == null)
                return Unauthorized();
            var accessToken = _jwtService.GenerateToken(user);
            return Ok(new { AccessToken = accessToken });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] Application.Controllers.Auth.LogoutRequest request, CancellationToken cancellationToken)
        {
            await _refreshTokenService.RevokeAsync(request.RefreshToken, cancellationToken);
            return NoContent();
        }
    }
}