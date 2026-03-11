namespace MsAuth.Application.UseCases.Auth;

public interface IRefreshTokenUseCase
{
    Task<string> ExecuteAsync(string refreshToken, CancellationToken cancellationToken = default);
}
