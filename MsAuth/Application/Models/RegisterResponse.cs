namespace MsAuth.Application.Models;

public sealed record RegisterResponse(string AccessToken, string RefreshToken, UserResponse User);
