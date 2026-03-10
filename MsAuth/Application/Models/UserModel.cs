namespace MsAuth.Application.Models
{
    public sealed record UserModel(int IdUser, string Email, string Name, string PasswordHash, bool IsDeleted, DateTime CreatedAt);
}