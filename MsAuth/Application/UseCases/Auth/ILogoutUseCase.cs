namespace MsAuth.Application.UseCases.Auth;

public interface ILogoutUseCase
{
    Task ExecuteAsync(string refreshToken, CancellationToken cancellationToken = default);
}
