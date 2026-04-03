using System.Text;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using MudBlazor.Services;
using WebApp.Services;
using WebApp.Models.Projects;

namespace WebApp.Tests.Pages;

public sealed class ProjectsPageShould : TestContext
{
    private void SetupServices(bool isAuthenticated = true, List<Project>? projects = null)
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        projects ??= new List<Project>();
        var json = System.Text.Json.JsonSerializer.Serialize(projects);

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        });

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

        RenderComponent<WebApp.Pages.Projects>();

        nav.Uri.Should().Contain("/login");
    }

    [Fact]
    public void RenderProjectsPage_WhenAuthenticated()
    {
        SetupServices(isAuthenticated: true);

        var cut = RenderComponent<WebApp.Pages.Projects>();

        cut.Markup.Should().Contain("Mis Proyectos");
    }

    [Fact]
    public void ShowEmptyMessage_WhenNoProjects()
    {
        SetupServices(isAuthenticated: true, projects: new List<Project>());

        var cut = RenderComponent<WebApp.Pages.Projects>();
        cut.WaitForState(() => cut.Markup.Contains("No tienes proyectos"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("No tienes proyectos");
    }

    [Fact]
    public void ShowProjectCards_WhenProjectsExist()
    {
        var projects = new List<Project>
        {
            new() { IdProject = 1, Name = "TestProject", Description = "Desc", StatusName = "Active", CreatedByName = "User1", CreatedAt = DateTime.UtcNow },
            new() { IdProject = 2, Name = "Another", Description = "Desc2", StatusName = "Archived", CreatedByName = "User2", CreatedAt = DateTime.UtcNow },
            new() { IdProject = 3, Name = "Paused One", Description = "", StatusName = "Paused", CreatedByName = "User3", CreatedAt = DateTime.UtcNow },
            new() { IdProject = 4, Name = "Unknown Status", Description = "X", StatusName = "Other", CreatedByName = "User4", CreatedAt = DateTime.UtcNow }
        };
        SetupServices(isAuthenticated: true, projects: projects);

        var cut = RenderComponent<WebApp.Pages.Projects>();
        cut.WaitForState(() => cut.Markup.Contains("TestProject"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("TestProject");
        cut.Markup.Should().Contain("Another");
        cut.Markup.Should().Contain("Paused One");
        cut.Markup.Should().Contain("Sin descripción");
    }

    [Fact]
    public void NavigateToProject_WhenCardClicked()
    {
        var projects = new List<Project>
        {
            new() { IdProject = 42, Name = "Navigate Me", Description = "Go", StatusName = "Active", CreatedByName = "User", CreatedAt = DateTime.UtcNow }
        };
        SetupServices(isAuthenticated: true, projects: projects);
        var nav = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();

        var cut = RenderComponent<WebApp.Pages.Projects>();
        cut.WaitForState(() => cut.Markup.Contains("Navigate Me"), TimeSpan.FromSeconds(3));

        var card = cut.Find("div.mud-card");
        card.Click();

        nav.Uri.Should().Contain("/projects/42");
    }

    [Fact]
    public void OpenCreateDialog_NavigatesToCreate()
    {
        SetupServices(isAuthenticated: true, projects: new List<Project>());
        var nav = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();

        var cut = RenderComponent<WebApp.Pages.Projects>();
        cut.WaitForState(() => cut.Markup.Contains("Nuevo Proyecto"), TimeSpan.FromSeconds(3));

        var createBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Nuevo Proyecto"));
        createBtn.Click();

        nav.Uri.Should().Contain("/projects/create");
    }

    [Fact]
    public void Dispose_UnsubscribesFromProjectChanges()
    {
        SetupServices(isAuthenticated: true, projects: new List<Project>());

        var cut = RenderComponent<WebApp.Pages.Projects>();
        cut.WaitForState(() => cut.Markup.Contains("No tienes proyectos"), TimeSpan.FromSeconds(3));

        cut.Dispose();
    }

    [Fact]
    public void HandleProjectsChanged_ReloadsProjects()
    {
        var projects = new List<Project>
        {
            new() { IdProject = 1, Name = "Initial", Description = "D", StatusName = "Active", CreatedByName = "User", CreatedAt = DateTime.UtcNow }
        };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var json = System.Text.Json.JsonSerializer.Serialize(projects);
        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
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
        var stateService = new ProjectStateService();
        Services.AddSingleton(stateService);

        var cut = RenderComponent<WebApp.Pages.Projects>();
        cut.WaitForState(() => cut.Markup.Contains("Initial"), TimeSpan.FromSeconds(3));

        stateService.NotifyProjectsChanged();

        cut.WaitForState(() => cut.Markup.Contains("Initial"), TimeSpan.FromSeconds(3));
        cut.Markup.Should().Contain("Initial");
    }
}
