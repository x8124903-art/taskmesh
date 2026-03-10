using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using WebApp.Services;

namespace WebApp.Tests.Services;

public sealed class AuthTokenHandlerShould
{
    private readonly Mock<IJSRuntime> _jsRuntime = new();
    private readonly Mock<ILogger<AuthTokenHandler>> _logger = new();

    private (AuthTokenHandler handler, HttpClient client) CreateHandler()
    {
        var tokenHandler = new AuthTokenHandler(_jsRuntime.Object, _logger.Object)
        {
            InnerHandler = new TestInnerHandler()
        };
        var client = new HttpClient(tokenHandler) { BaseAddress = new Uri("http://localhost") };
        return (tokenHandler, client);
    }

    [Fact]
    public async Task SendAsync_WithToken_SetsAuthorizationHeader()
    {
        _jsRuntime.Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync("my-token-123");
        var (_, client) = CreateHandler();

        await client.GetAsync("/test");

        TestInnerHandler.LastRequest!.Headers.Authorization.Should().NotBeNull();
        TestInnerHandler.LastRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        TestInnerHandler.LastRequest.Headers.Authorization.Parameter.Should().Be("my-token-123");
    }

    [Fact]
    public async Task SendAsync_WithoutToken_DoesNotSetAuthorizationHeader()
    {
        _jsRuntime.Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);
        var (_, client) = CreateHandler();

        await client.GetAsync("/test");

        TestInnerHandler.LastRequest!.Headers.Authorization.Should().BeNull();
    }

    [Fact]
    public async Task SendAsync_WithEmptyToken_DoesNotSetAuthorizationHeader()
    {
        _jsRuntime.Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync("");
        var (_, client) = CreateHandler();

        await client.GetAsync("/test");

        TestInnerHandler.LastRequest!.Headers.Authorization.Should().BeNull();
    }

    [Fact]
    public async Task SendAsync_WhenJsRuntimeThrows_ProceedsWithoutAuth()
    {
        _jsRuntime.Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ThrowsAsync(new JSException("JS error"));
        var (_, client) = CreateHandler();

        var response = await client.GetAsync("/test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        TestInnerHandler.LastRequest!.Headers.Authorization.Should().BeNull();
    }

    [Fact]
    public async Task SendAsync_RequestsAccessTokenFromLocalStorage()
    {
        _jsRuntime.Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);
        var (_, client) = CreateHandler();

        await client.GetAsync("/test");

        _jsRuntime.Verify(js => js.InvokeAsync<string?>(
            "localStorage.getItem",
            It.Is<object[]>(args => (string)args[0] == "accessToken")), Times.Once);
    }

    private class TestInnerHandler : HttpMessageHandler
    {
        [ThreadStatic]
        public static HttpRequestMessage? LastRequest;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
