using System.Diagnostics.CodeAnalysis;
using MsAuth.Application.Models;
using MsAuth.Infrastructure.Data;

namespace MsAuth.Infrastructure.Repositories;

[ExcludeFromCodeCoverage]
public sealed class RefreshTokenSqlRepository : IRefreshTokenRepository
{
    private readonly IDapperContext _context;
    public RefreshTokenSqlRepository(IDapperContext context) => _context = context;

    public async Task<RefreshTokenModel?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        return await _context.QueryFirstOrDefaultAsync<RefreshTokenModel>(
            connection,
            QueriesMySql.GetRefreshToken,
            new { Token = token },
            cancellationToken);
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
}
