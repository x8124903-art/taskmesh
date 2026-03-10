using System.Text;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using MudBlazor.Services;
using WebApp.Services;

namespace WebApp.Tests.Pages;

public sealed class RegisterPageShould : TestContext
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
            typeof(AuthService).GetProperty("CurrentUser")!
                .SetValue(authService, new WebApp.Models.User { Id = 1, Email = "test@test.com", Name = "Test" });
        }

        Services.AddSingleton(authService);
        Services.AddSingleton(new ProjectApiService(httpClient));
        Services.AddSingleton(new ProjectInvitationApiService(httpClient, NullLogger<ProjectInvitationApiService>.Instance));
        Services.AddSingleton(new ProjectStateService());
    }

    [Fact]
    public void RedirectToProjects_WhenAlreadyAuthenticated()
    {
        SetupServices(isAuthenticated: true);
        var nav = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();

        RenderComponent<WebApp.Pages.Register>();

        nav.Uri.Should().Contain("/projects");
    }

    [Fact]
    public void RenderRegisterForm_WhenNotAuthenticated()
    {
        SetupServices(isAuthenticated: false);

        var cut = RenderComponent<WebApp.Pages.Register>();

        cut.Markup.Should().Contain("Registro");
    }

    [Fact]
    public void HandleRegister_PasswordMismatch_ShowsError()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = Helpers.MockHttpMessageHandler.WithStatusCode(System.Net.HttpStatusCode.OK);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var jsRuntime = Services.BuildServiceProvider().GetRequiredService<IJSRuntime>();
        var authService = new AuthService(httpClient, jsRuntime, NullLogger<AuthService>.Instance);

        Services.AddSingleton(authService);
        Services.AddSingleton(new ProjectApiService(httpClient));
        Services.AddSingleton(new ProjectInvitationApiService(httpClient, NullLogger<ProjectInvitationApiService>.Instance));
        Services.AddSingleton(new ProjectStateService());

        var cut = RenderComponent<WebApp.Pages.Register>();

        var inputs = cut.FindAll("input");
        inputs[2].Change("password1");
        inputs[3].Change("password2");

        cut.Find("form").Submit();
        cut.WaitForState(() => cut.Markup.Contains("Las contrase\u00f1as no coinciden"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("Las contrase\u00f1as no coinciden");
    }

    [Fact]
    public void HandleRegister_Success_NavigatesToProjects()
    {
        var response = new
        {
            User = new { Id = 1, Email = "new@test.com", Name = "New" },
            AccessToken = "fake-token",
            RefreshToken = "fake-refresh"
        };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            var json = System.Text.Json.JsonSerializer.Serialize(response);
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
        var cut = RenderComponent<WebApp.Pages.Register>();

        cut.Find("form").Submit();
        cut.WaitForState(() => nav.Uri.Contains("/projects"), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void HandleRegister_Failure_ShowsError()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = Helpers.MockHttpMessageHandler.WithStatusCode(System.Net.HttpStatusCode.Conflict);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var jsRuntime = Services.BuildServiceProvider().GetRequiredService<IJSRuntime>();
        var authService = new AuthService(httpClient, jsRuntime, NullLogger<AuthService>.Instance);

        Services.AddSingleton(authService);
        Services.AddSingleton(new ProjectApiService(httpClient));
        Services.AddSingleton(new ProjectInvitationApiService(httpClient, NullLogger<ProjectInvitationApiService>.Instance));
        Services.AddSingleton(new ProjectStateService());

        var cut = RenderComponent<WebApp.Pages.Register>();

        cut.Find("form").Submit();
        cut.WaitForState(() => cut.Markup.Contains("Error al registrar"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("Error al registrar");
    }
}
