using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using WebApp.Models;
using WebApp.Models.Auth;

namespace WebApp.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<AuthService> _logger;

    public AuthService(HttpClient httpClient, IJSRuntime jsRuntime, ILogger<AuthService> logger)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public User? CurrentUser { get; set; }
    public bool IsAuthenticated => CurrentUser != null;

    public async Task<bool> InitializeAsync()
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "accessToken");
            if (!string.IsNullOrEmpty(token))
            {
                var claims = ParseJwtClaims(token);
                if (claims != null)
                {
                    CurrentUser = new User
                    {
                        Id = int.Parse(claims.GetValueOrDefault("sub", "0")),
                        Email = claims.GetValueOrDefault("email", ""),
                        Name = claims.GetValueOrDefault("name", "")
                    };
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing authentication from stored token");
        }
        return false;
    }

    private Dictionary<string, string>? ParseJwtClaims(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length != 3)
                return null;

            var payload = parts[1];
            var padLength = 4 - (payload.Length % 4);
            if (padLength < 4)
                payload += new string('=', padLength);

            var jsonBytes = Convert.FromBase64String(payload);
            var jsonString = System.Text.Encoding.UTF8.GetString(jsonBytes);
            
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, System.Text.Json.JsonElement>>(jsonString)
                ?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = password });
            if (!response.IsSuccessStatusCode)
                return false;
            
            var loginData = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (loginData == null)
                return false;
            
            CurrentUser = loginData.User;
            
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "accessToken", loginData.AccessToken);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "refreshToken", loginData.RefreshToken);
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RegisterAsync(string email, string name, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/register", 
                new { Email = email, Name = name, Password = password });
            
            if (!response.IsSuccessStatusCode)
                return false;
            
            var registerData = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (registerData == null)
                return false;
            
            CurrentUser = registerData.User;
            
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "accessToken", registerData.AccessToken);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "refreshToken", registerData.RefreshToken);
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        CurrentUser = null;
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "accessToken");
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "refreshToken");
    }

    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "refreshToken");
            if (string.IsNullOrEmpty(refreshToken))
                return false;

            var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/refresh", new { RefreshToken = refreshToken });
            if (!response.IsSuccessStatusCode)
                return false;

            var data = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (data == null)
                return false;

            CurrentUser = data.User;
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "accessToken", data.AccessToken);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "refreshToken", data.RefreshToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return false;
        }
    }
}
