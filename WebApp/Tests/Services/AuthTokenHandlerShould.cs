using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using WebApp.Services;

namespace WebApp.Tests.Services;

public sealed class AuthTokenHandlerShould
{
    private readonly Mock<IJSRuntime> _jsRuntime = new();
    private readonly Mock<IAuthService> _authService = new();
    private readonly Mock<ILogger<AuthTokenHandler>> _logger = new();

    private (AuthTokenHandler handler, HttpClient client) CreateHandler()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_authService.Object);
        var serviceProvider = services.BuildServiceProvider();

        var tokenHandler = new AuthTokenHandler(_jsRuntime.Object, serviceProvider, _logger.Object)
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

    [Fact]
    public async Task SendAsync_On401_AttemptsTokenRefresh()
    {
        _jsRuntime.Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync("my-token");
        _authService.Setup(a => a.RefreshTokenAsync()).ReturnsAsync(true);

        var services = new ServiceCollection();
        services.AddSingleton(_authService.Object);
        var serviceProvider = services.BuildServiceProvider();

        var callCount = 0;
        var tokenHandler = new AuthTokenHandler(_jsRuntime.Object, serviceProvider, _logger.Object)
        {
            InnerHandler = new CallCountHandler(() =>
            {
                callCount++;
                return callCount == 1
                    ? new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    : new HttpResponseMessage(HttpStatusCode.OK);
            })
        };
        var client = new HttpClient(tokenHandler) { BaseAddress = new Uri("http://localhost") };

        var response = await client.GetAsync("/test");

        _authService.Verify(a => a.RefreshTokenAsync(), Times.Once);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendAsync_On401_RefreshFails_ReturnsUnauthorized()
    {
        _jsRuntime.Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync("my-token");
        _authService.Setup(a => a.RefreshTokenAsync()).ReturnsAsync(false);

        var services = new ServiceCollection();
        services.AddSingleton(_authService.Object);
        var serviceProvider = services.BuildServiceProvider();

        var tokenHandler = new AuthTokenHandler(_jsRuntime.Object, serviceProvider, _logger.Object)
        {
            InnerHandler = new TestInnerHandler(HttpStatusCode.Unauthorized)
        };
        var client = new HttpClient(tokenHandler) { BaseAddress = new Uri("http://localhost") };

        var response = await client.GetAsync("/test");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendAsync_On401_RefreshEndpoint_DoesNotRetry()
    {
        _jsRuntime.Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync("my-token");

        var services = new ServiceCollection();
        services.AddSingleton(_authService.Object);
        var serviceProvider = services.BuildServiceProvider();

        var tokenHandler = new AuthTokenHandler(_jsRuntime.Object, serviceProvider, _logger.Object)
        {
            InnerHandler = new TestInnerHandler(HttpStatusCode.Unauthorized)
        };
        var client = new HttpClient(tokenHandler) { BaseAddress = new Uri("http://localhost") };

        var response = await client.GetAsync("/api/v1/auth/refresh");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        _authService.Verify(a => a.RefreshTokenAsync(), Times.Never);
    }

    [Fact]
    public async Task SendAsync_On401_WithPostContent_ClonesRequestCorrectly()
    {
        _jsRuntime.Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync("my-token");
        _authService.Setup(a => a.RefreshTokenAsync()).ReturnsAsync(true);

        var services = new ServiceCollection();
        services.AddSingleton(_authService.Object);
        var serviceProvider = services.BuildServiceProvider();

        var callCount = 0;
        var tokenHandler = new AuthTokenHandler(_jsRuntime.Object, serviceProvider, _logger.Object)
        {
            InnerHandler = new CallCountHandler(() =>
            {
                callCount++;
                return callCount == 1
                    ? new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    : new HttpResponseMessage(HttpStatusCode.OK);
            })
        };
        var client = new HttpClient(tokenHandler) { BaseAddress = new Uri("http://localhost") };

        var response = await client.PostAsync("/test", new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        callCount.Should().Be(2);
    }

    private class TestInnerHandler : HttpMessageHandler
    {
        [ThreadStatic]
        public static HttpRequestMessage? LastRequest;
        private readonly HttpStatusCode _statusCode;

        public TestInnerHandler(HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(_statusCode));
        }
    }

    private class CallCountHandler : HttpMessageHandler
    {
        private readonly Func<HttpResponseMessage> _responseFactory;

        public CallCountHandler(Func<HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responseFactory());
        }
    }
}
