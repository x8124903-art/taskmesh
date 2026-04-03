namespace MsAuth.Infrastructure.Persistence;

internal sealed record RefreshTokenEntity(
    int IdRefreshToken,
    string Token,
    int UserId,
    DateTime ExpiresAt,
    bool IsRevoked,
    DateTime CreatedAt,
    DateTime? RevokedAt);
