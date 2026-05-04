using WebApp.Models;

namespace WebApp.Services;

public interface IAuthService
{
    User? CurrentUser { get; }
    bool IsAuthenticated { get; }
    Task<bool> InitializeAsync();
    Task EnsureInitializedAsync();
    Task<bool> LoginAsync(string email, string password);
    Task<bool> RegisterAsync(string email, string name, string password);
    Task LogoutAsync();
    Task<bool> RefreshTokenAsync();
}
