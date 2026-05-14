using System.Diagnostics.CodeAnalysis;

namespace WebApp.Models.Auth;

[ExcludeFromCodeCoverage]
public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public User? User { get; set; }
}
