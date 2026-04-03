using System.Diagnostics.CodeAnalysis;
using MsAuth.Application.Models;
using MsAuth.Infrastructure.Data;
using MsAuth.Infrastructure.Persistence;

namespace MsAuth.Infrastructure.Repositories;

[ExcludeFromCodeCoverage]
public sealed class RefreshTokenSqlRepository : IRefreshTokenRepository
{
    private readonly IDapperContext _context;
    public RefreshTokenSqlRepository(IDapperContext context) => _context = context;

    public async Task<RefreshTokenModel?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var entity = await _context.QueryFirstOrDefaultAsync<RefreshTokenEntity>(
            connection,
            QueriesMySql.GetRefreshToken,
            new { Token = token },
            cancellationToken);
        return entity is null ? null : ToModel(entity);
    }

    public async Task<int> AddAsync(RefreshTokenModel model, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        return await _context.ExecuteScalarAsync<int>(
            connection,
            QueriesMySql.InsertRefreshToken,
            new { model.Token, model.UserId, model.ExpiresAt, model.IsRevoked, model.CreatedAt },
            cancellationToken);
    }

    public async Task RevokeAsync(string token, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        await _context.ExecuteAsync(
            connection,
            QueriesMySql.RevokeRefreshToken,
            new { Token = token, RevokedAt = DateTime.UtcNow },
            cancellationToken);
    }

    private static RefreshTokenModel ToModel(RefreshTokenEntity e) =>
        new(e.IdRefreshToken, e.Token, e.UserId, e.ExpiresAt, e.IsRevoked, e.CreatedAt, e.RevokedAt);
}
