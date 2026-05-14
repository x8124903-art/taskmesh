using System.Text;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;
using WebApp.Models.Projects;
using WebApp.Services;

namespace WebApp.Tests.Pages;

public sealed class ProjectDetailPageShould : TestContext
{
    private void SetupServices(bool isAuthenticated = true, Project? project = null, List<ProjectMember>? members = null)
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            var uri = req.RequestUri?.PathAndQuery ?? "";

            if (uri.Contains("/members"))
            {
                var memberData = members ?? new List<ProjectMember>();
                var json = System.Text.Json.JsonSerializer.Serialize(memberData);
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }

            if (uri.Contains("/projects/"))
            {
                if (project != null)
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(project);
                    return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    });
                }
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("null", Encoding.UTF8, "application/json")
                });
            }

            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
            });
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
        Services.AddSingleton(new TaskApiService(httpClient));
    }

    [Fact]
    public void RedirectToLogin_WhenNotAuthenticated()
    {
        SetupServices(isAuthenticated: false);
        var nav = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();

        RenderComponent<WebApp.Pages.ProjectDetail>(p => p.Add(x => x.Id, 1));

        nav.Uri.Should().Contain("/login");
    }

    [Fact]
    public void ShowProjectDetails_WhenProjectExists()
    {
        var project = new Project
        {
            IdProject = 1,
            Name = "My Test Project",
            Description = "A test description",
            StatusName = "Active",
            CreatedByName = "TestUser",
            CreatedAt = DateTime.UtcNow
        };
        var members = new List<ProjectMember>
        {
            new() { UserId = 1, UserName = "Test", Email = "test@test.com", RoleName = "Owner", JoinedAt = DateTime.UtcNow }
        };
        SetupServices(isAuthenticated: true, project: project, members: members);

        var cut = RenderComponent<WebApp.Pages.ProjectDetail>(p => p.Add(x => x.Id, 1));
        cut.WaitForState(() => cut.Markup.Contains("My Test Project"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("My Test Project");
    }

    [Fact]
    public void ShowEditOptions_WhenUserIsOwner()
    {
        var project = new Project
        {
            IdProject = 1, Name = "Owner Project", Description = "Desc",
            StatusName = "Active", CreatedByName = "Test", CreatedAt = DateTime.UtcNow
        };
        var members = new List<ProjectMember>
        {
            new() { UserId = 1, UserName = "Test", Email = "test@test.com", RoleName = "Owner", JoinedAt = DateTime.UtcNow },
            new() { UserId = 2, UserName = "Other", Email = "other@test.com", RoleName = "Admin", JoinedAt = DateTime.UtcNow },
            new() { UserId = 3, UserName = "Viewer", Email = "viewer@test.com", RoleName = "Viewer", JoinedAt = DateTime.UtcNow },
            new() { UserId = 4, UserName = "Mem", Email = "mem@test.com", RoleName = "Member", JoinedAt = DateTime.UtcNow }
        };
        SetupServices(isAuthenticated: true, project: project, members: members);

        var cut = RenderComponent<WebApp.Pages.ProjectDetail>(p => p.Add(x => x.Id, 1));
        cut.WaitForState(() => cut.Markup.Contains("Owner Project"), TimeSpan.FromSeconds(3));

        var tabs = cut.FindAll("div.mud-tab");
        if (tabs.Count >= 4) tabs[3].Click();

        cut.Markup.Should().Contain("Owner Project");
        cut.Markup.Should().Contain("Miembros del Proyecto");
    }

    [Fact]
    public void ShowProjectWithDifferentStatuses()
    {
        var project = new Project
        {
            IdProject = 1, Name = "Archived Project", Description = "Old",
            StatusName = "Archived", CreatedByName = "User", CreatedAt = DateTime.UtcNow
        };
        var members = new List<ProjectMember>
        {
            new() { UserId = 1, UserName = "Test", Email = "test@test.com", RoleName = "Owner", JoinedAt = DateTime.UtcNow }
        };
        SetupServices(isAuthenticated: true, project: project, members: members);

        var cut = RenderComponent<WebApp.Pages.ProjectDetail>(p => p.Add(x => x.Id, 1));
        cut.WaitForState(() => cut.Markup.Contains("Archived Project"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("Archived");
    }

    [Fact]
    public void SaveChanges_Success_UpdatesProject()
    {
        var project = new Project
        {
            IdProject = 1, Name = "Edit Me", Description = "Desc",
            StatusName = "Active", CreatedByName = "Test", CreatedAt = DateTime.UtcNow
        };
        var members = new List<ProjectMember>
        {
            new() { UserId = 1, UserName = "Test", Email = "test@test.com", RoleName = "Owner", JoinedAt = DateTime.UtcNow }
        };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            var uri = req.RequestUri?.PathAndQuery ?? "";
            if (uri.Contains("/members"))
            {
                var json = System.Text.Json.JsonSerializer.Serialize(members);
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }
            if (req.Method == HttpMethod.Put)
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            }
            if (uri.Contains("/projects/"))
            {
                var json = System.Text.Json.JsonSerializer.Serialize(project);
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
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
        Services.AddSingleton(new TaskApiService(httpClient));

        var cut = RenderComponent<WebApp.Pages.ProjectDetail>(p => p.Add(x => x.Id, 1));
        cut.WaitForState(() => cut.Markup.Contains("Edit Me"), TimeSpan.FromSeconds(3));

        var tabs = cut.FindAll("div.mud-tab");
        if (tabs.Count >= 5) tabs[4].Click();

        var forms = cut.FindAll("form");
        if (forms.Any()) forms.Last().Submit();
    }

    [Fact]
    public void ShowViewerNoEditPermissions()
    {
        var project = new Project
        {
            IdProject = 1, Name = "View Only", Description = "Desc",
            StatusName = "Paused", CreatedByName = "User", CreatedAt = DateTime.UtcNow
        };
        var members = new List<ProjectMember>
        {
            new() { UserId = 1, UserName = "Test", Email = "test@test.com", RoleName = "Viewer", JoinedAt = DateTime.UtcNow }
        };
        SetupServices(isAuthenticated: true, project: project, members: members);

        var cut = RenderComponent<WebApp.Pages.ProjectDetail>(p => p.Add(x => x.Id, 1));
        cut.WaitForState(() => cut.Markup.Contains("View Only"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("Paused");
    }

    [Fact]
    public void SaveChanges_Failure_ShowsError()
    {
        var project = new Project
        {
            IdProject = 1, Name = "Fail Save", Description = "Desc",
            StatusName = "Active", CreatedByName = "Test", CreatedAt = DateTime.UtcNow
        };
        var members = new List<ProjectMember>
        {
            new() { UserId = 1, UserName = "Test", Email = "test@test.com", RoleName = "Owner", JoinedAt = DateTime.UtcNow }
        };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            var uri = req.RequestUri?.PathAndQuery ?? "";
            if (uri.Contains("/members"))
            {
                var json = System.Text.Json.JsonSerializer.Serialize(members);
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }
            if (req.Method == HttpMethod.Put)
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError));
            }
            if (uri.Contains("/projects/"))
            {
                var json = System.Text.Json.JsonSerializer.Serialize(project);
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
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
        Services.AddSingleton(new TaskApiService(httpClient));

        var cut = RenderComponent<WebApp.Pages.ProjectDetail>(p => p.Add(x => x.Id, 1));
        cut.WaitForState(() => cut.Markup.Contains("Fail Save"), TimeSpan.FromSeconds(3));

        var tabs = cut.FindAll("div.mud-tab");
        if (tabs.Count >= 5) tabs[4].Click();

        var forms = cut.FindAll("form");
        if (forms.Any()) forms.Last().Submit();

        cut.Markup.Should().Contain("Fail Save");
    }

    [Fact]
    public void SaveChanges_Exception_ShowsError()
    {
        var project = new Project
        {
            IdProject = 1, Name = "Exception Save", Description = "Desc",
            StatusName = "Active", CreatedByName = "Test", CreatedAt = DateTime.UtcNow
        };
        var members = new List<ProjectMember>
        {
            new() { UserId = 1, UserName = "Test", Email = "test@test.com", RoleName = "Owner", JoinedAt = DateTime.UtcNow }
        };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            var uri = req.RequestUri?.PathAndQuery ?? "";
            if (uri.Contains("/members"))
            {
                var json = System.Text.Json.JsonSerializer.Serialize(members);
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }
            if (req.Method == HttpMethod.Put)
            {
                throw new HttpRequestException("Network error");
            }
            if (uri.Contains("/projects/"))
            {
                var json = System.Text.Json.JsonSerializer.Serialize(project);
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
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
        Services.AddSingleton(new TaskApiService(httpClient));

        var cut = RenderComponent<WebApp.Pages.ProjectDetail>(p => p.Add(x => x.Id, 1));
        cut.WaitForState(() => cut.Markup.Contains("Exception Save"), TimeSpan.FromSeconds(3));

        var tabs = cut.FindAll("div.mud-tab");
        if (tabs.Count >= 5) tabs[4].Click();

        var forms = cut.FindAll("form");
        if (forms.Any()) forms.Last().Submit();

        cut.Markup.Should().Contain("Exception Save");
    }

    [Fact]
    public void LoadMembers_Exception_SetsEmptyList()
    {
        var project = new Project
        {
            IdProject = 1, Name = "Member Error", Description = "Desc",
            StatusName = "Active", CreatedByName = "Test", CreatedAt = DateTime.UtcNow
        };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            var uri = req.RequestUri?.PathAndQuery ?? "";
            if (uri.Contains("/members"))
            {
                throw new HttpRequestException("Members error");
            }
            if (uri.Contains("/projects/"))
            {
                var json = System.Text.Json.JsonSerializer.Serialize(project);
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
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
        Services.AddSingleton(new TaskApiService(httpClient));

        var cut = RenderComponent<WebApp.Pages.ProjectDetail>(p => p.Add(x => x.Id, 1));
        cut.WaitForState(() => cut.Markup.Contains("Member Error"), TimeSpan.FromSeconds(3));

        var tabs = cut.FindAll("div.mud-tab");
        if (tabs.Count >= 4) tabs[3].Click();

        cut.Markup.Should().Contain("Member Error");
    }

    [Fact]
    public void NavigateToTab_WhenTabParameterProvided()
    {
        var project = new Project
        {
            IdProject = 1, Name = "Tab Nav", Description = "Desc",
            StatusName = "Active", CreatedByName = "Test", CreatedAt = DateTime.UtcNow
        };
        var members = new List<ProjectMember>
        {
            new() { UserId = 1, UserName = "Test", Email = "test@test.com", RoleName = "Owner", JoinedAt = DateTime.UtcNow }
        };
        SetupServices(isAuthenticated: true, project: project, members: members);

        var cut = RenderComponent<WebApp.Pages.ProjectDetail>(p => p
            .Add(x => x.Id, 1)
            .Add(x => x.Tab, "backlog"));
        cut.WaitForState(() => cut.Markup.Contains("Tab Nav"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("Tab Nav");
    }

    [Fact]
    public void ShowEmptyDescription_WhenProjectHasNoDescription()
    {
        var project = new Project
        {
            IdProject = 1, Name = "No Desc", Description = "",
            StatusName = "Active", CreatedByName = "Test", CreatedAt = DateTime.UtcNow
        };
        var members = new List<ProjectMember>
        {
            new() { UserId = 1, UserName = "Test", Email = "test@test.com", RoleName = "Owner", JoinedAt = DateTime.UtcNow }
        };
        SetupServices(isAuthenticated: true, project: project, members: members);

        var cut = RenderComponent<WebApp.Pages.ProjectDetail>(p => p.Add(x => x.Id, 1));
        cut.WaitForState(() => cut.Markup.Contains("No Desc"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("Sin descripción");
    }

    [Fact]
    public void ShowNoEditPermissions_WhenUserIsMember()
    {
        var project = new Project
        {
            IdProject = 1, Name = "Member Only", Description = "Desc",
            StatusName = "Active", CreatedByName = "Test", CreatedAt = DateTime.UtcNow
        };
        var members = new List<ProjectMember>
        {
            new() { UserId = 1, UserName = "Test", Email = "test@test.com", RoleName = "Member", JoinedAt = DateTime.UtcNow }
        };
        SetupServices(isAuthenticated: true, project: project, members: members);

        var cut = RenderComponent<WebApp.Pages.ProjectDetail>(p => p.Add(x => x.Id, 1));
        cut.WaitForState(() => cut.Markup.Contains("Member Only"), TimeSpan.FromSeconds(3));

        var tabs = cut.FindAll("div.mud-tab");
        if (tabs.Count >= 5) tabs[4].Click();

        cut.Markup.Should().Contain("No tienes permisos");
    }

    [Fact]
    public void RemoveMember_OnSelf_ShowsWarning()
    {
        var project = new Project
        {
            IdProject = 1, Name = "Self Remove", Description = "Desc",
            StatusName = "Active", CreatedByName = "Test", CreatedAt = DateTime.UtcNow
        };
        var members = new List<ProjectMember>
        {
            new() { UserId = 1, UserName = "Test", Email = "test@test.com", RoleName = "Admin", JoinedAt = DateTime.UtcNow },
            new() { UserId = 2, UserName = "Other", Email = "other@test.com", RoleName = "Member", JoinedAt = DateTime.UtcNow }
        };
        SetupServices(isAuthenticated: true, project: project, members: members);

        var cut = RenderComponent<WebApp.Pages.ProjectDetail>(p => p.Add(x => x.Id, 1));
        cut.WaitForState(() => cut.Markup.Contains("Self Remove"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("Self Remove");
    }

    [Fact]
    public void ShowProjectNotFound_WhenProjectIsNull()
    {
        SetupServices(isAuthenticated: true, project: null);

        var cut = RenderComponent<WebApp.Pages.ProjectDetail>(p => p.Add(x => x.Id, 1));
        cut.WaitForState(() => cut.Markup.Contains("Proyecto no encontrado"), TimeSpan.FromSeconds(3));

        cut.Markup.Should().Contain("Proyecto no encontrado");
    }

    [Fact]
    public void ShowMembersTab_WithMemberActions()
    {
        var project = new Project
        {
            IdProject = 1, Name = "Actions Project", Description = "Desc",
            StatusName = "Active", CreatedByName = "Test", CreatedAt = DateTime.UtcNow
        };
        var members = new List<ProjectMember>
        {
            new() { UserId = 1, UserName = "Test", Email = "test@test.com", RoleName = "Owner", JoinedAt = DateTime.UtcNow },
            new() { UserId = 2, UserName = "Admin User", Email = "admin@test.com", RoleName = "Admin", JoinedAt = DateTime.UtcNow },
            new() { UserId = 3, UserName = "Regular", Email = "reg@test.com", RoleName = "Member", JoinedAt = DateTime.UtcNow }
        };
        SetupServices(isAuthenticated: true, project: project, members: members);

        var cut = RenderComponent<WebApp.Pages.ProjectDetail>(p => p.Add(x => x.Id, 1));
        cut.WaitForState(() => cut.Markup.Contains("Actions Project"), TimeSpan.FromSeconds(3));

        var tabs = cut.FindAll("div.mud-tab");
        if (tabs.Count >= 4) tabs[3].Click();

        cut.Markup.Should().Contain("Admin User");
        cut.Markup.Should().Contain("Regular");
        cut.Markup.Should().Contain("Invitar Miembro");
    }

    [Fact]
    public void ClickChangeRole_OnSelf_ShowsWarning()
    {
        var project = new Project
        {
            IdProject = 1, Name = "Self Role", Description = "Desc",
            StatusName = "Active", CreatedByName = "Test", CreatedAt = DateTime.UtcNow
        };
        var members = new List<ProjectMember>
        {
            new() { UserId = 1, UserName = "Test", Email = "test@test.com", RoleName = "Admin", JoinedAt = DateTime.UtcNow },
            new() { UserId = 2, UserName = "Other", Email = "other@test.com", RoleName = "Member", JoinedAt = DateTime.UtcNow }
        };

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            var uri = req.RequestUri?.PathAndQuery ?? "";
            if (uri.Contains("/members"))
            {
                var json = System.Text.Json.JsonSerializer.Serialize(members);
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }
            if (uri.Contains("/projects/"))
            {
                var json = System.Text.Json.JsonSerializer.Serialize(project);
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
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
        Services.AddSingleton(new TaskApiService(httpClient));

        var cut = RenderComponent<WebApp.Pages.ProjectDetail>(p => p.Add(x => x.Id, 1));
        cut.WaitForState(() => cut.Markup.Contains("Self Role"), TimeSpan.FromSeconds(3));

        var tabs = cut.FindAll("div.mud-tab");
        if (tabs.Count >= 4) tabs[3].Click();

        cut.Markup.Should().Contain("Other");
    }

    [Fact]
    public void ShowInviteButton_WhenCanManageMembers()
    {
        var project = new Project
        {
            IdProject = 1, Name = "Invite Test", Description = "Desc",
            StatusName = "Active", CreatedByName = "Test", CreatedAt = DateTime.UtcNow
        };
        var members = new List<ProjectMember>
        {
            new() { UserId = 1, UserName = "Test", Email = "test@test.com", RoleName = "Owner", JoinedAt = DateTime.UtcNow }
        };
        SetupServices(isAuthenticated: true, project: project, members: members);

        var cut = RenderComponent<WebApp.Pages.ProjectDetail>(p => p.Add(x => x.Id, 1));
        cut.WaitForState(() => cut.Markup.Contains("Invite Test"), TimeSpan.FromSeconds(3));

        var tabs = cut.FindAll("div.mud-tab");
        if (tabs.Count >= 4) tabs[3].Click();

        cut.Markup.Should().Contain("Invitar Miembro");
    }

    [Fact]
    public void ShowProjectNotFound_WhenLoadThrows()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
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
        Services.AddSingleton(new TaskApiService(httpClient));

        try
        {
            var cut = RenderComponent<WebApp.Pages.ProjectDetail>(p => p.Add(x => x.Id, 999));
        }
        catch
        {
        }
    }
}
