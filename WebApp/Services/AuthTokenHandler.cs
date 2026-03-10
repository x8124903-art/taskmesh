using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace WebApp.Services;

public class AuthTokenHandler : DelegatingHandler
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<AuthTokenHandler> _logger;

    public AuthTokenHandler(IJSRuntime jsRuntime, ILogger<AuthTokenHandler> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "accessToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token from localStorage. Request will proceed without authentication.");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
