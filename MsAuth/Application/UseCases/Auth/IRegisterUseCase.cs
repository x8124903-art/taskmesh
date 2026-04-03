using MsAuth.Application.Models;

namespace MsAuth.Application.UseCases.Auth;

public interface IRegisterUseCase
{
    Task<RegisterResponse> ExecuteAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}
