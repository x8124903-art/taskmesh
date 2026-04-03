using System.Text;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using MudBlazor.Services;
using WebApp.Services;

namespace WebApp.Tests.Pages;

public sealed class CreateProjectPageShould : TestContext
{
    private void SetupServices(bool isAuthenticated = true)
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
    public void RedirectToLogin_WhenNotAuthenticated()
    {
        SetupServices(isAuthenticated: false);
        var nav = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();

        RenderComponent<WebApp.Pages.CreateProject>();

        nav.Uri.Should().Contain("/login");
    }

    [Fact]
    public void RenderCreateForm_WhenAuthenticated()
    {
        SetupServices(isAuthenticated: true);

        var cut = RenderComponent<WebApp.Pages.CreateProject>();

        cut.Markup.Should().Contain("Crear Nuevo Proyecto");
    }

    [Fact]
    public void HandleCreate_Success_NavigatesToProject()
    {
        var project = new { IdProject = 5, Name = "New", Description = "Desc", StatusName = "Active", CreatedByName = "User", CreatedAt = DateTime.UtcNow };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            var json = System.Text.Json.JsonSerializer.Serialize(project);
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var jsRuntime = Services.BuildServiceProvider().GetRequiredService<IJSRuntime>();
        var authService = new AuthService(httpClient, jsRuntime, NullLogger<AuthService>.Instance);
        typeof(AuthService).GetProperty("CurrentUser")!
            .SetValue(authService, new WebApp.Models.User { Id = 1, Email = "test@test.com", Name = "Test" });

        Services.AddSingleton(authService);
        Services.AddSingleton(new ProjectApiService(httpClient));
        Services.AddSingleton(new ProjectInvitationApiService(httpClient, NullLogger<ProjectInvitationApiService>.Instance));
        Services.AddSingleton(new ProjectStateService());

        var nav = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();
        var cut = RenderComponent<WebApp.Pages.CreateProject>();

        cut.Find("form").Submit();
        cut.WaitForState(() => nav.Uri.Contains("/projects/"), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void HandleCreate_NullResponse_ShowsError()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var jsRuntime = Services.BuildServiceProvider().GetRequiredService<IJSRuntime>();
        var authService = new AuthService(httpClient, jsRuntime, NullLogger<AuthService>.Instance);
        typeof(AuthService).GetProperty("CurrentUser")!
            .SetValue(authService, new WebApp.Models.User { Id = 1, Email = "test@test.com", Name = "Test" });

        Services.AddSingleton(authService);
        Services.AddSingleton(new ProjectApiService(httpClient));
        Services.AddSingleton(new ProjectInvitationApiService(httpClient, NullLogger<ProjectInvitationApiService>.Instance));
        Services.AddSingleton(new ProjectStateService());

        var cut = RenderComponent<WebApp.Pages.CreateProject>();

        cut.Find("form").Submit();
        cut.WaitForState(() => cut.Markup.Contains("Error al crear"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("Error al crear");
    }

    [Fact]
    public void Cancel_NavigatesToProjects()
    {
        SetupServices(isAuthenticated: true);
        var nav = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();
        var cut = RenderComponent<WebApp.Pages.CreateProject>();

        var buttons = cut.FindAll("button");
        var cancelButton = buttons.First(b => b.TextContent.Contains("Cancelar"));
        cancelButton.Click();

        nav.Uri.Should().Contain("/projects");
    }
}
