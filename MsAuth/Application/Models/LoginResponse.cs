namespace MsAuth.Application.Models
{
    public sealed record LoginResponse(string AccessToken, string RefreshToken, UserResponse User);
}