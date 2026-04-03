using MsAuth.Domain.Services;

namespace MsAuth.Application.UseCases.Auth;

public sealed class RefreshTokenUseCase : IRefreshTokenUseCase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;

    public RefreshTokenUseCase(
        IUserService userService,
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService)
    {
        _userService = userService;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<string> ExecuteAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await _refreshTokenService.GetByTokenAsync(refreshToken, cancellationToken);
        if (token == null || token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        var user = await _userService.GetByIdAsync(token.UserId, cancellationToken);
        if (user == null)
            throw new UnauthorizedAccessException("User not found");

        return _jwtService.GenerateToken(user);
    }
}
