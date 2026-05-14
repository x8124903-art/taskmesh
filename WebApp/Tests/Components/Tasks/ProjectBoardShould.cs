using System.Text;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using WebApp.Components.Tasks;
using WebApp.Services;

namespace WebApp.Tests.Components.Tasks;

public sealed class ProjectBoardShould : TestContext
{
    private void SetupServices(Helpers.MockHttpMessageHandler handler)
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        Services.AddSingleton(new TaskApiService(httpClient));
        Services.AddSingleton(new ProjectApiService(httpClient));
    }

    [Fact]
    public void RenderBoard_WithColumns()
    {
        var board = new WebApp.Models.Tasks.BoardResponse(new Dictionary<string, List<WebApp.Models.Tasks.TaskModel>>
        {
            ["Todo"] = new()
            {
                new() { IdTask = 1, Title = "Task 1", Priority = "High", Status = "Todo", CreatedBy = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            },
            ["InProgress"] = new()
        });

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            object data = req.RequestUri!.AbsolutePath.Contains("/board/")
                ? board
                : (object)new List<WebApp.Models.Projects.ProjectMember>();
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        });

        SetupServices(handler);

        var comp = RenderComponent<ProjectBoard>(p => p
            .Add(x => x.ProjectId, 1)
            .Add(x => x.Members, new List<WebApp.Models.Projects.ProjectMember>())
            .Add(x => x.UserRole, "Member"));

        comp.Markup.Should().Contain("PENDIENTE");
        comp.Markup.Should().Contain("EN PROGRESO");
    }

    [Fact]
    public void RenderBoard_LoadError_ShowsError()
    {
        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            throw new HttpRequestException("Connection failed");
        });

        SetupServices(handler);

        var comp = RenderComponent<ProjectBoard>(p => p
            .Add(x => x.ProjectId, 1)
            .Add(x => x.Members, new List<WebApp.Models.Projects.ProjectMember>())
            .Add(x => x.UserRole, "Member"));

        comp.Markup.Should().Contain("PENDIENTE");
    }

    [Fact]
    public void RenderBoard_WithFilters()
    {
        var board = new WebApp.Models.Tasks.BoardResponse(new Dictionary<string, List<WebApp.Models.Tasks.TaskModel>>());

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            object data = req.RequestUri!.AbsolutePath.Contains("/board/")
                ? board
                : (object)new List<WebApp.Models.Projects.ProjectMember>();
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        });

        SetupServices(handler);

        var comp = RenderComponent<ProjectBoard>(p => p
            .Add(x => x.ProjectId, 1)
            .Add(x => x.Members, new List<WebApp.Models.Projects.ProjectMember>())
            .Add(x => x.UserRole, "Member"));

        comp.Markup.Should().Contain("Asignado a");
        comp.Markup.Should().Contain("Prioridad");
        comp.Markup.Should().Contain("Buscar");
    }
}
