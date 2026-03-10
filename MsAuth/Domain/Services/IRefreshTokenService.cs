using MsAuth.Application.Models;

namespace MsAuth.Domain.Services;

public interface IRefreshTokenService
{
    Task<RefreshTokenModel?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<int> AddAsync(RefreshTokenModel model, CancellationToken cancellationToken = default);
    Task RevokeAsync(string token, CancellationToken cancellationToken = default);
}
