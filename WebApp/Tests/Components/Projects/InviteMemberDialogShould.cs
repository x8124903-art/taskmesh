using System.Text;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MudBlazor;
using MudBlazor.Services;
using WebApp.Components.Projects;
using WebApp.Services;

namespace WebApp.Tests.Components.Projects;

public sealed class InviteMemberDialogShould : TestContext
{
    private void SetupServices(Helpers.MockHttpMessageHandler handler)
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        Services.AddSingleton(new ProjectApiService(httpClient));
        Services.AddSingleton(new ProjectInvitationApiService(httpClient, NullLogger<ProjectInvitationApiService>.Instance));
    }

    private async Task<IRenderedComponent<MudDialogProvider>> OpenDialog()
    {
        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();
        var parameters = new DialogParameters<InviteMemberDialog> { { x => x.ProjectId, 1 } };
        await comp.InvokeAsync(() => dialogService.Show<InviteMemberDialog>("Invitar Miembro", parameters));
        return comp;
    }

    private void FillEmailAndSubmit(IRenderedComponent<MudDialogProvider> comp)
    {
        var emailInput = comp.Find("input[type='email']");
        emailInput.Input("invite@test.com");

        comp.WaitForState(() =>
        {
            var btns = comp.FindAll("button");
            var sendBtn = btns.FirstOrDefault(b => b.TextContent.Contains("Enviar"));
            return sendBtn != null && !sendBtn.HasAttribute("disabled");
        }, TimeSpan.FromSeconds(3));

        comp.FindAll("button").First(b => b.TextContent.Contains("Enviar")).Click();
    }

    [Fact]
    public async Task RenderDialog_WithForm()
    {
        SetupServices(Helpers.MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK));
        var comp = await OpenDialog();
        comp.Markup.Should().Contain("Email del Usuario");
        comp.Markup.Should().Contain("Rol en el Proyecto");
        comp.Markup.Should().Contain("Owner");
        comp.Markup.Should().Contain("Admin");
        comp.Markup.Should().Contain("Member");
        comp.Markup.Should().Contain("Viewer");
    }

    [Fact]
    public async Task Cancel_ClosesDialog()
    {
        SetupServices(Helpers.MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK));
        var comp = await OpenDialog();
        comp.FindAll("button").First(b => b.TextContent.Contains("Cancelar")).Click();
    }

    [Fact]
    public async Task SendInvitation_WhenFormInvalid_ReturnsEarly()
    {
        SetupServices(Helpers.MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK));
        var comp = await OpenDialog();
        var dialog = comp.FindComponent<InviteMemberDialog>();
        await dialog.InvokeAsync(() =>
        {
            var method = typeof(InviteMemberDialog).GetMethod("SendInvitation",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (Task)method!.Invoke(dialog.Instance, null)!;
        });
    }

    [Fact]
    public async Task SendInvitation_Success_ClosesDialog()
    {
        var invitation = new WebApp.Models.Projects.ProjectInvitation
        {
            IdProjectInvitation = 1, ProjectId = 1, ProjectName = "Test",
            Email = "invite@test.com", RoleName = "Member", Token = "tok",
            Status = "Pending", InvitedByName = "Admin",
            InvitedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        SetupServices(Helpers.MockHttpMessageHandler.WithJsonResponse(invitation));
        var comp = await OpenDialog();
        FillEmailAndSubmit(comp);
    }

    [Fact]
    public async Task SendInvitation_NullResponse_ShowsError()
    {
        SetupServices(new Helpers.MockHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent("null", Encoding.UTF8, "application/json") })));
        var comp = await OpenDialog();
        FillEmailAndSubmit(comp);
        comp.WaitForAssertion(() =>
            comp.Markup.Should().Contain("No se pudo crear la invitaci"), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task SendInvitation_HttpError409_ShowsConflictMessage()
    {
        SetupServices(new Helpers.MockHttpMessageHandler((_, _) =>
        {
            throw new HttpRequestException("Response status code does not indicate success: 409");
        }));
        var comp = await OpenDialog();
        FillEmailAndSubmit(comp);
        comp.WaitForAssertion(() =>
            comp.Markup.Should().Contain("ya tiene una invitaci"), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task SendInvitation_HttpError403_ShowsPermissionMessage()
    {
        SetupServices(new Helpers.MockHttpMessageHandler((_, _) =>
        {
            throw new HttpRequestException("Response status code does not indicate success: 403");
        }));
        var comp = await OpenDialog();
        FillEmailAndSubmit(comp);
        comp.WaitForAssertion(() =>
            comp.Markup.Should().Contain("No tienes permisos"), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task SendInvitation_HttpError400_ShowsValidationMessage()
    {
        SetupServices(new Helpers.MockHttpMessageHandler((_, _) =>
        {
            throw new HttpRequestException("Response status code does not indicate success: 400");
        }));
        var comp = await OpenDialog();
        FillEmailAndSubmit(comp);
        comp.WaitForAssertion(() =>
            comp.Markup.Should().Contain("datos de la invitaci"), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task SendInvitation_GenericHttpError_ShowsMessage()
    {
        SetupServices(new Helpers.MockHttpMessageHandler((_, _) =>
        {
            throw new HttpRequestException("Response status code does not indicate success: 500");
        }));
        var comp = await OpenDialog();
        FillEmailAndSubmit(comp);
        comp.WaitForAssertion(() =>
            comp.Markup.Should().Contain("Error al enviar la invitaci"), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task SendInvitation_GenericException_ShowsErrorMessage()
    {
        SetupServices(new Helpers.MockHttpMessageHandler((_, _) =>
        {
            throw new InvalidOperationException("Unexpected error");
        }));
        var comp = await OpenDialog();
        FillEmailAndSubmit(comp);
        comp.WaitForAssertion(() =>
            comp.Markup.Should().Contain("Error inesperado"), TimeSpan.FromSeconds(3));
    }
}
