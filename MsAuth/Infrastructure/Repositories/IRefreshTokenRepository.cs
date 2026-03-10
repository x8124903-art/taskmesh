using MsAuth.Application.Models;

namespace MsAuth.Infrastructure.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshTokenModel?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<int> AddAsync(RefreshTokenModel model, CancellationToken cancellationToken = default);
    Task RevokeAsync(string token, CancellationToken cancellationToken = default);
}
