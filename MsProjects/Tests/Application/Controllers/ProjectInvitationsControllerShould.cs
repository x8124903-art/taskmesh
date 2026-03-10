using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MsProjects.Application.Controllers;
using MsProjects.Application.Models;
using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;

namespace MsProjects.Tests.Application.Controllers;

public sealed class ProjectInvitationsControllerShould
{
    [Fact]
    public async Task CreateInvitation_WithValidRequest_ReturnsCreated()
    {
        var invitationServiceMock = new Mock<IProjectInvitationService>();
        var authServiceMock = new Mock<IProjectAuthorizationService>();
        var loggerMock = new Mock<ILogger<ProjectInvitationsController>>();
        
        var request = new InviteMemberRequest(1, "test@demo.com", "Member");
        var invitation = new ProjectInvitationModel(1, 1, "Project One", "test@demo.com", 3, "Member", "token123", "Pending", 100, "Inviter", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null);
        
        authServiceMock.Setup(x => x.HasProjectRoleAsync(100, 1, It.IsAny<CancellationToken>(), ProjectRoles.Owner, ProjectRoles.Admin))
            .ReturnsAsync(true);
        
        invitationServiceMock.Setup(x => x.CreateInvitationAsync(request, 100, It.IsAny<CancellationToken>(), It.IsAny<string?>()))
            .ReturnsAsync(invitation);
        
        var controller = new ProjectInvitationsController(invitationServiceMock.Object, authServiceMock.Object, loggerMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.HttpContext.Request.Headers["X-User-Id"] = "100";
        
        var result = await controller.CreateInvitation(request, CancellationToken.None);
        
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.Value.Should().Be(invitation);
    }

    [Fact]
    public async Task CreateInvitation_WithoutPermission_ReturnsForbid()
    {
        var invitationServiceMock = new Mock<IProjectInvitationService>();
        var authServiceMock = new Mock<IProjectAuthorizationService>();
        var loggerMock = new Mock<ILogger<ProjectInvitationsController>>();
        
        var request = new InviteMemberRequest(1, "test@demo.com", "Member");
        
        authServiceMock.Setup(x => x.HasProjectRoleAsync(100, 1, It.IsAny<CancellationToken>(), ProjectRoles.Owner, ProjectRoles.Admin))
            .ReturnsAsync(false);
        
        var controller = new ProjectInvitationsController(invitationServiceMock.Object, authServiceMock.Object, loggerMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.HttpContext.Request.Headers["X-User-Id"] = "100";
        
        var result = await controller.CreateInvitation(request, CancellationToken.None);
        
        result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task GetInvitationById_WithValidId_ReturnsOk()
    {
        var invitationServiceMock = new Mock<IProjectInvitationService>();
        var authServiceMock = new Mock<IProjectAuthorizationService>();
        var loggerMock = new Mock<ILogger<ProjectInvitationsController>>();
        
        var invitation = new ProjectInvitationModel(1, 1, "Project One", "test@demo.com", 3, "Member", "token123", "Pending", 100, "Inviter", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null);
        
        invitationServiceMock.Setup(x => x.GetInvitationByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);
        
        authServiceMock.Setup(x => x.HasProjectRoleAsync(100, 1, It.IsAny<CancellationToken>(), ProjectRoles.Owner, ProjectRoles.Admin))
            .ReturnsAsync(true);
        
        var controller = new ProjectInvitationsController(invitationServiceMock.Object, authServiceMock.Object, loggerMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.HttpContext.Request.Headers["X-User-Id"] = "100";
        
        var result = await controller.GetInvitationById(1, CancellationToken.None);
        
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(invitation);
    }

    [Fact]
    public async Task GetInvitationById_WithInvalidId_ReturnsNotFound()
    {
        var invitationServiceMock = new Mock<IProjectInvitationService>();
        var authServiceMock = new Mock<IProjectAuthorizationService>();
        var loggerMock = new Mock<ILogger<ProjectInvitationsController>>();
        
        invitationServiceMock.Setup(x => x.GetInvitationByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProjectInvitationModel?)null);
        
        var controller = new ProjectInvitationsController(invitationServiceMock.Object, authServiceMock.Object, loggerMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.HttpContext.Request.Headers["X-User-Id"] = "100";
        
        var result = await controller.GetInvitationById(999, CancellationToken.None);
        
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task AcceptInvitation_WithValidToken_ReturnsOk()
    {
        var invitationServiceMock = new Mock<IProjectInvitationService>();
        var authServiceMock = new Mock<IProjectAuthorizationService>();
        var loggerMock = new Mock<ILogger<ProjectInvitationsController>>();
        
        var request = new AcceptInvitationRequest("token123");
        
        invitationServiceMock.Setup(x => x.AcceptInvitationAsync("token123", 100, It.IsAny<CancellationToken>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        
        var controller = new ProjectInvitationsController(invitationServiceMock.Object, authServiceMock.Object, loggerMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.HttpContext.Request.Headers["X-User-Id"] = "100";
        
        var result = await controller.AcceptInvitation(request, CancellationToken.None);
        
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetMyInvitations_WithEmailAndStatus_ReturnsFilteredInvitations()
    {
        var invitationServiceMock = new Mock<IProjectInvitationService>();
        var authServiceMock = new Mock<IProjectAuthorizationService>();
        var loggerMock = new Mock<ILogger<ProjectInvitationsController>>();
        
        var invitations = new List<ProjectInvitationModel>
        {
            new ProjectInvitationModel(1, 1, "Project One", "test@demo.com", 3, "Member", "token1", "Pending", 100, "Inviter", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null),
            new ProjectInvitationModel(2, 2, "Project Two", "test@demo.com", 3, "Member", "token2", "Accepted", 100, "Inviter", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), DateTime.UtcNow, null)
        };
        
        invitationServiceMock.Setup(x => x.GetInvitationsByEmailAsync("test@demo.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitations);
        
        var controller = new ProjectInvitationsController(invitationServiceMock.Object, authServiceMock.Object, loggerMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.HttpContext.Request.Headers["X-User-Id"] = "100";
        
        var result = await controller.GetMyInvitations("test@demo.com", "Pending", CancellationToken.None);
        
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var returnedInvitations = okResult!.Value as IEnumerable<ProjectInvitationModel>;
        returnedInvitations.Should().HaveCount(1);
        returnedInvitations!.First().Status.Should().Be("Pending");
    }

    private static ProjectInvitationsController CreateController(
        Mock<IProjectInvitationService> invSvc,
        Mock<IProjectAuthorizationService> authSvc,
        string userId = "100")
    {
        var logger = new Mock<ILogger<ProjectInvitationsController>>();
        var controller = new ProjectInvitationsController(invSvc.Object, authSvc.Object, logger.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.HttpContext.Request.Headers["X-User-Id"] = userId;
        return controller;
    }

    [Fact]
    public async Task GetInvitationByToken_WithValidToken_ReturnsOk()
    {
        var invSvc = new Mock<IProjectInvitationService>();
        var authSvc = new Mock<IProjectAuthorizationService>();
        var invitation = new ProjectInvitationModel(1, 1, "P", "e@e.com", 3, "Member", "tok", "Pending", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null);
        invSvc.Setup(x => x.GetInvitationByTokenAsync("tok", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);
        var controller = CreateController(invSvc, authSvc);

        var result = await controller.GetInvitationByToken("tok", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)result).Value.Should().Be(invitation);
    }

    [Fact]
    public async Task GetInvitationByToken_WithInvalidToken_ReturnsNotFound()
    {
        var invSvc = new Mock<IProjectInvitationService>();
        var authSvc = new Mock<IProjectAuthorizationService>();
        invSvc.Setup(x => x.GetInvitationByTokenAsync("bad", It.IsAny<CancellationToken>())).ReturnsAsync((ProjectInvitationModel?)null);
        var controller = CreateController(invSvc, authSvc);

        var result = await controller.GetInvitationByToken("bad", CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetPendingInvitations_WithPermission_ReturnsOk()
    {
        var invSvc = new Mock<IProjectInvitationService>();
        var authSvc = new Mock<IProjectAuthorizationService>();
        authSvc.Setup(x => x.HasProjectRoleAsync(100, 1, It.IsAny<CancellationToken>(), ProjectRoles.Owner, ProjectRoles.Admin)).ReturnsAsync(true);
        var invitations = new List<ProjectInvitationModel>();
        invSvc.Setup(x => x.GetPendingInvitationsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(invitations);
        var controller = CreateController(invSvc, authSvc);

        var result = await controller.GetPendingInvitations(1, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPendingInvitations_WithoutPermission_ReturnsForbid()
    {
        var invSvc = new Mock<IProjectInvitationService>();
        var authSvc = new Mock<IProjectAuthorizationService>();
        authSvc.Setup(x => x.HasProjectRoleAsync(100, 1, It.IsAny<CancellationToken>(), ProjectRoles.Owner, ProjectRoles.Admin)).ReturnsAsync(false);
        var controller = CreateController(invSvc, authSvc);

        var result = await controller.GetPendingInvitations(1, CancellationToken.None);

        result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task GetInvitationById_WithoutPermission_ReturnsForbid()
    {
        var invSvc = new Mock<IProjectInvitationService>();
        var authSvc = new Mock<IProjectAuthorizationService>();
        var invitation = new ProjectInvitationModel(1, 1, "P", "e@e.com", 3, "Member", "tok", "Pending", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null);
        invSvc.Setup(x => x.GetInvitationByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(invitation);
        authSvc.Setup(x => x.HasProjectRoleAsync(100, 1, It.IsAny<CancellationToken>(), ProjectRoles.Owner, ProjectRoles.Admin)).ReturnsAsync(false);
        var controller = CreateController(invSvc, authSvc);

        var result = await controller.GetInvitationById(1, CancellationToken.None);

        result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task RejectInvitation_WithValidToken_ReturnsOk()
    {
        var invSvc = new Mock<IProjectInvitationService>();
        var authSvc = new Mock<IProjectAuthorizationService>();
        invSvc.Setup(x => x.RejectInvitationAsync("tok", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var controller = CreateController(invSvc, authSvc);

        var result = await controller.RejectInvitation(new RejectInvitationRequest("tok"), CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteInvitation_WithPermission_ReturnsNoContent()
    {
        var invSvc = new Mock<IProjectInvitationService>();
        var authSvc = new Mock<IProjectAuthorizationService>();
        var invitation = new ProjectInvitationModel(1, 1, "P", "e@e.com", 3, "Member", "tok", "Pending", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null);
        invSvc.Setup(x => x.GetInvitationByTokenAsync("tok", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);
        invSvc.Setup(x => x.DeleteInvitationAsync("tok", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        authSvc.Setup(x => x.HasProjectRoleAsync(100, 1, It.IsAny<CancellationToken>(), ProjectRoles.Owner, ProjectRoles.Admin)).ReturnsAsync(true);
        var controller = CreateController(invSvc, authSvc);

        var result = await controller.DeleteInvitation("tok", CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteInvitation_WithoutPermission_ReturnsForbid()
    {
        var invSvc = new Mock<IProjectInvitationService>();
        var authSvc = new Mock<IProjectAuthorizationService>();
        var invitation = new ProjectInvitationModel(1, 1, "P", "e@e.com", 3, "Member", "tok", "Pending", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null);
        invSvc.Setup(x => x.GetInvitationByTokenAsync("tok", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);
        authSvc.Setup(x => x.HasProjectRoleAsync(100, 1, It.IsAny<CancellationToken>(), ProjectRoles.Owner, ProjectRoles.Admin)).ReturnsAsync(false);
        var controller = CreateController(invSvc, authSvc);

        var result = await controller.DeleteInvitation("tok", CancellationToken.None);

        result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task DeleteInvitation_WithInvalidToken_ReturnsNotFound()
    {
        var invSvc = new Mock<IProjectInvitationService>();
        var authSvc = new Mock<IProjectAuthorizationService>();
        invSvc.Setup(x => x.GetInvitationByTokenAsync("bad", It.IsAny<CancellationToken>())).ReturnsAsync((ProjectInvitationModel?)null);
        var controller = CreateController(invSvc, authSvc);

        var result = await controller.DeleteInvitation("bad", CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetMyInvitations_WithEmptyEmail_ReturnsBadRequest()
    {
        var invSvc = new Mock<IProjectInvitationService>();
        var authSvc = new Mock<IProjectAuthorizationService>();
        var controller = CreateController(invSvc, authSvc);

        var result = await controller.GetMyInvitations("", null, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetMyInvitations_WithoutStatusFilter_ReturnsAll()
    {
        var invSvc = new Mock<IProjectInvitationService>();
        var authSvc = new Mock<IProjectAuthorizationService>();
        var invitations = new List<ProjectInvitationModel>
        {
            new(1, 1, "P", "a@a.com", 3, "Member", "t1", "Pending", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null),
            new(2, 1, "P", "a@a.com", 3, "Member", "t2", "Accepted", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), DateTime.UtcNow, null)
        };
        invSvc.Setup(x => x.GetInvitationsByEmailAsync("a@a.com", It.IsAny<CancellationToken>())).ReturnsAsync(invitations);
        var controller = CreateController(invSvc, authSvc);

        var result = await controller.GetMyInvitations("a@a.com", null, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var items = ((OkObjectResult)result).Value as IEnumerable<ProjectInvitationModel>;
        items.Should().HaveCount(2);
    }

    [Fact]
    public void GetCurrentUserId_WithMissingHeader_ThrowsUnauthorizedAccessException()
    {
        var invSvc = new Mock<IProjectInvitationService>();
        var authSvc = new Mock<IProjectAuthorizationService>();
        var logger = new Mock<ILogger<ProjectInvitationsController>>();
        var controller = new ProjectInvitationsController(invSvc.Object, authSvc.Object, logger.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        Func<Task> act = () => controller.CreateInvitation(new InviteMemberRequest(1, "e@e.com", "Member"), CancellationToken.None);

        act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public void GetCurrentUserId_WithInvalidHeader_ThrowsArgumentException()
    {
        var invSvc = new Mock<IProjectInvitationService>();
        var authSvc = new Mock<IProjectAuthorizationService>();
        var logger = new Mock<ILogger<ProjectInvitationsController>>();
        var controller = new ProjectInvitationsController(invSvc.Object, authSvc.Object, logger.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        controller.HttpContext.Request.Headers["X-User-Id"] = "not-a-number";

        Func<Task> act = () => controller.CreateInvitation(new InviteMemberRequest(1, "e@e.com", "Member"), CancellationToken.None);

        act.Should().ThrowAsync<ArgumentException>();
    }
}
