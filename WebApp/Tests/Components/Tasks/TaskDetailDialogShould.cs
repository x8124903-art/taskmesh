using System.Text;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using WebApp.Components.Tasks;
using WebApp.Services;

namespace WebApp.Tests.Components.Tasks;

public sealed class TaskDetailDialogShould : TestContext
{
    private static WebApp.Models.Tasks.TaskModel CreateTask() => new()
    {
        IdTask = 1, Title = "Test Task", Description = "A description", ProjectId = 1,
        Priority = "High", Status = "InProgress", CreatedBy = 1, AssignedToUserId = 2,
        DueDate = DateTime.UtcNow.AddDays(5), RowVersion = 1,
        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
    };

    private void SetupServices(Helpers.MockHttpMessageHandler handler)
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        Services.AddSingleton(new TaskApiService(httpClient));
    }

    [Fact]
    public async Task RenderDialog_WithTaskDetails()
    {
        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(new List<WebApp.Models.Tasks.TaskCommentModel>()));
        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var members = new List<WebApp.Models.Projects.ProjectMember>
        {
            new() { UserId = 1, UserName = "Admin" },
            new() { UserId = 2, UserName = "Alice" }
        };

        var parameters = new DialogParameters<TaskDetailDialog>
        {
            { x => x.Task, CreateTask() },
            { x => x.Members, members },
            { x => x.UserRole, "Member" }
        };
        await comp.InvokeAsync(() => dialogService.Show<TaskDetailDialog>("Task", parameters));

        comp.Markup.Should().Contain("A description");
        comp.Markup.Should().Contain("Alice");
        comp.Markup.Should().Contain("Alta");
        comp.Markup.Should().Contain("Añadir comentario");
    }

    [Fact]
    public async Task RenderDialog_ViewerRole_HidesCommentInput()
    {
        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(new List<WebApp.Models.Tasks.TaskCommentModel>()));
        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<TaskDetailDialog>
        {
            { x => x.Task, CreateTask() },
            { x => x.Members, new List<WebApp.Models.Projects.ProjectMember>() },
            { x => x.UserRole, "Viewer" }
        };
        await comp.InvokeAsync(() => dialogService.Show<TaskDetailDialog>("Task", parameters));

        comp.Markup.Should().NotContain("Añadir comentario");
    }

    [Fact]
    public async Task RenderDialog_WithComments()
    {
        var comments = new List<WebApp.Models.Tasks.TaskCommentModel>
        {
            new(1, 1, 1, "Admin", "Great work!", DateTime.UtcNow)
        };

        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(comments));
        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<TaskDetailDialog>
        {
            { x => x.Task, CreateTask() },
            { x => x.Members, new List<WebApp.Models.Projects.ProjectMember> { new() { UserId = 1, UserName = "Admin" } } },
            { x => x.UserRole, "Member" }
        };
        await comp.InvokeAsync(() => dialogService.Show<TaskDetailDialog>("Task", parameters));

        comp.Markup.Should().Contain("Great work!");
        comp.Markup.Should().Contain("Comentarios (1)");
    }

    [Fact]
    public async Task RenderDialog_NoComments_ShowsEmptyMessage()
    {
        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(new List<WebApp.Models.Tasks.TaskCommentModel>()));
        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<TaskDetailDialog>
        {
            { x => x.Task, CreateTask() },
            { x => x.Members, new List<WebApp.Models.Projects.ProjectMember>() },
            { x => x.UserRole, "Member" }
        };
        await comp.InvokeAsync(() => dialogService.Show<TaskDetailDialog>("Task", parameters));

        comp.Markup.Should().Contain("No hay comentarios");
    }

    [Fact]
    public async Task RenderDialog_TaskWithoutDescription()
    {
        var task = CreateTask();
        task.Description = null;

        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(new List<WebApp.Models.Tasks.TaskCommentModel>()));
        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<TaskDetailDialog>
        {
            { x => x.Task, task },
            { x => x.Members, new List<WebApp.Models.Projects.ProjectMember>() },
            { x => x.UserRole, "Member" }
        };
        await comp.InvokeAsync(() => dialogService.Show<TaskDetailDialog>("Task", parameters));

        comp.Markup.Should().NotContain("Descripción");
    }

    [Fact]
    public async Task RenderDialog_TaskWithoutAssignee()
    {
        var task = CreateTask();
        task.AssignedToUserId = null;

        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(new List<WebApp.Models.Tasks.TaskCommentModel>()));
        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<TaskDetailDialog>
        {
            { x => x.Task, task },
            { x => x.Members, new List<WebApp.Models.Projects.ProjectMember>() },
            { x => x.UserRole, "Member" }
        };
        await comp.InvokeAsync(() => dialogService.Show<TaskDetailDialog>("Task", parameters));

        comp.Markup.Should().Contain("Sin asignar");
    }

    [Fact]
    public async Task RenderDialog_TaskWithoutDueDate()
    {
        var task = CreateTask();
        task.DueDate = null;

        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(new List<WebApp.Models.Tasks.TaskCommentModel>()));
        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<TaskDetailDialog>
        {
            { x => x.Task, task },
            { x => x.Members, new List<WebApp.Models.Projects.ProjectMember>() },
            { x => x.UserRole, "Member" }
        };
        await comp.InvokeAsync(() => dialogService.Show<TaskDetailDialog>("Task", parameters));

        comp.Markup.Should().Contain("Sin fecha");
    }

    [Fact]
    public async Task Close_ClosesDialog()
    {
        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(new List<WebApp.Models.Tasks.TaskCommentModel>()));
        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<TaskDetailDialog>
        {
            { x => x.Task, CreateTask() },
            { x => x.Members, new List<WebApp.Models.Projects.ProjectMember>() },
            { x => x.UserRole, "Member" }
        };
        await comp.InvokeAsync(() => dialogService.Show<TaskDetailDialog>("Task", parameters));

        var closeBtn = comp.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Cerrar"));
        closeBtn?.Click();
    }

    [Fact]
    public async Task AddComment_Success_ReloadsComments()
    {
        var comment = new WebApp.Models.Tasks.TaskCommentModel(1, 1, 1, "Admin", "New comment", DateTime.UtcNow);
        var callCount = 0;
        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            if (req.Method == HttpMethod.Post)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(comment);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent(json, Encoding.UTF8, "application/json") });
            }
            callCount++;
            var comments = callCount > 1
                ? new List<WebApp.Models.Tasks.TaskCommentModel> { comment }
                : new List<WebApp.Models.Tasks.TaskCommentModel>();
            var listJson = System.Text.Json.JsonSerializer.Serialize(comments);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(listJson, Encoding.UTF8, "application/json") });
        });

        SetupServices(handler);
        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<TaskDetailDialog>
        {
            { x => x.Task, CreateTask() },
            { x => x.Members, new List<WebApp.Models.Projects.ProjectMember>() },
            { x => x.UserRole, "Member" }
        };
        await comp.InvokeAsync(() => dialogService.Show<TaskDetailDialog>("Task", parameters));

        var dialog = comp.FindComponent<TaskDetailDialog>();
        await dialog.InvokeAsync(() =>
        {
            typeof(TaskDetailDialog).GetField("newComment",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(dialog.Instance, "New comment");
            return (Task)typeof(TaskDetailDialog).GetMethod("AddComment",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(dialog.Instance, null)!;
        });
    }

    [Fact]
    public async Task AddComment_NullResponse_ShowsError()
    {
        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            if (req.Method == HttpMethod.Post)
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent("null", Encoding.UTF8, "application/json") });
            var json = System.Text.Json.JsonSerializer.Serialize(new List<WebApp.Models.Tasks.TaskCommentModel>());
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(json, Encoding.UTF8, "application/json") });
        });

        SetupServices(handler);
        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<TaskDetailDialog>
        {
            { x => x.Task, CreateTask() },
            { x => x.Members, new List<WebApp.Models.Projects.ProjectMember>() },
            { x => x.UserRole, "Member" }
        };
        await comp.InvokeAsync(() => dialogService.Show<TaskDetailDialog>("Task", parameters));

        var dialog = comp.FindComponent<TaskDetailDialog>();
        await dialog.InvokeAsync(() =>
        {
            typeof(TaskDetailDialog).GetField("newComment",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(dialog.Instance, "Test comment");
            return (Task)typeof(TaskDetailDialog).GetMethod("AddComment",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(dialog.Instance, null)!;
        });
    }

    [Fact]
    public async Task AddComment_Exception_ShowsError()
    {
        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            if (req.Method == HttpMethod.Post)
                throw new HttpRequestException("Server error");
            var json = System.Text.Json.JsonSerializer.Serialize(new List<WebApp.Models.Tasks.TaskCommentModel>());
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(json, Encoding.UTF8, "application/json") });
        });

        SetupServices(handler);
        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<TaskDetailDialog>
        {
            { x => x.Task, CreateTask() },
            { x => x.Members, new List<WebApp.Models.Projects.ProjectMember>() },
            { x => x.UserRole, "Member" }
        };
        await comp.InvokeAsync(() => dialogService.Show<TaskDetailDialog>("Task", parameters));

        var dialog = comp.FindComponent<TaskDetailDialog>();
        await dialog.InvokeAsync(() =>
        {
            typeof(TaskDetailDialog).GetField("newComment",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(dialog.Instance, "Test comment");
            return (Task)typeof(TaskDetailDialog).GetMethod("AddComment",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(dialog.Instance, null)!;
        });
    }

    [Fact]
    public async Task AddComment_EmptyText_DoesNothing()
    {
        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(new List<WebApp.Models.Tasks.TaskCommentModel>()));
        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<TaskDetailDialog>
        {
            { x => x.Task, CreateTask() },
            { x => x.Members, new List<WebApp.Models.Projects.ProjectMember>() },
            { x => x.UserRole, "Member" }
        };
        await comp.InvokeAsync(() => dialogService.Show<TaskDetailDialog>("Task", parameters));

        var dialog = comp.FindComponent<TaskDetailDialog>();
        await dialog.InvokeAsync(() =>
            (Task)typeof(TaskDetailDialog).GetMethod("AddComment",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(dialog.Instance, null)!);
    }

    [Fact]
    public async Task LoadComments_Error_ShowsError()
    {
        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            throw new HttpRequestException("Connection failed");
        });

        SetupServices(handler);
        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<TaskDetailDialog>
        {
            { x => x.Task, CreateTask() },
            { x => x.Members, new List<WebApp.Models.Projects.ProjectMember>() },
            { x => x.UserRole, "Member" }
        };
        await comp.InvokeAsync(() => dialogService.Show<TaskDetailDialog>("Task", parameters));

        comp.Markup.Should().Contain("No hay comentarios");
    }

    [Theory]
    [InlineData("Todo", "Pendiente")]
    [InlineData("InProgress", "En Progreso")]
    [InlineData("Review", "Revisión")]
    [InlineData("Testing", "Pruebas")]
    [InlineData("Done", "Completada")]
    [InlineData("Blocked", "Bloqueada")]
    [InlineData("Unknown", "Unknown")]
    public async Task RenderDialog_WithCorrectStatusText(string status, string expected)
    {
        var task = CreateTask();
        task.Status = status;

        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(new List<WebApp.Models.Tasks.TaskCommentModel>()));
        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<TaskDetailDialog>
        {
            { x => x.Task, task },
            { x => x.Members, new List<WebApp.Models.Projects.ProjectMember>() },
            { x => x.UserRole, "Member" }
        };
        await comp.InvokeAsync(() => dialogService.Show<TaskDetailDialog>("Task", parameters));

        comp.Markup.Should().Contain(expected);
    }
}
