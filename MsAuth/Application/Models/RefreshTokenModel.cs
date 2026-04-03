namespace MsAuth.Application.Models;

public sealed record RefreshTokenModel(
    int IdRefreshToken,
    string Token,
    int UserId,
    DateTime ExpiresAt,
    bool IsRevoked,
    DateTime CreatedAt,
    DateTime? RevokedAt
);
