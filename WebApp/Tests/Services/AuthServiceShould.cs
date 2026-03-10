using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using WebApp.Models;
using WebApp.Models.Auth;
using WebApp.Services;
using WebApp.Tests.Helpers;

namespace WebApp.Tests.Services;

public sealed class AuthServiceShould
{
    private readonly Mock<IJSRuntime> _jsRuntime = new();
    private readonly Mock<ILogger<AuthService>> _logger = new();

    private AuthService CreateService(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        return new AuthService(httpClient, _jsRuntime.Object, _logger.Object);
    }

    private static string CreateTestJwt(string sub = "1", string email = "test@test.com", string name = "Test User")
    {
        var header = Convert.ToBase64String("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"u8).TrimEnd('=');
        var payloadJson = JsonSerializer.Serialize(new { sub, email, name });
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payloadJson)).TrimEnd('=');
        var signature = Convert.ToBase64String(new byte[32]).TrimEnd('=');
        return $"{header}.{payload}.{signature}";
    }

    [Fact]
    public async Task InitializeAsync_WithValidToken_SetsCurrentUser()
    {
        var jwt = CreateTestJwt("42", "user@example.com", "John");
        _jsRuntime.Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(jwt);

        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK));
        var result = await service.InitializeAsync();

        result.Should().BeTrue();
        service.CurrentUser.Should().NotBeNull();
        service.CurrentUser!.Id.Should().Be(42);
        service.CurrentUser.Email.Should().Be("user@example.com");
        service.CurrentUser.Name.Should().Be("John");
    }

    [Fact]
    public async Task InitializeAsync_WithNoToken_ReturnsFalse()
    {
        _jsRuntime.Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK));
        var result = await service.InitializeAsync();

        result.Should().BeFalse();
        service.CurrentUser.Should().BeNull();
    }

    [Fact]
    public async Task InitializeAsync_WithInvalidToken_ReturnsFalse()
    {
        _jsRuntime.Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync("invalid-jwt-token");

        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK));
        var result = await service.InitializeAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task InitializeAsync_WhenJsRuntimeThrows_ReturnsFalse()
    {
        _jsRuntime.Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ThrowsAsync(new JSException("test error"));

        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK));
        var result = await service.InitializeAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_WithSuccessResponse_ReturnsTrueAndSetsUser()
    {
        var loginResponse = new LoginResponse
        {
            AccessToken = "token123",
            RefreshToken = "refresh123",
            User = new User { Id = 1, Email = "test@test.com", Name = "Test" }
        };
        var handler = MockHttpMessageHandler.WithJsonResponse(loginResponse);
        var service = CreateService(handler);

        var result = await service.LoginAsync("test@test.com", "password");

        result.Should().BeTrue();
        service.CurrentUser.Should().NotBeNull();
        service.CurrentUser!.Email.Should().Be("test@test.com");
        service.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public async Task LoginAsync_StoresTokensInLocalStorage()
    {
        var loginResponse = new LoginResponse
        {
            AccessToken = "access-tok",
            RefreshToken = "refresh-tok",
            User = new User { Id = 1, Email = "a@a.com", Name = "A" }
        };
        var handler = MockHttpMessageHandler.WithJsonResponse(loginResponse);
        var service = CreateService(handler);

        await service.LoginAsync("a@a.com", "pass");

        _jsRuntime.Verify(js => js.InvokeAsync<object>(
            "localStorage.setItem",
            It.Is<object[]>(args => (string)args[0] == "accessToken" && (string)args[1] == "access-tok")),
            Times.Once);
        _jsRuntime.Verify(js => js.InvokeAsync<object>(
            "localStorage.setItem",
            It.Is<object[]>(args => (string)args[0] == "refreshToken" && (string)args[1] == "refresh-tok")),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithFailResponse_ReturnsFalse()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.Unauthorized);
        var service = CreateService(handler);

        var result = await service.LoginAsync("test@test.com", "wrong");

        result.Should().BeFalse();
        service.CurrentUser.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_WithSuccessResponse_ReturnsTrueAndSetsUser()
    {
        var loginResponse = new LoginResponse
        {
            AccessToken = "token123",
            RefreshToken = "refresh123",
            User = new User { Id = 2, Email = "new@test.com", Name = "New User" }
        };
        var handler = MockHttpMessageHandler.WithJsonResponse(loginResponse);
        var service = CreateService(handler);

        var result = await service.RegisterAsync("new@test.com", "New User", "password");

        result.Should().BeTrue();
        service.CurrentUser.Should().NotBeNull();
        service.CurrentUser!.Name.Should().Be("New User");
    }

    [Fact]
    public async Task RegisterAsync_WithFailResponse_ReturnsFalse()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.Conflict);
        var service = CreateService(handler);

        var result = await service.RegisterAsync("exists@test.com", "User", "password");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task LogoutAsync_ClearsCurrentUserAndTokens()
    {
        var loginResponse = new LoginResponse
        {
            AccessToken = "t", RefreshToken = "r",
            User = new User { Id = 1, Email = "a@a.com", Name = "A" }
        };
        var handler = MockHttpMessageHandler.WithJsonResponse(loginResponse);
        var service = CreateService(handler);
        await service.LoginAsync("a@a.com", "p");

        await service.LogoutAsync();

        service.CurrentUser.Should().BeNull();
        service.IsAuthenticated.Should().BeFalse();
        _jsRuntime.Verify(js => js.InvokeAsync<object>(
            "localStorage.removeItem",
            It.Is<object[]>(args => (string)args[0] == "accessToken")), Times.Once);
        _jsRuntime.Verify(js => js.InvokeAsync<object>(
            "localStorage.removeItem",
            It.Is<object[]>(args => (string)args[0] == "refreshToken")), Times.Once);
    }

    [Fact]
    public void IsAuthenticated_WhenNoUser_ReturnsFalse()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK));
        service.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_SendsPostToCorrectEndpoint()
    {
        var loginResponse = new LoginResponse
        {
            AccessToken = "t", RefreshToken = "r",
            User = new User { Id = 1, Email = "a@a.com", Name = "A" }
        };
        var handler = MockHttpMessageHandler.WithJsonResponse(loginResponse);
        var service = CreateService(handler);

        await service.LoginAsync("a@a.com", "pass");

        handler.Requests.Should().ContainSingle();
        handler.Requests[0].Method.Should().Be(HttpMethod.Post);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/auth/login");
    }

    [Fact]
    public async Task RegisterAsync_SendsPostToCorrectEndpoint()
    {
        var loginResponse = new LoginResponse
        {
            AccessToken = "t", RefreshToken = "r",
            User = new User { Id = 1, Email = "a@a.com", Name = "A" }
        };
        var handler = MockHttpMessageHandler.WithJsonResponse(loginResponse);
        var service = CreateService(handler);

        await service.RegisterAsync("a@a.com", "A", "pass");

        handler.Requests.Should().ContainSingle();
        handler.Requests[0].Method.Should().Be(HttpMethod.Post);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/auth/register");
    }

    [Fact]
    public async Task LoginAsync_WithNullResponseBody_ReturnsFalse()
    {
        var handler = new MockHttpMessageHandler((req, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
            }));
        var service = CreateService(handler);

        var result = await service.LoginAsync("test@test.com", "password");

        result.Should().BeFalse();
        service.CurrentUser.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WhenHttpClientThrows_ReturnsFalse()
    {
        var handler = new MockHttpMessageHandler((req, _) =>
        {
            throw new HttpRequestException("Network unavailable");
        });
        var service = CreateService(handler);

        var result = await service.LoginAsync("test@test.com", "password");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterAsync_WithNullResponseBody_ReturnsFalse()
    {
        var handler = new MockHttpMessageHandler((req, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
            }));
        var service = CreateService(handler);

        var result = await service.RegisterAsync("new@test.com", "New", "password");

        result.Should().BeFalse();
        service.CurrentUser.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_WhenHttpClientThrows_ReturnsFalse()
    {
        var handler = new MockHttpMessageHandler((req, _) =>
        {
            throw new HttpRequestException("Network unavailable");
        });
        var service = CreateService(handler);

        var result = await service.RegisterAsync("new@test.com", "New", "password");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task InitializeAsync_WithTokenNeedingPadding_ParsesCorrectly()
    {
        var header = Convert.ToBase64String("{\"alg\":\"HS256\"}"u8).TrimEnd('=');
        var payloadJson = "{\"sub\":\"7\",\"email\":\"pad@test.com\",\"name\":\"Padded\"}";
        var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payloadJson);
        var payload = Convert.ToBase64String(payloadBytes).TrimEnd('=');
        var signature = Convert.ToBase64String(new byte[16]).TrimEnd('=');
        var jwt = $"{header}.{payload}.{signature}";

        _jsRuntime.Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(jwt);

        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK));
        var result = await service.InitializeAsync();

        result.Should().BeTrue();
        service.CurrentUser!.Email.Should().Be("pad@test.com");
    }

    [Fact]
    public async Task InitializeAsync_WithMalformedBase64_ReturnsFalse()
    {
        _jsRuntime.Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync("aaa.!!!invalid-base64!!!.ccc");

        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK));
        var result = await service.InitializeAsync();

        result.Should().BeFalse();
    }
}
