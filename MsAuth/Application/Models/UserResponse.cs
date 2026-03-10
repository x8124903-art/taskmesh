namespace MsAuth.Application.Models
{
    public sealed record UserResponse(int IdUser, string Email, string Name, DateTime CreatedAt);
}
