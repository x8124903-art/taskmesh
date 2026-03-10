namespace MsAuth.Infrastructure.Repositories;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
internal static class QueriesMySql
{
    internal const string GetById = @"
        SELECT IdUser, Email, Name, PasswordHash, IsDeleted, CreatedAt
        FROM User
        WHERE IdUser = @IdUser AND IsDeleted = FALSE
        LIMIT 1;";

    internal const string GetByEmail = @"
        SELECT IdUser, Email, Name, PasswordHash, IsDeleted, CreatedAt
        FROM User
        WHERE Email = @Email AND IsDeleted = FALSE
        LIMIT 1;";

    internal const string Insert = @"
        INSERT INTO User (Email, Name, PasswordHash, IsDeleted, CreatedAt)
        VALUES (@Email, @Name, @PasswordHash, FALSE, @CreatedAt);
        SELECT LAST_INSERT_ID();";

    internal const string GetRefreshToken = @"
        SELECT IdRefreshToken, Token, UserId, ExpiresAt, IsRevoked, CreatedAt, RevokedAt
        FROM RefreshToken
        WHERE Token = @Token
        LIMIT 1;";

    internal const string InsertRefreshToken = @"
        INSERT INTO RefreshToken (Token, UserId, ExpiresAt, IsRevoked, CreatedAt)
        VALUES (@Token, @UserId, @ExpiresAt, @IsRevoked, @CreatedAt);
        SELECT LAST_INSERT_ID();";

    internal const string RevokeRefreshToken = @"
        UPDATE RefreshToken SET IsRevoked = TRUE, RevokedAt = @RevokedAt WHERE Token = @Token;";
}
