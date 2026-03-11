using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace WebApp.Services;

public class AuthTokenHandler : DelegatingHandler
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuthTokenHandler> _logger;

    public AuthTokenHandler(IJSRuntime jsRuntime, IServiceProvider serviceProvider, ILogger<AuthTokenHandler> logger)
    {
        _jsRuntime = jsRuntime;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await AttachTokenAsync(request);
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized && !IsRefreshRequest(request))
        {
            var authService = _serviceProvider.GetRequiredService<IAuthService>();
            var refreshed = await authService.RefreshTokenAsync();
            if (refreshed)
            {
                var retry = await CloneRequestAsync(request);
                await AttachTokenAsync(retry);
                response = await base.SendAsync(retry, cancellationToken);
            }
        }

        return response;
    }

    private async Task AttachTokenAsync(HttpRequestMessage request)
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "accessToken");
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token from localStorage.");
        }
    }

    private static bool IsRefreshRequest(HttpRequestMessage request) =>
        request.RequestUri?.AbsolutePath.Contains("/auth/refresh", StringComparison.OrdinalIgnoreCase) == true;

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage original)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri);
        if (original.Content != null)
        {
            var content = await original.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(content);
            if (original.Content.Headers.ContentType != null)
                clone.Content.Headers.ContentType = original.Content.Headers.ContentType;
        }
        foreach (var header in original.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        return clone;
    }
}
