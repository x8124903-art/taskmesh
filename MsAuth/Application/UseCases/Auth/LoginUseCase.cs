using MsAuth.Application.Models;
using MsAuth.Domain.Services;

namespace MsAuth.Application.UseCases.Auth;

public sealed class LoginUseCase : ILoginUseCase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;

    public LoginUseCase(
        IUserService userService,
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService)
    {
        _userService = userService;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<LoginResponse> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userService.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid credentials");

        var accessToken = _jwtService.GenerateToken(user);
        var refreshToken = Guid.NewGuid().ToString();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        await _refreshTokenService.AddAsync(
            new RefreshTokenModel(0, refreshToken, user.IdUser, expiresAt, false, DateTime.UtcNow, null),
            cancellationToken);

        var userResponse = new UserResponse(user.IdUser, user.Email, user.Name, user.CreatedAt);
        return new LoginResponse(accessToken, refreshToken, userResponse);
    }
}
