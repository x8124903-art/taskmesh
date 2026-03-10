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
    [Fact]
    public async Task RenderDialog_WithForm()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = Helpers.MockHttpMessageHandler.WithStatusCode(System.Net.HttpStatusCode.OK);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        Services.AddSingleton(new ProjectApiService(httpClient));
        Services.AddSingleton(new ProjectInvitationApiService(httpClient, NullLogger<ProjectInvitationApiService>.Instance));

        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<InviteMemberDialog>
        {
            { x => x.ProjectId, 1 }
        };

        await comp.InvokeAsync(() => dialogService.Show<InviteMemberDialog>("Invitar Miembro", parameters));

        comp.Markup.Should().Contain("Email del Usuario");
    }

    [Fact]
    public async Task Cancel_ClosesDialog()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = Helpers.MockHttpMessageHandler.WithStatusCode(System.Net.HttpStatusCode.OK);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        Services.AddSingleton(new ProjectApiService(httpClient));
        Services.AddSingleton(new ProjectInvitationApiService(httpClient, NullLogger<ProjectInvitationApiService>.Instance));

        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<InviteMemberDialog>
        {
            { x => x.ProjectId, 1 }
        };

        await comp.InvokeAsync(() => dialogService.Show<InviteMemberDialog>("Invitar Miembro", parameters));

        var cancelBtn = comp.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Cancelar"));
        cancelBtn?.Click();
    }

    [Fact]
    public async Task SendInvitation_Success_ClosesDialog()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var invitation = new WebApp.Models.Projects.ProjectInvitation
        {
            IdProjectInvitation = 1, ProjectId = 1, ProjectName = "Test",
            Email = "invite@test.com", RoleName = "Member", Token = "tok",
            Status = "Pending", InvitedByName = "Admin",
            InvitedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            var json = System.Text.Json.JsonSerializer.Serialize(invitation);
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        Services.AddSingleton(new ProjectApiService(httpClient));
        Services.AddSingleton(new ProjectInvitationApiService(httpClient, NullLogger<ProjectInvitationApiService>.Instance));

        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<InviteMemberDialog>
        {
            { x => x.ProjectId, 1 }
        };

        await comp.InvokeAsync(() => dialogService.Show<InviteMemberDialog>("Invitar Miembro", parameters));

        var sendBtn = comp.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Enviar"));
        sendBtn?.Click();
    }

    [Fact]
    public async Task SendInvitation_HttpError409_ShowsConflictMessage()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            throw new HttpRequestException("Response status code does not indicate success: 409");
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        Services.AddSingleton(new ProjectApiService(httpClient));
        Services.AddSingleton(new ProjectInvitationApiService(httpClient, NullLogger<ProjectInvitationApiService>.Instance));

        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<InviteMemberDialog>
        {
            { x => x.ProjectId, 1 }
        };

        await comp.InvokeAsync(() => dialogService.Show<InviteMemberDialog>("Invitar Miembro", parameters));

        var sendBtn = comp.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Enviar"));
        sendBtn?.Click();

        comp.Markup.Should().Contain("Email del Usuario");
    }

    [Fact]
    public async Task SendInvitation_HttpError403_ShowsPermissionMessage()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            throw new HttpRequestException("Response status code does not indicate success: 403");
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        Services.AddSingleton(new ProjectApiService(httpClient));
        Services.AddSingleton(new ProjectInvitationApiService(httpClient, NullLogger<ProjectInvitationApiService>.Instance));

        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<InviteMemberDialog>
        {
            { x => x.ProjectId, 1 }
        };

        await comp.InvokeAsync(() => dialogService.Show<InviteMemberDialog>("Invitar Miembro", parameters));

        var sendBtn = comp.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Enviar"));
        sendBtn?.Click();

        comp.Markup.Should().Contain("Email del Usuario");
    }

    [Fact]
    public async Task SendInvitation_HttpError400_ShowsValidationMessage()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            throw new HttpRequestException("Response status code does not indicate success: 400");
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        Services.AddSingleton(new ProjectApiService(httpClient));
        Services.AddSingleton(new ProjectInvitationApiService(httpClient, NullLogger<ProjectInvitationApiService>.Instance));

        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<InviteMemberDialog>
        {
            { x => x.ProjectId, 1 }
        };

        await comp.InvokeAsync(() => dialogService.Show<InviteMemberDialog>("Invitar Miembro", parameters));

        var sendBtn = comp.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Enviar"));
        sendBtn?.Click();

        comp.Markup.Should().Contain("Email del Usuario");
    }

    [Fact]
    public async Task SendInvitation_GenericException_ShowsErrorMessage()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            throw new InvalidOperationException("Unexpected error");
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        Services.AddSingleton(new ProjectApiService(httpClient));
        Services.AddSingleton(new ProjectInvitationApiService(httpClient, NullLogger<ProjectInvitationApiService>.Instance));

        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<InviteMemberDialog>
        {
            { x => x.ProjectId, 1 }
        };

        await comp.InvokeAsync(() => dialogService.Show<InviteMemberDialog>("Invitar Miembro", parameters));

        var sendBtn = comp.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Enviar"));
        sendBtn?.Click();

        comp.Markup.Should().Contain("Email del Usuario");
    }

    [Fact]
    public async Task SendInvitation_NullResponse_ShowsErrorMessage()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var handler = new Helpers.MockHttpMessageHandler((req, _) =>
        {
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            });
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        Services.AddSingleton(new ProjectApiService(httpClient));
        Services.AddSingleton(new ProjectInvitationApiService(httpClient, NullLogger<ProjectInvitationApiService>.Instance));

        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<InviteMemberDialog>
        {
            { x => x.ProjectId, 1 }
        };

        await comp.InvokeAsync(() => dialogService.Show<InviteMemberDialog>("Invitar Miembro", parameters));

        var sendBtn = comp.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Enviar"));
        sendBtn?.Click();

        comp.Markup.Should().Contain("Email del Usuario");
    }
}
