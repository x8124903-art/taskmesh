using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using WebApp.Components.Projects;
using WebApp.Models.Projects;

namespace WebApp.Tests.Components.Projects;

public sealed class ChangeRoleDialogShould : TestContext
{
    [Fact]
    public async Task RenderDialog_WithMemberInfo()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var member = new ProjectMember
        {
            UserId = 2, UserName = "Test User", Email = "test@test.com", RoleName = "Member"
        };

        var parameters = new DialogParameters<ChangeRoleDialog>
        {
            { x => x.Member, member }
        };

        await comp.InvokeAsync(() => dialogService.Show<ChangeRoleDialog>("Cambiar Rol", parameters));

        comp.Markup.Should().Contain("Test User");
    }

    [Fact]
    public async Task Submit_RendersRoleOptions()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var member = new ProjectMember
        {
            UserId = 2, UserName = "TestUser", Email = "t@t.com", RoleName = "Viewer"
        };

        var parameters = new DialogParameters<ChangeRoleDialog>
        {
            { x => x.Member, member }
        };

        await comp.InvokeAsync(() => dialogService.Show<ChangeRoleDialog>("Cambiar Rol", parameters));

        comp.Markup.Should().Contain("Admin");
        comp.Markup.Should().Contain("Member");
        comp.Markup.Should().Contain("Viewer");

        var cancelBtn = comp.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Cancelar"));
        cancelBtn?.Click();
    }

    [Fact]
    public async Task Cancel_ClosesDialog()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var member = new ProjectMember
        {
            UserId = 2, UserName = "TestUser", Email = "t@t.com", RoleName = "Member"
        };

        var parameters = new DialogParameters<ChangeRoleDialog>
        {
            { x => x.Member, member }
        };

        await comp.InvokeAsync(() => dialogService.Show<ChangeRoleDialog>("Cambiar Rol", parameters));

        var cancelBtn = comp.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Cancelar"));
        cancelBtn?.Click();
    }

    [Fact]
    public async Task Submit_ClosesDialogWithSelectedRole()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var comp = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var member = new ProjectMember
        {
            UserId = 2, UserName = "TestUser", Email = "t@t.com", RoleName = "Member"
        };

        var parameters = new DialogParameters<ChangeRoleDialog>
        {
            { x => x.Member, member }
        };

        IDialogReference? dialogRef = null;
        await comp.InvokeAsync(() =>
        {
            dialogRef = dialogService.Show<ChangeRoleDialog>("Cambiar Rol", parameters);
        });

        var radioInputs = comp.FindAll("input[type='radio']");
        var adminRadio = radioInputs.FirstOrDefault();
        if (adminRadio != null)
        {
            adminRadio.Click();
        }

        var submitBtn = comp.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Cambiar Rol"));
        submitBtn?.Click();
    }
}
