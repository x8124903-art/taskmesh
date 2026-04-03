using System.Text;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using MudBlazor.Services;
using WebApp.Services;

namespace WebApp.Tests.Pages;

public sealed class LoginPageShould : TestContext
{
    private AuthService SetupServices(bool isAuthenticated = false)
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = Helpers.MockHttpMessageHandler.WithStatusCode(System.Net.HttpStatusCode.OK);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };

        var jsRuntime = Services.BuildServiceProvider().GetRequiredService<IJSRuntime>();
        var authService = new AuthService(httpClient, jsRuntime, NullLogger<AuthService>.Instance);

        if (isAuthenticated)
        {
            typeof(AuthService).GetProperty("CurrentUser")!
                .SetValue(authService, new WebApp.Models.User { Id = 1, Email = "test@test.com", Name = "Test" });
        }

        Services.AddSingleton(authService);
        Services.AddSingleton(new ProjectApiService(httpClient));
        Services.AddSingleton(new ProjectInvitationApiService(httpClient, NullLogger<ProjectInvitationApiService>.Instance));
        Services.AddSingleton(new ProjectStateService());
        return authService;
    }

    [Fact]
    public void RedirectToProjects_WhenAlreadyAuthenticated()
    {
        SetupServices(isAuthenticated: true);
        var nav = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();

        RenderComponent<WebApp.Pages.Login>();

        nav.Uri.Should().Contain("/projects");
    }

    [Fact]
    public void RenderLoginForm_WhenNotAuthenticated()
    {
        SetupServices(isAuthenticated: false);

        var cut = RenderComponent<WebApp.Pages.Login>();

        cut.Markup.Should().Contain("Iniciar sesión");
    }

    [Fact]
    public void HandleLogin_Success_NavigatesToHome()
    {
        var loginResponse = new
        {
            User = new { Id = 1, Email = "test@test.com", Name = "Test" },
            AccessToken = "fake-token",
            RefreshToken = "fake-refresh"
        };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            var json = System.Text.Json.JsonSerializer.Serialize(loginResponse);
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var jsRuntime = Services.BuildServiceProvider().GetRequiredService<IJSRuntime>();
        var authService = new AuthService(httpClient, jsRuntime, NullLogger<AuthService>.Instance);

        Services.AddSingleton(authService);
        Services.AddSingleton(new ProjectApiService(httpClient));
        Services.AddSingleton(new ProjectInvitationApiService(httpClient, NullLogger<ProjectInvitationApiService>.Instance));
        Services.AddSingleton(new ProjectStateService());

        var nav = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();
        var cut = RenderComponent<WebApp.Pages.Login>();

        cut.Find("form").Submit();
        cut.WaitForState(() => handler.Requests.Count > 0, TimeSpan.FromSeconds(3));

        handler.Requests.Should().ContainSingle();
        cut.Markup.Should().NotContain("Login incorrecto");
    }

    [Fact]
    public void HandleLogin_Failure_ShowsError()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = Helpers.MockHttpMessageHandler.WithStatusCode(System.Net.HttpStatusCode.Unauthorized);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var jsRuntime = Services.BuildServiceProvider().GetRequiredService<IJSRuntime>();
        var authService = new AuthService(httpClient, jsRuntime, NullLogger<AuthService>.Instance);

        Services.AddSingleton(authService);
        Services.AddSingleton(new ProjectApiService(httpClient));
        Services.AddSingleton(new ProjectInvitationApiService(httpClient, NullLogger<ProjectInvitationApiService>.Instance));
        Services.AddSingleton(new ProjectStateService());

        var cut = RenderComponent<WebApp.Pages.Login>();

        cut.Find("form").Submit();
        cut.WaitForState(() => cut.Markup.Contains("Login incorrecto"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("Login incorrecto");
    }
}
