using MsAuth.Domain.Services;

namespace MsAuth.Application.UseCases.Auth;

public sealed class LogoutUseCase : ILogoutUseCase
{
    private readonly IRefreshTokenService _refreshTokenService;

    public LogoutUseCase(IRefreshTokenService refreshTokenService)
    {
        _refreshTokenService = refreshTokenService;
    }

    public async Task ExecuteAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        await _refreshTokenService.RevokeAsync(refreshToken, cancellationToken);
    }
}
