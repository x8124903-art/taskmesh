using MsAuth.Application.Models;

namespace MsAuth.Application.UseCases.Auth;

public interface ILoginUseCase
{
    Task<LoginResponse> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
