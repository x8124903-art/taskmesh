using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using WebApp.Components.Tasks;

namespace WebApp.Tests.Components.Tasks;

public sealed class TaskCardShould : TestContext
{
    public TaskCardShould()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void RenderCard_WithTitle()
    {
        var task = new WebApp.Models.Tasks.TaskModel
        {
            IdTask = 1, Title = "My Task", Priority = "Medium", Status = "Todo",
            CreatedBy = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };

        var comp = RenderComponent<TaskCard>(p => p
            .Add(x => x.Task, task)
            .Add(x => x.Members, new List<WebApp.Models.Projects.ProjectMember>()));

        comp.Markup.Should().Contain("My Task");
        comp.Markup.Should().Contain("Sin asignar");
    }

    [Theory]
    [InlineData("High", "Alta")]
    [InlineData("Medium", "Media")]
    [InlineData("Low", "Baja")]
    [InlineData("Unknown", "Unknown")]
    public void RenderCard_WithCorrectPriorityText(string priority, string expected)
    {
        var task = new WebApp.Models.Tasks.TaskModel
        {
            IdTask = 1, Title = "Task", Priority = priority, Status = "Todo",
            CreatedBy = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };

        var comp = RenderComponent<TaskCard>(p => p
            .Add(x => x.Task, task)
            .Add(x => x.Members, new List<WebApp.Models.Projects.ProjectMember>()));

        comp.Markup.Should().Contain(expected);
    }

    [Fact]
    public void RenderCard_WithAssignedUser()
    {
        var task = new WebApp.Models.Tasks.TaskModel
        {
            IdTask = 1, Title = "Task", Priority = "Medium", Status = "Todo",
            AssignedToUserId = 5, CreatedBy = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };

        var members = new List<WebApp.Models.Projects.ProjectMember>
        {
            new() { UserId = 5, UserName = "Alice" }
        };

        var comp = RenderComponent<TaskCard>(p => p
            .Add(x => x.Task, task)
            .Add(x => x.Members, members));

        comp.Markup.Should().Contain("Alice");
    }

    [Fact]
    public void RenderCard_WithAssignedUser_NotInMembers()
    {
        var task = new WebApp.Models.Tasks.TaskModel
        {
            IdTask = 1, Title = "Task", Priority = "Medium", Status = "Todo",
            AssignedToUserId = 99, CreatedBy = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };

        var comp = RenderComponent<TaskCard>(p => p
            .Add(x => x.Task, task)
            .Add(x => x.Members, new List<WebApp.Models.Projects.ProjectMember>()));

        comp.Markup.Should().Contain("Usuario #99");
    }

    [Fact]
    public void Click_InvokesCallback()
    {
        var task = new WebApp.Models.Tasks.TaskModel
        {
            IdTask = 1, Title = "Task", Priority = "Medium", Status = "Todo",
            CreatedBy = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };

        WebApp.Models.Tasks.TaskModel? clicked = null;
        var comp = RenderComponent<TaskCard>(p => p
            .Add(x => x.Task, task)
            .Add(x => x.Members, new List<WebApp.Models.Projects.ProjectMember>())
            .Add(x => x.OnClick, t => { clicked = t; }));

        comp.Find(".mud-card").Click();
        clicked.Should().NotBeNull();
        clicked!.IdTask.Should().Be(1);
    }
}
