using System.Text;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using MudBlazor.Services;
using WebApp.Models.Projects;
using WebApp.Services;

namespace WebApp.Tests.Pages;

public sealed class AcceptInvitationPageShould : TestContext
{
    private void SetupServices(ProjectInvitation? invitation = null, bool throwOnLoad = false)
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            if (throwOnLoad)
                throw new HttpRequestException("Not found");

            if (invitation != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(invitation);
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }

            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
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
    }

    [Fact]
    public void RedirectToProjects_WhenTokenEmpty()
    {
        SetupServices();
        var nav = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();

        RenderComponent<WebApp.Pages.AcceptInvitation>(p => p.Add(x => x.Token, ""));

        nav.Uri.Should().Contain("/projects");
    }

    [Fact]
    public void ShowNotFound_WhenInvitationNotFound()
    {
        SetupServices(invitation: null, throwOnLoad: true);

        var cut = RenderComponent<WebApp.Pages.AcceptInvitation>(p => p.Add(x => x.Token, "invalid-token"));
        cut.WaitForState(() => cut.Markup.Contains("no encontrada"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("no encontrada");
    }

    [Fact]
    public void ShowPendingInvitation_WhenStatusIsPending()
    {
        var invitation = new ProjectInvitation
        {
            IdProjectInvitation = 1,
            ProjectId = 10,
            ProjectName = "Test Project",
            Email = "test@test.com",
            RoleName = "Member",
            Token = "valid-token",
            Status = "Pending",
            InvitedByName = "Admin User",
            InvitedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6)
        };
        SetupServices(invitation: invitation);

        var cut = RenderComponent<WebApp.Pages.AcceptInvitation>(p => p.Add(x => x.Token, "valid-token"));
        cut.WaitForState(() => cut.Markup.Contains("Invitación al Proyecto"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("Invitación al Proyecto");
        cut.Markup.Should().Contain("Test Project");
    }

    [Fact]
    public void ShowAccepted_WhenStatusIsAccepted()
    {
        var invitation = new ProjectInvitation
        {
            IdProjectInvitation = 1,
            ProjectId = 10,
            ProjectName = "Test Project",
            Email = "test@test.com",
            RoleName = "Member",
            Token = "accepted-token",
            Status = "Accepted",
            InvitedByName = "Admin User",
            InvitedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6)
        };
        SetupServices(invitation: invitation);

        var cut = RenderComponent<WebApp.Pages.AcceptInvitation>(p => p.Add(x => x.Token, "accepted-token"));
        cut.WaitForState(() => cut.Markup.Contains("Ya Aceptaste"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("Ya Aceptaste");
    }

    [Fact]
    public void ShowRejected_WhenStatusIsRejected()
    {
        var invitation = new ProjectInvitation
        {
            IdProjectInvitation = 1,
            ProjectId = 10,
            ProjectName = "Test Project",
            Email = "test@test.com",
            RoleName = "Member",
            Token = "rejected-token",
            Status = "Rejected",
            InvitedByName = "Admin User",
            InvitedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6)
        };
        SetupServices(invitation: invitation);

        var cut = RenderComponent<WebApp.Pages.AcceptInvitation>(p => p.Add(x => x.Token, "rejected-token"));
        cut.WaitForState(() => cut.Markup.Contains("Rechazada"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("Rechazada");
    }

    [Fact]
    public void ShowExpired_WhenInvitationExpired()
    {
        var invitation = new ProjectInvitation
        {
            IdProjectInvitation = 1,
            ProjectId = 10,
            ProjectName = "Test Project",
            Email = "test@test.com",
            RoleName = "Member",
            Token = "expired-token",
            Status = "Expired",
            InvitedByName = "Admin User",
            InvitedAt = DateTime.UtcNow.AddDays(-10),
            ExpiresAt = DateTime.UtcNow.AddDays(-3)
        };
        SetupServices(invitation: invitation);

        var cut = RenderComponent<WebApp.Pages.AcceptInvitation>(p => p.Add(x => x.Token, "expired-token"));
        cut.WaitForState(() => cut.Markup.Contains("Expirada"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("Expirada");
    }

    [Fact]
    public void AcceptInvitation_Success_NavigatesToProject()
    {
        var invitation = new ProjectInvitation
        {
            IdProjectInvitation = 1, ProjectId = 10, ProjectName = "Test Project",
            Email = "test@test.com", RoleName = "Member", Token = "accept-tok",
            Status = "Pending", InvitedByName = "Admin User",
            InvitedAt = DateTime.UtcNow.AddDays(-1), ExpiresAt = DateTime.UtcNow.AddDays(6)
        };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var requestCount = 0;
        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            requestCount++;
            if (req.Method == HttpMethod.Post)
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            }
            var json = System.Text.Json.JsonSerializer.Serialize(invitation);
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
        var cut = RenderComponent<WebApp.Pages.AcceptInvitation>(p => p.Add(x => x.Token, "accept-tok"));
        cut.WaitForState(() => cut.Markup.Contains("Aceptar Invitación"), TimeSpan.FromSeconds(3));

        var acceptBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Aceptar"));
        acceptBtn.Click();

        cut.WaitForState(() => requestCount >= 2, TimeSpan.FromSeconds(5));
        requestCount.Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public void AcceptInvitation_Failure_ShowsError()
    {
        var invitation = new ProjectInvitation
        {
            IdProjectInvitation = 1, ProjectId = 10, ProjectName = "Test Project",
            Email = "test@test.com", RoleName = "Member", Token = "fail-tok",
            Status = "Pending", InvitedByName = "Admin User",
            InvitedAt = DateTime.UtcNow.AddDays(-1), ExpiresAt = DateTime.UtcNow.AddDays(6)
        };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            if (req.Method == HttpMethod.Post)
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
            }
            var json = System.Text.Json.JsonSerializer.Serialize(invitation);
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

        var cut = RenderComponent<WebApp.Pages.AcceptInvitation>(p => p.Add(x => x.Token, "fail-tok"));
        cut.WaitForState(() => cut.Markup.Contains("Aceptar Invitación"), TimeSpan.FromSeconds(3));

        var acceptBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Aceptar"));
        acceptBtn.Click();

        cut.WaitForState(() => cut.Markup.Contains("No se pudo aceptar"), TimeSpan.FromSeconds(3));
        cut.Markup.Should().Contain("No se pudo aceptar");
    }

    [Fact]
    public void RejectInvitation_Success_NavigatesToProjects()
    {
        var invitation = new ProjectInvitation
        {
            IdProjectInvitation = 1, ProjectId = 10, ProjectName = "Test Project",
            Email = "test@test.com", RoleName = "Member", Token = "reject-tok",
            Status = "Pending", InvitedByName = "Admin User",
            InvitedAt = DateTime.UtcNow.AddDays(-1), ExpiresAt = DateTime.UtcNow.AddDays(6)
        };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var requestCount = 0;
        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            requestCount++;
            if (req.Method == HttpMethod.Post)
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            }
            var json = System.Text.Json.JsonSerializer.Serialize(invitation);
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

        var cut = RenderComponent<WebApp.Pages.AcceptInvitation>(p => p.Add(x => x.Token, "reject-tok"));
        cut.WaitForState(() => cut.Markup.Contains("Rechazar"), TimeSpan.FromSeconds(3));

        var rejectBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Rechazar"));
        rejectBtn.Click();

        cut.WaitForState(() => requestCount >= 2, TimeSpan.FromSeconds(5));
        requestCount.Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public void RejectInvitation_Failure_ShowsError()
    {
        var invitation = new ProjectInvitation
        {
            IdProjectInvitation = 1, ProjectId = 10, ProjectName = "Test Project",
            Email = "test@test.com", RoleName = "Admin", Token = "reject-fail",
            Status = "Pending", InvitedByName = "Admin User",
            InvitedAt = DateTime.UtcNow.AddDays(-1), ExpiresAt = DateTime.UtcNow.AddDays(6)
        };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            if (req.Method == HttpMethod.Post)
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
            }
            var json = System.Text.Json.JsonSerializer.Serialize(invitation);
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

        var cut = RenderComponent<WebApp.Pages.AcceptInvitation>(p => p.Add(x => x.Token, "reject-fail"));
        cut.WaitForState(() => cut.Markup.Contains("Rechazar"), TimeSpan.FromSeconds(3));

        var rejectBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Rechazar"));
        rejectBtn.Click();

        cut.WaitForState(() => cut.Markup.Contains("No se pudo rechazar"), TimeSpan.FromSeconds(3));
        cut.Markup.Should().Contain("No se pudo rechazar");
    }

    [Fact]
    public void ShowPendingInvitation_WithDifferentRoles()
    {
        var invitation = new ProjectInvitation
        {
            IdProjectInvitation = 1, ProjectId = 10, ProjectName = "Role Test",
            Email = "test@test.com", RoleName = "Owner", Token = "role-tok",
            Status = "Pending", InvitedByName = "Admin User",
            InvitedAt = DateTime.UtcNow.AddDays(-1), ExpiresAt = DateTime.UtcNow.AddDays(6)
        };
        SetupServices(invitation: invitation);

        var cut = RenderComponent<WebApp.Pages.AcceptInvitation>(p => p.Add(x => x.Token, "role-tok"));
        cut.WaitForState(() => cut.Markup.Contains("Role Test"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("Owner");
        cut.Markup.Should().Contain("Control total del proyecto");
    }

    [Fact]
    public void ShowPendingInvitation_WithViewerRole()
    {
        var invitation = new ProjectInvitation
        {
            IdProjectInvitation = 1, ProjectId = 10, ProjectName = "Viewer Test",
            Email = "test@test.com", RoleName = "Viewer", Token = "viewer-tok",
            Status = "Pending", InvitedByName = "Admin User",
            InvitedAt = DateTime.UtcNow.AddDays(-1), ExpiresAt = DateTime.UtcNow.AddDays(6)
        };
        SetupServices(invitation: invitation);

        var cut = RenderComponent<WebApp.Pages.AcceptInvitation>(p => p.Add(x => x.Token, "viewer-tok"));
        cut.WaitForState(() => cut.Markup.Contains("Viewer Test"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("Solo lectura");
    }

    [Fact]
    public void ShowPendingInvitation_WithAdminRole()
    {
        var invitation = new ProjectInvitation
        {
            IdProjectInvitation = 1, ProjectId = 10, ProjectName = "Admin Test",
            Email = "test@test.com", RoleName = "Admin", Token = "admin-tok",
            Status = "Pending", InvitedByName = "Admin User",
            InvitedAt = DateTime.UtcNow.AddDays(-1), ExpiresAt = DateTime.UtcNow.AddDays(6)
        };
        SetupServices(invitation: invitation);

        var cut = RenderComponent<WebApp.Pages.AcceptInvitation>(p => p.Add(x => x.Token, "admin-tok"));
        cut.WaitForState(() => cut.Markup.Contains("Admin Test"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("Gestión completa");
    }

    [Fact]
    public void ShowPendingInvitation_WithUnknownRole()
    {
        var invitation = new ProjectInvitation
        {
            IdProjectInvitation = 1, ProjectId = 10, ProjectName = "Unknown Role",
            Email = "test@test.com", RoleName = "Custom", Token = "custom-tok",
            Status = "Pending", InvitedByName = "Admin User",
            InvitedAt = DateTime.UtcNow.AddDays(-1), ExpiresAt = DateTime.UtcNow.AddDays(6)
        };
        SetupServices(invitation: invitation);

        var cut = RenderComponent<WebApp.Pages.AcceptInvitation>(p => p.Add(x => x.Token, "custom-tok"));
        cut.WaitForState(() => cut.Markup.Contains("Unknown Role"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("Rol desconocido");
    }

    [Fact]
    public void AcceptInvitation_HttpError404_ShowsNotFoundMessage()
    {
        var invitation = new ProjectInvitation
        {
            IdProjectInvitation = 1, ProjectId = 10, ProjectName = "Test 404",
            Email = "test@test.com", RoleName = "Member", Token = "err404",
            Status = "Pending", InvitedByName = "Admin User",
            InvitedAt = DateTime.UtcNow.AddDays(-1), ExpiresAt = DateTime.UtcNow.AddDays(6)
        };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            if (req.Method == HttpMethod.Post)
                throw new HttpRequestException("Response status code does not indicate success: 404");
            var json = System.Text.Json.JsonSerializer.Serialize(invitation);
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

        var cut = RenderComponent<WebApp.Pages.AcceptInvitation>(p => p.Add(x => x.Token, "err404"));
        cut.WaitForState(() => cut.Markup.Contains("Aceptar Invitación"), TimeSpan.FromSeconds(3));

        var acceptBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Aceptar"));
        acceptBtn.Click();

        cut.WaitForState(() => cut.Markup.Contains("no existe"), TimeSpan.FromSeconds(3));
        cut.Markup.Should().Contain("no existe");
    }

    [Fact]
    public void AcceptInvitation_HttpError409_ShowsConflictMessage()
    {
        var invitation = new ProjectInvitation
        {
            IdProjectInvitation = 1, ProjectId = 10, ProjectName = "Test 409",
            Email = "test@test.com", RoleName = "Member", Token = "err409",
            Status = "Pending", InvitedByName = "Admin User",
            InvitedAt = DateTime.UtcNow.AddDays(-1), ExpiresAt = DateTime.UtcNow.AddDays(6)
        };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            if (req.Method == HttpMethod.Post)
                throw new HttpRequestException("Response status code does not indicate success: 409");
            var json = System.Text.Json.JsonSerializer.Serialize(invitation);
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

        var cut = RenderComponent<WebApp.Pages.AcceptInvitation>(p => p.Add(x => x.Token, "err409"));
        cut.WaitForState(() => cut.Markup.Contains("Aceptar Invitación"), TimeSpan.FromSeconds(3));

        var acceptBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Aceptar"));
        acceptBtn.Click();

        cut.WaitForState(() => cut.Markup.Contains("Ya eres miembro"), TimeSpan.FromSeconds(3));
        cut.Markup.Should().Contain("Ya eres miembro");
    }

    [Fact]
    public void AcceptInvitation_GenericException_ShowsError()
    {
        var invitation = new ProjectInvitation
        {
            IdProjectInvitation = 1, ProjectId = 10, ProjectName = "Test Exc",
            Email = "test@test.com", RoleName = "Member", Token = "errgen",
            Status = "Pending", InvitedByName = "Admin User",
            InvitedAt = DateTime.UtcNow.AddDays(-1), ExpiresAt = DateTime.UtcNow.AddDays(6)
        };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            if (req.Method == HttpMethod.Post)
                throw new InvalidOperationException("Unexpected failure");
            var json = System.Text.Json.JsonSerializer.Serialize(invitation);
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

        var cut = RenderComponent<WebApp.Pages.AcceptInvitation>(p => p.Add(x => x.Token, "errgen"));
        cut.WaitForState(() => cut.Markup.Contains("Aceptar Invitación"), TimeSpan.FromSeconds(3));

        var acceptBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Aceptar"));
        acceptBtn.Click();

        cut.WaitForState(() => cut.Markup.Contains("Error inesperado"), TimeSpan.FromSeconds(3));
        cut.Markup.Should().Contain("Error inesperado");
    }

    [Fact]
    public void RejectInvitation_Exception_ShowsError()
    {
        var invitation = new ProjectInvitation
        {
            IdProjectInvitation = 1, ProjectId = 10, ProjectName = "Test Reject Exc",
            Email = "test@test.com", RoleName = "Member", Token = "rejexc",
            Status = "Pending", InvitedByName = "Admin User",
            InvitedAt = DateTime.UtcNow.AddDays(-1), ExpiresAt = DateTime.UtcNow.AddDays(6)
        };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            if (req.Method == HttpMethod.Post)
                throw new InvalidOperationException("Reject network failure");
            var json = System.Text.Json.JsonSerializer.Serialize(invitation);
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

        var cut = RenderComponent<WebApp.Pages.AcceptInvitation>(p => p.Add(x => x.Token, "rejexc"));
        cut.WaitForState(() => cut.Markup.Contains("Rechazar"), TimeSpan.FromSeconds(3));

        var rejectBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Rechazar"));
        rejectBtn.Click();

        cut.WaitForState(() => cut.Markup.Contains("Error al rechazar"), TimeSpan.FromSeconds(3));
        cut.Markup.Should().Contain("Error al rechazar");
    }

    [Fact]
    public void AcceptInvitation_HttpErrorGeneric_ShowsGenericMessage()
    {
        var invitation = new ProjectInvitation
        {
            IdProjectInvitation = 1, ProjectId = 10, ProjectName = "Test Generic",
            Email = "test@test.com", RoleName = "Member", Token = "errhttp",
            Status = "Pending", InvitedByName = "Admin User",
            InvitedAt = DateTime.UtcNow.AddDays(-1), ExpiresAt = DateTime.UtcNow.AddDays(6)
        };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            if (req.Method == HttpMethod.Post)
                throw new HttpRequestException("Response status code does not indicate success: 500");
            var json = System.Text.Json.JsonSerializer.Serialize(invitation);
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

        var cut = RenderComponent<WebApp.Pages.AcceptInvitation>(p => p.Add(x => x.Token, "errhttp"));
        cut.WaitForState(() => cut.Markup.Contains("Aceptar Invitación"), TimeSpan.FromSeconds(3));

        var acceptBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Aceptar"));
        acceptBtn.Click();

        cut.WaitForState(() => cut.Markup.Contains("Error al aceptar"), TimeSpan.FromSeconds(3));
        cut.Markup.Should().Contain("Error al aceptar");
    }
}
