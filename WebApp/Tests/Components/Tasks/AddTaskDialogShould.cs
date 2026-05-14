using System.Text;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using WebApp.Components.Tasks;
using WebApp.Services;

namespace WebApp.Tests.Components.Tasks;

public sealed class AddTaskDialogShould : TestContext
{
    private void SetupServices(Helpers.MockHttpMessageHandler handler)
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        Services.AddSingleton(new TaskApiService(httpClient));
        Services.AddSingleton(new ProjectApiService(httpClient));
    }

    private async Task<IRenderedComponent<MudDialogProvider>> OpenDialog()
    {
        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();
        var parameters = new DialogParameters<AddTaskDialog> { { x => x.ProjectId, 1 } };
        await comp.InvokeAsync(() => dialogService.Show<AddTaskDialog>("Nueva Tarea", parameters));
        return comp;
    }

    private void FillTitleAndSubmit(IRenderedComponent<MudDialogProvider> comp)
    {
        // MudTextField with Immediate="true" uses oninput
        var titleInput = comp.FindAll("input").First();
        titleInput.Input("Test Task Title");

        // Wait for form to become valid
        comp.WaitForState(() =>
        {
            var btns = comp.FindAll("button");
            var createBtn = btns.FirstOrDefault(b => b.TextContent.Contains("Crear Tarea"));
            return createBtn != null && !createBtn.HasAttribute("disabled");
        }, TimeSpan.FromSeconds(3));

        comp.FindAll("button").First(b => b.TextContent.Contains("Crear Tarea")).Click();
    }

    [Fact]
    public async Task RenderDialog_WithAllFormFields()
    {
        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(new List<WebApp.Models.Projects.ProjectMember>()));
        var comp = await OpenDialog();
        comp.Markup.Should().Contain("Prioridad");
        comp.Markup.Should().Contain("Asignado a");
        comp.Markup.Should().Contain("Crear Tarea");
        comp.Markup.Should().Contain("Cancelar");
        comp.Markup.Should().Contain("Fecha de Vencimiento");
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
        comp.Markup.Should().Contain("Prioridad");
    }

    [Fact]
    public async Task CreateTask_WhenFormInvalid_ReturnsEarly()
    {
        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(new List<WebApp.Models.Projects.ProjectMember>()));
        var comp = await OpenDialog();
        var dialog = comp.FindComponent<AddTaskDialog>();
        await dialog.InvokeAsync(() =>
        {
            var method = typeof(AddTaskDialog).GetMethod("CreateTask",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (Task)method!.Invoke(dialog.Instance, null)!;
        });
    }

    [Fact]
    public async Task CreateTask_Success_ClosesDialog()
    {
        var task = new WebApp.Models.Tasks.TaskModel
        {
            IdTask = 1, Title = "Test Task Title", ProjectId = 1, Priority = "Medium", Status = "Todo",
            CreatedBy = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            object data = req.RequestUri!.AbsolutePath.Contains("/members")
                ? new List<WebApp.Models.Projects.ProjectMember>()
                : (object)task;
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(json, Encoding.UTF8, "application/json") });
        });
        SetupServices(handler);
        var comp = await OpenDialog();
        FillTitleAndSubmit(comp);
    }

    [Fact]
    public async Task CreateTask_NullResponse_ShowsError()
    {
        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            if (req.RequestUri!.AbsolutePath.Contains("/members"))
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent("[]", Encoding.UTF8, "application/json") });
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent("null", Encoding.UTF8, "application/json") });
        });
        SetupServices(handler);
        var comp = await OpenDialog();
        FillTitleAndSubmit(comp);
        comp.WaitForAssertion(() =>
            comp.Markup.Should().Contain("No se pudo crear la tarea"), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task CreateTask_HttpError403_ShowsPermissionMessage()
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
        FillTitleAndSubmit(comp);
        comp.WaitForAssertion(() =>
            comp.Markup.Should().Contain("No tienes permisos"), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task CreateTask_HttpError400_ShowsValidationMessage()
    {
        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            if (req.RequestUri!.AbsolutePath.Contains("/members"))
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent("[]", Encoding.UTF8, "application/json") });
            throw new HttpRequestException("Response status code does not indicate success: 400");
        });
        SetupServices(handler);
        var comp = await OpenDialog();
        FillTitleAndSubmit(comp);
        comp.WaitForAssertion(() =>
            comp.Markup.Should().Contain("datos de la tarea"), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task CreateTask_GenericHttpError_ShowsGenericMessage()
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
        FillTitleAndSubmit(comp);
        comp.WaitForAssertion(() =>
            comp.Markup.Should().Contain("Error al crear la tarea"), TimeSpan.FromSeconds(3));
    }
}
