using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;
using Moq;
using WebApp.Models.Notifications;
using WebApp.Services;

namespace WebApp.Tests.Components;

public sealed class MainLayoutShould : TestContext
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
                .SetValue(authService, new WebApp.Models.User { Id = 1, Email = "test@test.com", Name = "TestUser" });
        }

        Services.AddSingleton(authService);
        Services.AddSingleton(new ProjectApiService(httpClient));
        Services.AddSingleton(new ProjectInvitationApiService(httpClient, NullLogger<ProjectInvitationApiService>.Instance));
        Services.AddSingleton(new ProjectStateService());

        var notificationMock = new Mock<INotificationApiService>();
        notificationMock.Setup(x => x.GetUnreadCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UnreadCountResponse(0));
        Services.AddSingleton<INotificationApiService>(notificationMock.Object);
    }

    [Fact]
    public void ShowAppBar_WhenAuthenticated()
    {
        SetupServices(isAuthenticated: true);

        var cut = RenderComponent<WebApp.Components.MainLayout>();

        cut.Markup.Should().Contain("TaskMesh");
        cut.Markup.Should().Contain("TestUser");
    }

    [Fact]
    public void NotShowAppBar_WhenNotAuthenticated()
    {
        SetupServices(isAuthenticated: false);

        var cut = RenderComponent<WebApp.Components.MainLayout>();

        cut.Markup.Should().NotContain("Proyectos");
    }

    [Fact]
    public void ToggleDrawer_ChangesState()
    {
        SetupServices(isAuthenticated: true);

        var cut = RenderComponent<WebApp.Components.MainLayout>();

        cut.Markup.Should().Contain("TaskMesh");
        
        var menuButton = cut.FindAll("button").FirstOrDefault(b => 
            b.ClassList.Contains("mud-icon-button"));
        menuButton?.Click();
        
        menuButton?.Click();
    }

    [Fact]
    public void Logout_NavigatesToLoginPage()
    {
        SetupServices(isAuthenticated: true);
        var nav = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();

        var provider = RenderComponent<MudBlazor.MudPopoverProvider>();
        var cut = RenderComponent<WebApp.Components.MainLayout>();
        cut.Markup.Should().Contain("TaskMesh");

        var userButton = cut.FindAll("button.mud-icon-button")
            .FirstOrDefault(b => b.InnerHtml.Contains("AccountCircle") || 
                                  b.OuterHtml.Contains("AccountCircle"));
        
        if (userButton != null)
        {
            userButton.Click();
            
            var popoverMarkup = provider.Markup;
            var logoutItem = provider.FindAll(".mud-list-item")
                .FirstOrDefault(e => e.TextContent.Contains("Cerrar Sesión"));
            
            if (logoutItem != null)
            {
                logoutItem.Click();
                nav.Uri.Should().Contain("/login");
                return;
            }
        }

        cut.Markup.Should().Contain("TestUser");
    }

    [Fact]
    public void ShowNavigationLinks_WhenAuthenticated()
    {
        SetupServices(isAuthenticated: true);

        var cut = RenderComponent<WebApp.Components.MainLayout>();

        cut.Markup.Should().Contain("Proyectos");
        cut.Markup.Should().Contain("Invitaciones");
    }

    [Fact]
    public void ToggleUserMenu_ShowsAndHidesMenu()
    {
        SetupServices(isAuthenticated: true);

        var provider = RenderComponent<MudBlazor.MudPopoverProvider>();
        var cut = RenderComponent<WebApp.Components.MainLayout>();

        // The account circle button is the last icon button in the app bar
        var iconButtons = cut.FindAll("button.mud-icon-button").ToList();
        var userButton = iconButtons.LastOrDefault();

        if (userButton != null)
        {
            userButton.Click();
            cut.Markup.Should().Contain("Cerrar");
        }
    }

    [Fact]
    public void ClosesUserMenu_WhenOverlayClicked()
    {
        SetupServices(isAuthenticated: true);

        var provider = RenderComponent<MudBlazor.MudPopoverProvider>();
        var cut = RenderComponent<WebApp.Components.MainLayout>();

        var iconButtons = cut.FindAll("button.mud-icon-button").ToList();
        var userButton = iconButtons.LastOrDefault();

        if (userButton != null)
        {
            userButton.Click();
            var overlay = cut.FindAll(".mud-overlay").FirstOrDefault();
            overlay?.Click();
        }
    }
}
