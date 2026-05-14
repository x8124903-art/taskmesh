using System.Text;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using WebApp.Components.Tasks;
using WebApp.Services;

namespace WebApp.Tests.Components.Tasks;

public sealed class ProjectBacklogShould : TestContext
{
    private void SetupServices(Helpers.MockHttpMessageHandler handler)
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        Services.AddSingleton(new TaskApiService(httpClient));
    }

    [Fact]
    public void RenderBacklog_WithTasks()
    {
        var tasks = new List<WebApp.Models.Tasks.TaskModel>
        {
            new() { IdTask = 1, Title = "Task 1", Description = "Desc", Priority = "High", Status = "Todo",
                     AssignedToUserId = 1, CreatedBy = 1, DueDate = DateTime.UtcNow.AddDays(5),
                     CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { IdTask = 2, Title = "Task 2", Priority = "Low", Status = "Done",
                     CreatedBy = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(tasks));

        var members = new List<WebApp.Models.Projects.ProjectMember>
        {
            new() { UserId = 1, UserName = "Alice" }
        };

        var comp = RenderComponent<ProjectBacklog>(p => p
            .Add(x => x.ProjectId, 1)
            .Add(x => x.Members, members)
            .Add(x => x.UserRole, "Member"));

        comp.Markup.Should().Contain("Task 1");
        comp.Markup.Should().Contain("Task 2");
        comp.Markup.Should().Contain("Nueva Tarea");
    }

    [Fact]
    public void RenderBacklog_ViewerRole_HidesNewTaskButton()
    {
        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(new List<WebApp.Models.Tasks.TaskModel>()));

        var comp = RenderComponent<ProjectBacklog>(p => p
            .Add(x => x.ProjectId, 1)
            .Add(x => x.Members, new List<WebApp.Models.Projects.ProjectMember>())
            .Add(x => x.UserRole, "Viewer"));

        comp.Markup.Should().NotContain("Nueva Tarea");
    }

    [Fact]
    public void RenderBacklog_Empty_ShowsNoRecords()
    {
        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(new List<WebApp.Models.Tasks.TaskModel>()));

        var comp = RenderComponent<ProjectBacklog>(p => p
            .Add(x => x.ProjectId, 1)
            .Add(x => x.Members, new List<WebApp.Models.Projects.ProjectMember>())
            .Add(x => x.UserRole, "Member"));

        comp.Markup.Should().Contain("No hay tareas que mostrar");
    }

    [Fact]
    public void RenderBacklog_LoadError_ShowsEmptyTable()
    {
        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            throw new HttpRequestException("Connection failed");
        });

        SetupServices(handler);

        var comp = RenderComponent<ProjectBacklog>(p => p
            .Add(x => x.ProjectId, 1)
            .Add(x => x.Members, new List<WebApp.Models.Projects.ProjectMember>())
            .Add(x => x.UserRole, "Member"));

        comp.Markup.Should().Contain("No hay tareas que mostrar");
    }

    [Fact]
    public void RenderBacklog_TaskWithLongDescription_Truncates()
    {
        var longDesc = new string('A', 100);
        var tasks = new List<WebApp.Models.Tasks.TaskModel>
        {
            new() { IdTask = 1, Title = "Task", Description = longDesc, Priority = "Medium", Status = "Todo",
                     CreatedBy = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(tasks));

        var comp = RenderComponent<ProjectBacklog>(p => p
            .Add(x => x.ProjectId, 1)
            .Add(x => x.Members, new List<WebApp.Models.Projects.ProjectMember>())
            .Add(x => x.UserRole, "Member"));

        comp.Markup.Should().Contain("...");
    }

    [Fact]
    public void RenderBacklog_TaskWithUnassigned_ShowsSinAsignar()
    {
        var tasks = new List<WebApp.Models.Tasks.TaskModel>
        {
            new() { IdTask = 1, Title = "Task", Priority = "Medium", Status = "Todo",
                     CreatedBy = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(tasks));

        var comp = RenderComponent<ProjectBacklog>(p => p
            .Add(x => x.ProjectId, 1)
            .Add(x => x.Members, new List<WebApp.Models.Projects.ProjectMember>())
            .Add(x => x.UserRole, "Member"));

        comp.Markup.Should().Contain("Sin asignar");
    }

    [Fact]
    public void RenderBacklog_TaskWithPastDueDate_ShowsOverdue()
    {
        var tasks = new List<WebApp.Models.Tasks.TaskModel>
        {
            new() { IdTask = 1, Title = "Task", Priority = "Medium", Status = "Todo",
                     DueDate = DateTime.Now.AddDays(-2),
                     CreatedBy = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(tasks));

        var comp = RenderComponent<ProjectBacklog>(p => p
            .Add(x => x.ProjectId, 1)
            .Add(x => x.Members, new List<WebApp.Models.Projects.ProjectMember>())
            .Add(x => x.UserRole, "Member"));

        comp.Markup.Should().Contain(DateTime.Now.AddDays(-2).ToString("dd/MM/yyyy"));
    }

    [Fact]
    public void RenderBacklog_TaskWithNoDueDate_ShowsDash()
    {
        var tasks = new List<WebApp.Models.Tasks.TaskModel>
        {
            new() { IdTask = 1, Title = "Task", Priority = "Medium", Status = "Todo",
                     CreatedBy = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(tasks));

        var comp = RenderComponent<ProjectBacklog>(p => p
            .Add(x => x.ProjectId, 1)
            .Add(x => x.Members, new List<WebApp.Models.Projects.ProjectMember>())
            .Add(x => x.UserRole, "Member"));

        comp.Markup.Should().Contain("-");
    }

    [Fact]
    public void RenderBacklog_ViewerRole_HidesActionButtons()
    {
        var tasks = new List<WebApp.Models.Tasks.TaskModel>
        {
            new() { IdTask = 1, Title = "Task", Priority = "Medium", Status = "Todo",
                     CreatedBy = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(tasks));

        var comp = RenderComponent<ProjectBacklog>(p => p
            .Add(x => x.ProjectId, 1)
            .Add(x => x.Members, new List<WebApp.Models.Projects.ProjectMember>())
            .Add(x => x.UserRole, "Viewer"));

        comp.Markup.Should().NotContain("Editar");
        comp.Markup.Should().NotContain("Eliminar");
    }
}
