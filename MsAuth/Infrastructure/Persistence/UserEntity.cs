namespace MsAuth.Infrastructure.Persistence;

internal sealed record UserEntity(
    int IdUser,
    string Email,
    string Name,
    string PasswordHash,
    bool IsDeleted,
    DateTime CreatedAt);
