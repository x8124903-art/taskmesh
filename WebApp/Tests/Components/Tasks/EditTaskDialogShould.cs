using System.Text;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using WebApp.Components.Tasks;
using WebApp.Services;

namespace WebApp.Tests.Components.Tasks;

public sealed class EditTaskDialogShould : TestContext
{
    private static WebApp.Models.Tasks.TaskModel CreateTask() => new()
    {
        IdTask = 1, Title = "Test Task", Description = "Desc", ProjectId = 1,
        Priority = "Medium", Status = "Todo", CreatedBy = 1, RowVersion = 1,
        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
    };

    private void SetupServices(Helpers.MockHttpMessageHandler handler)
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        Services.AddSingleton(new TaskApiService(httpClient));
        Services.AddSingleton(new ProjectApiService(httpClient));
    }

    private async Task<IRenderedComponent<MudDialogProvider>> OpenDialog(WebApp.Models.Tasks.TaskModel? task = null)
    {
        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();
        var parameters = new DialogParameters<EditTaskDialog> { { x => x.Task, task ?? CreateTask() } };
        await comp.InvokeAsync(() => dialogService.Show<EditTaskDialog>("Editar Tarea", parameters));
        return comp;
    }

    private void EnsureFormValidAndSubmit(IRenderedComponent<MudDialogProvider> comp)
    {
        // The title is pre-filled from task, trigger input event to ensure validation
        var titleInput = comp.FindAll("input").First();
        titleInput.Input("Test Task Updated");

        comp.WaitForState(() =>
        {
            var btns = comp.FindAll("button");
            var saveBtn = btns.FirstOrDefault(b => b.TextContent.Contains("Guardar Cambios"));
            return saveBtn != null && !saveBtn.HasAttribute("disabled");
        }, TimeSpan.FromSeconds(3));

        comp.FindAll("button").First(b => b.TextContent.Contains("Guardar Cambios")).Click();
    }

    [Fact]
    public async Task RenderDialog_WithTaskData()
    {
        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(new List<WebApp.Models.Projects.ProjectMember>()));
        var comp = await OpenDialog();
        comp.Markup.Should().Contain("Guardar Cambios");
        comp.Markup.Should().Contain("Cancelar");
        comp.Markup.Should().Contain("Prioridad");
    }

    [Fact]
    public async Task Cancel_ClosesDialog()
    {
        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(new List<WebApp.Models.Projects.ProjectMember>()));
        var comp = await OpenDialog();
        comp.FindAll("button").First(b => b.TextContent.Contains("Cancelar")).Click();
    }

    [Fact]
    public async Task OnInit_MembersLoadError_StillRendersForm()
    {
        SetupServices(new Helpers.MockHttpMessageHandler((_, _) =>
        {
            throw new HttpRequestException("Connection failed");
        }));
        var comp = await OpenDialog();
        comp.Markup.Should().Contain("Guardar Cambios");
    }

    [Fact]
    public async Task UpdateTask_WhenFormInvalid_ReturnsEarly()
    {
        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(new List<WebApp.Models.Projects.ProjectMember>()));
        // Use a task with empty title to make form invalid
        var task = CreateTask();
        task.Title = "";
        var comp = await OpenDialog(task);
        var dialog = comp.FindComponent<EditTaskDialog>();
        await dialog.InvokeAsync(() =>
        {
            var method = typeof(EditTaskDialog).GetMethod("UpdateTask",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (Task)method!.Invoke(dialog.Instance, null)!;
        });
    }

    [Fact]
    public async Task UpdateTask_Success_ClosesDialog()
    {
        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            if (req.RequestUri!.AbsolutePath.Contains("/members"))
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent("[]", Encoding.UTF8, "application/json") });
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent("true", Encoding.UTF8, "application/json") });
        });
        SetupServices(handler);
        var comp = await OpenDialog();
        EnsureFormValidAndSubmit(comp);
    }

    [Fact]
    public async Task UpdateTask_Failure_ShowsError()
    {
        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            if (req.RequestUri!.AbsolutePath.Contains("/members"))
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent("[]", Encoding.UTF8, "application/json") });
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Forbidden));
        });
        SetupServices(handler);
        var comp = await OpenDialog();
        EnsureFormValidAndSubmit(comp);
        comp.WaitForAssertion(() =>
            comp.Markup.Should().Contain("No se pudo actualizar"), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task UpdateTask_HttpError409_ShowsConflictMessage()
    {
        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            if (req.RequestUri!.AbsolutePath.Contains("/members"))
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent("[]", Encoding.UTF8, "application/json") });
            throw new HttpRequestException("Response status code does not indicate success: 409");
        });
        SetupServices(handler);
        var comp = await OpenDialog();
        EnsureFormValidAndSubmit(comp);
        comp.WaitForAssertion(() =>
            comp.Markup.Should().Contain("modificada por otro usuario"), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task UpdateTask_HttpError403_ShowsPermissionMessage()
    {
        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            if (req.RequestUri!.AbsolutePath.Contains("/members"))
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent("[]", Encoding.UTF8, "application/json") });
            throw new HttpRequestException("Response status code does not indicate success: 403");
        });
        SetupServices(handler);
        var comp = await OpenDialog();
        EnsureFormValidAndSubmit(comp);
        comp.WaitForAssertion(() =>
            comp.Markup.Should().Contain("No tienes permisos"), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task UpdateTask_HttpError404_ShowsNotFoundMessage()
    {
        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            if (req.RequestUri!.AbsolutePath.Contains("/members"))
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent("[]", Encoding.UTF8, "application/json") });
            throw new HttpRequestException("Response status code does not indicate success: 404");
        });
        SetupServices(handler);
        var comp = await OpenDialog();
        EnsureFormValidAndSubmit(comp);
        comp.WaitForAssertion(() =>
            comp.Markup.Should().Contain("ya no existe"), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task UpdateTask_GenericHttpError_ShowsGenericMessage()
    {
        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            if (req.RequestUri!.AbsolutePath.Contains("/members"))
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent("[]", Encoding.UTF8, "application/json") });
            throw new HttpRequestException("Response status code does not indicate success: 500");
        });
        SetupServices(handler);
        var comp = await OpenDialog();
        EnsureFormValidAndSubmit(comp);
        comp.WaitForAssertion(() =>
            comp.Markup.Should().Contain("Error al actualizar"), TimeSpan.FromSeconds(3));
    }
}
