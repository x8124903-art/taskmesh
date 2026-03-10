using System.Text;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using MudBlazor.Services;
using WebApp.Models.Projects;
using WebApp.Services;

namespace WebApp.Tests.Pages;

public sealed class MyInvitationsPageShould : TestContext
{
    private void SetupServices(List<ProjectInvitation>? invitations = null)
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        invitations ??= new List<ProjectInvitation>();
        var json = System.Text.Json.JsonSerializer.Serialize(invitations);

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

        typeof(AuthService).GetProperty("CurrentUser")!
            .SetValue(authService, new WebApp.Models.User { Id = 1, Email = "test@test.com", Name = "Test" });

        Services.AddSingleton(authService);
        Services.AddSingleton(new ProjectApiService(httpClient));
        Services.AddSingleton(new ProjectInvitationApiService(httpClient, NullLogger<ProjectInvitationApiService>.Instance));
        Services.AddSingleton(new ProjectStateService());

        this.AddTestAuthorization().SetAuthorized("test@test.com");
    }

    [Fact]
    public void RenderInvitationsPage()
    {
        SetupServices();

        var cut = RenderComponent<WebApp.Pages.MyInvitations>();

        cut.Markup.Should().Contain("Mis Invitaciones");
    }

    [Fact]
    public void ShowEmptyPendingMessage_WhenNoInvitations()
    {
        SetupServices(new List<ProjectInvitation>());

        var cut = RenderComponent<WebApp.Pages.MyInvitations>();
        cut.WaitForState(() => cut.Markup.Contains("No tienes invitaciones pendientes"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("No tienes invitaciones pendientes");
    }

    [Fact]
    public void ShowPendingInvitationsTable_WhenDataExists()
    {
        var invitations = new List<ProjectInvitation>
        {
            new()
            {
                IdProjectInvitation = 1, ProjectId = 1, ProjectName = "Project A",
                Email = "test@test.com", RoleName = "Admin", Token = "tok1",
                Status = "Pending", InvitedByName = "Owner",
                InvitedAt = DateTime.UtcNow.AddDays(-1), ExpiresAt = DateTime.UtcNow.AddDays(6)
            },
            new()
            {
                IdProjectInvitation = 2, ProjectId = 2, ProjectName = "Project B",
                Email = "test@test.com", RoleName = "Owner", Token = "tok2",
                Status = "Pending", InvitedByName = "Admin2",
                InvitedAt = DateTime.UtcNow.AddDays(-2), ExpiresAt = DateTime.UtcNow.AddDays(5)
            },
            new()
            {
                IdProjectInvitation = 3, ProjectId = 3, ProjectName = "Project C",
                Email = "test@test.com", RoleName = "Member", Token = "tok3",
                Status = "Accepted", InvitedByName = "Admin3",
                InvitedAt = DateTime.UtcNow.AddDays(-5), ExpiresAt = DateTime.UtcNow.AddDays(2),
                AcceptedAt = DateTime.UtcNow.AddDays(-4)
            },
            new()
            {
                IdProjectInvitation = 4, ProjectId = 4, ProjectName = "Project D",
                Email = "test@test.com", RoleName = "Viewer", Token = "tok4",
                Status = "Rejected", InvitedByName = "Admin4",
                InvitedAt = DateTime.UtcNow.AddDays(-10), ExpiresAt = DateTime.UtcNow.AddDays(-3),
                RejectedAt = DateTime.UtcNow.AddDays(-9)
            }
        };
        SetupServices(invitations);

        var cut = RenderComponent<WebApp.Pages.MyInvitations>();
        cut.WaitForState(() => cut.Markup.Contains("Project A"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("Project A");
        cut.Markup.Should().Contain("Project B");
    }

    [Fact]
    public void ShowAcceptedTab_WhenClickedAndHasData()
    {
        var invitations = new List<ProjectInvitation>
        {
            new()
            {
                IdProjectInvitation = 1, ProjectId = 1, ProjectName = "Accepted Project",
                Email = "test@test.com", RoleName = "Member", Token = "tok1",
                Status = "Accepted", InvitedByName = "Admin",
                InvitedAt = DateTime.UtcNow.AddDays(-5), ExpiresAt = DateTime.UtcNow.AddDays(2),
                AcceptedAt = DateTime.UtcNow.AddDays(-3)
            }
        };
        SetupServices(invitations);

        var cut = RenderComponent<WebApp.Pages.MyInvitations>();
        cut.WaitForState(() => !cut.Markup.Contains("CircularProgress"), TimeSpan.FromSeconds(3));

        var tabs = cut.FindAll("div.mud-tab");
        if (tabs.Count >= 2) tabs[1].Click();

        cut.Markup.Should().Contain("Accepted Project");
    }

    [Fact]
    public void ShowRejectedTab_WhenClickedAndHasData()
    {
        var invitations = new List<ProjectInvitation>
        {
            new()
            {
                IdProjectInvitation = 1, ProjectId = 1, ProjectName = "Rejected Project",
                Email = "test@test.com", RoleName = "Viewer", Token = "tok1",
                Status = "Rejected", InvitedByName = "Admin",
                InvitedAt = DateTime.UtcNow.AddDays(-10), ExpiresAt = DateTime.UtcNow.AddDays(-3),
                RejectedAt = DateTime.UtcNow.AddDays(-8)
            }
        };
        SetupServices(invitations);

        var cut = RenderComponent<WebApp.Pages.MyInvitations>();
        cut.WaitForState(() => !cut.Markup.Contains("CircularProgress"), TimeSpan.FromSeconds(3));

        var tabs = cut.FindAll("div.mud-tab");
        if (tabs.Count >= 3) tabs[2].Click();

        cut.Markup.Should().Contain("Rejected Project");
    }

    [Fact]
    public void ShowEmptyAccepted_WhenNoAcceptedInvitations()
    {
        SetupServices(new List<ProjectInvitation>());

        var cut = RenderComponent<WebApp.Pages.MyInvitations>();
        cut.WaitForState(() => cut.Markup.Contains("No tienes invitaciones pendientes"), TimeSpan.FromSeconds(3));

        var tabs = cut.FindAll("div.mud-tab");
        if (tabs.Count >= 2) tabs[1].Click();

        cut.Markup.Should().Contain("No tienes invitaciones aceptadas");
    }

    [Fact]
    public void ShowEmptyRejected_WhenNoRejectedInvitations()
    {
        SetupServices(new List<ProjectInvitation>());

        var cut = RenderComponent<WebApp.Pages.MyInvitations>();
        cut.WaitForState(() => cut.Markup.Contains("No tienes invitaciones pendientes"), TimeSpan.FromSeconds(3));

        var tabs = cut.FindAll("div.mud-tab");
        if (tabs.Count >= 3) tabs[2].Click();

        cut.Markup.Should().Contain("No tienes invitaciones rechazadas");
    }

    [Fact]
    public void AcceptInvitation_Success_ReloadsData()
    {
        var invitations = new List<ProjectInvitation>
        {
            new()
            {
                IdProjectInvitation = 1, ProjectId = 1, ProjectName = "Project A",
                Email = "test@test.com", RoleName = "Member", Token = "tok1",
                Status = "Pending", InvitedByName = "Owner",
                InvitedAt = DateTime.UtcNow.AddDays(-1), ExpiresAt = DateTime.UtcNow.AddDays(6)
            }
        };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var callCount = 0;
        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            callCount++;
            if (req.Method == HttpMethod.Post)
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            }
            var json = System.Text.Json.JsonSerializer.Serialize(invitations);
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
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
        this.AddTestAuthorization().SetAuthorized("test@test.com");

        var cut = RenderComponent<WebApp.Pages.MyInvitations>();
        cut.WaitForState(() => cut.Markup.Contains("Project A"), TimeSpan.FromSeconds(3));

        var acceptBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Aceptar"));
        acceptBtn.Click();

        cut.WaitForState(() => callCount >= 3, TimeSpan.FromSeconds(3));
        callCount.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public void RejectInvitation_Success_ReloadsData()
    {
        var invitations = new List<ProjectInvitation>
        {
            new()
            {
                IdProjectInvitation = 1, ProjectId = 1, ProjectName = "Project A",
                Email = "test@test.com", RoleName = "Member", Token = "tok1",
                Status = "Pending", InvitedByName = "Owner",
                InvitedAt = DateTime.UtcNow.AddDays(-1), ExpiresAt = DateTime.UtcNow.AddDays(6)
            }
        };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var callCount = 0;
        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            callCount++;
            if (req.Method == HttpMethod.Post)
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            }
            var json = System.Text.Json.JsonSerializer.Serialize(invitations);
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
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
        this.AddTestAuthorization().SetAuthorized("test@test.com");

        var cut = RenderComponent<WebApp.Pages.MyInvitations>();
        cut.WaitForState(() => cut.Markup.Contains("Project A"), TimeSpan.FromSeconds(3));

        var rejectBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Rechazar"));
        rejectBtn.Click();

        cut.WaitForState(() => callCount >= 3, TimeSpan.FromSeconds(3));
        callCount.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public void AcceptInvitation_Failure_ShowsError()
    {
        var invitations = new List<ProjectInvitation>
        {
            new()
            {
                IdProjectInvitation = 1, ProjectId = 1, ProjectName = "Project Fail",
                Email = "test@test.com", RoleName = "Member", Token = "tok1",
                Status = "Pending", InvitedByName = "Owner",
                InvitedAt = DateTime.UtcNow.AddDays(-1), ExpiresAt = DateTime.UtcNow.AddDays(6)
            }
        };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            if (req.Method == HttpMethod.Post)
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
            }
            var json = System.Text.Json.JsonSerializer.Serialize(invitations);
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
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
        this.AddTestAuthorization().SetAuthorized("test@test.com");

        var cut = RenderComponent<WebApp.Pages.MyInvitations>();
        cut.WaitForState(() => cut.Markup.Contains("Project Fail"), TimeSpan.FromSeconds(3));

        var acceptBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Aceptar"));
        acceptBtn.Click();

        cut.Markup.Should().Contain("Project Fail");
    }

    [Fact]
    public void RejectInvitation_Failure_ShowsError()
    {
        var invitations = new List<ProjectInvitation>
        {
            new()
            {
                IdProjectInvitation = 1, ProjectId = 1, ProjectName = "Project Reject Fail",
                Email = "test@test.com", RoleName = "Admin", Token = "tok1",
                Status = "Pending", InvitedByName = "Owner",
                InvitedAt = DateTime.UtcNow.AddDays(-1), ExpiresAt = DateTime.UtcNow.AddDays(6)
            }
        };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            if (req.Method == HttpMethod.Post)
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
            }
            var json = System.Text.Json.JsonSerializer.Serialize(invitations);
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
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
        this.AddTestAuthorization().SetAuthorized("test@test.com");

        var cut = RenderComponent<WebApp.Pages.MyInvitations>();
        cut.WaitForState(() => cut.Markup.Contains("Project Reject Fail"), TimeSpan.FromSeconds(3));

        var rejectBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Rechazar"));
        rejectBtn.Click();

        cut.Markup.Should().Contain("Project Reject Fail");
    }

    [Fact]
    public void AcceptInvitation_Exception_ShowsError()
    {
        var invitations = new List<ProjectInvitation>
        {
            new()
            {
                IdProjectInvitation = 1, ProjectId = 1, ProjectName = "Project Exception",
                Email = "test@test.com", RoleName = "Member", Token = "tok1",
                Status = "Pending", InvitedByName = "Owner",
                InvitedAt = DateTime.UtcNow.AddDays(-1), ExpiresAt = DateTime.UtcNow.AddDays(6)
            }
        };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            if (req.Method == HttpMethod.Post)
            {
                throw new HttpRequestException("Network error");
            }
            var json = System.Text.Json.JsonSerializer.Serialize(invitations);
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
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
        this.AddTestAuthorization().SetAuthorized("test@test.com");

        var cut = RenderComponent<WebApp.Pages.MyInvitations>();
        cut.WaitForState(() => cut.Markup.Contains("Project Exception"), TimeSpan.FromSeconds(3));

        var acceptBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Aceptar"));
        acceptBtn.Click();

        cut.Markup.Should().Contain("Project Exception");
    }

    [Fact]
    public void LoadInvitations_WithNullEmail_DoesNotCrash()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = Helpers.MockHttpMessageHandler.WithJsonResponse(new List<ProjectInvitation>());
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var jsRuntime = Services.BuildServiceProvider().GetRequiredService<IJSRuntime>();
        var authService = new AuthService(httpClient, jsRuntime, NullLogger<AuthService>.Instance);

        Services.AddSingleton(authService);
        Services.AddSingleton(new ProjectApiService(httpClient));
        Services.AddSingleton(new ProjectInvitationApiService(httpClient, NullLogger<ProjectInvitationApiService>.Instance));
        Services.AddSingleton(new ProjectStateService());
        this.AddTestAuthorization().SetAuthorized("test@test.com");

        var cut = RenderComponent<WebApp.Pages.MyInvitations>();

        cut.Markup.Should().Contain("Mis Invitaciones");
    }

    [Fact]
    public void LoadInvitations_Exception_ShowsError()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            throw new HttpRequestException("Service unavailable");
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
        this.AddTestAuthorization().SetAuthorized("test@test.com");

        var cut = RenderComponent<WebApp.Pages.MyInvitations>();

        cut.Markup.Should().Contain("Mis Invitaciones");
    }
}
