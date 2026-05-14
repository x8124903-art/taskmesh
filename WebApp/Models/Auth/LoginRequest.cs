using System.Diagnostics.CodeAnalysis;

namespace WebApp.Models.Auth;

[ExcludeFromCodeCoverage]
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
