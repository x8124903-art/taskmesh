using MsAuth.Application.Models;
using MsAuth.Infrastructure.Repositories;

namespace MsAuth.Domain.Services;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _repository;
    public RefreshTokenService(IRefreshTokenRepository repository) => _repository = repository;

    public Task<RefreshTokenModel?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        => _repository.GetByTokenAsync(token, cancellationToken);

    public Task<int> AddAsync(RefreshTokenModel model, CancellationToken cancellationToken = default)
        => _repository.AddAsync(model, cancellationToken);

    public Task RevokeAsync(string token, CancellationToken cancellationToken = default)
        => _repository.RevokeAsync(token, cancellationToken);
}
