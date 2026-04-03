using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using MudBlazor.Services;
using WebApp.Services;

namespace WebApp.Tests.Pages;

public sealed class IndexPageShould : TestContext
{
    private void SetupServices(bool isAuthenticated = false)
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = Helpers.MockHttpMessageHandler.WithStatusCode(System.Net.HttpStatusCode.OK);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };

        var jsRuntime = Services.BuildServiceProvider().GetRequiredService<IJSRuntime>();
        var authService = new AuthService(httpClient, jsRuntime, NullLogger<AuthService>.Instance);

        if (isAuthenticated)
        {
            var prop = typeof(AuthService).GetProperty("CurrentUser");
            prop!.SetValue(authService, new WebApp.Models.User { Id = 1, Email = "test@test.com", Name = "Test" });
        }

        Services.AddSingleton(authService);
        Services.AddSingleton(new ProjectApiService(httpClient));
        Services.AddSingleton(new ProjectInvitationApiService(httpClient, NullLogger<ProjectInvitationApiService>.Instance));
        Services.AddSingleton(new ProjectStateService());
    }

    [Fact]
    public void RedirectToLogin_WhenNotAuthenticated()
    {
        SetupServices(isAuthenticated: false);
        var nav = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();

        RenderComponent<WebApp.Pages.Index>();

        nav.Uri.Should().Contain("/login");
    }

    [Fact]
    public void RedirectToProjects_WhenAuthenticated()
    {
        SetupServices(isAuthenticated: true);
        var nav = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();

        RenderComponent<WebApp.Pages.Index>();

        nav.Uri.Should().Contain("/projects");
    }
}
