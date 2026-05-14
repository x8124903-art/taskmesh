using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using WebApp.Components.Tasks;

namespace WebApp.Tests.Components.Tasks;

public sealed class BoardColumnShould : TestContext
{
    public BoardColumnShould()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void RenderColumn_WithTitle()
    {
        var comp = RenderComponent<BoardColumn>(p => p
            .Add(x => x.Title, "PENDIENTE")
            .Add(x => x.Status, "Todo")
            .Add(x => x.Tasks, new List<WebApp.Models.Tasks.TaskModel>())
            .Add(x => x.UserRole, "Member")
            .Add(x => x.Members, new List<WebApp.Models.Projects.ProjectMember>()));

        comp.Markup.Should().Contain("PENDIENTE");
        comp.Markup.Should().Contain("No hay tareas");
    }

    [Fact]
    public void RenderColumn_WithTasks()
    {
        var tasks = new List<WebApp.Models.Tasks.TaskModel>
        {
            new() { IdTask = 1, Title = "Task 1", Priority = "High", Status = "Todo", CreatedBy = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { IdTask = 2, Title = "Task 2", Priority = "Low", Status = "Todo", CreatedBy = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        var comp = RenderComponent<BoardColumn>(p => p
            .Add(x => x.Title, "PENDIENTE")
            .Add(x => x.Status, "Todo")
            .Add(x => x.Tasks, tasks)
            .Add(x => x.UserRole, "Member")
            .Add(x => x.Members, new List<WebApp.Models.Projects.ProjectMember>()));

        comp.Markup.Should().Contain("Task 1");
        comp.Markup.Should().Contain("Task 2");
        comp.Markup.Should().Contain("Mover a...");
    }

    [Fact]
    public void RenderColumn_ViewerRole_HidesMoveButton()
    {
        var tasks = new List<WebApp.Models.Tasks.TaskModel>
        {
            new() { IdTask = 1, Title = "Task 1", Priority = "High", Status = "Todo", CreatedBy = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        var comp = RenderComponent<BoardColumn>(p => p
            .Add(x => x.Title, "PENDIENTE")
            .Add(x => x.Status, "Todo")
            .Add(x => x.Tasks, tasks)
            .Add(x => x.UserRole, "Viewer")
            .Add(x => x.Members, new List<WebApp.Models.Projects.ProjectMember>()));

        comp.Markup.Should().Contain("Task 1");
        comp.Markup.Should().NotContain("Mover a...");
    }

    [Fact]
    public void RenderColumn_ShowsTaskCount()
    {
        var tasks = new List<WebApp.Models.Tasks.TaskModel>
        {
            new() { IdTask = 1, Title = "Task 1", Priority = "High", Status = "Todo", CreatedBy = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { IdTask = 2, Title = "Task 2", Priority = "Low", Status = "Todo", CreatedBy = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { IdTask = 3, Title = "Task 3", Priority = "Medium", Status = "Todo", CreatedBy = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        var comp = RenderComponent<BoardColumn>(p => p
            .Add(x => x.Title, "PENDIENTE")
            .Add(x => x.Status, "Todo")
            .Add(x => x.Tasks, tasks)
            .Add(x => x.UserRole, "Member")
            .Add(x => x.Members, new List<WebApp.Models.Projects.ProjectMember>()));

        comp.Markup.Should().Contain("3");
    }
}
