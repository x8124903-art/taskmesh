using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MsProjects.Application.Controllers;
using MsProjects.Application.Models;
using MsProjects.Application.UseCases.ProjectInvitation;
using Xunit;

namespace MsProjects.Tests.Application.Controllers;

public sealed class ProjectInvitationsControllerShould
{
    private readonly Mock<ICreateProjectInvitationUseCase> _createMock = new();
    private readonly Mock<IGetInvitationDetailsUseCase> _getDetailsMock = new();
    private readonly Mock<IGetInvitationByTokenUseCase> _getByTokenMock = new();
    private readonly Mock<IGetProjectPendingInvitationsUseCase> _getPendingMock = new();
    private readonly Mock<IGetMyInvitationsUseCase> _getMyMock = new();
    private readonly Mock<IAcceptProjectInvitationUseCase> _acceptMock = new();
    private readonly Mock<IRejectProjectInvitationUseCase> _rejectMock = new();
    private readonly Mock<ICancelProjectInvitationUseCase> _cancelMock = new();
    private readonly ProjectInvitationsController _controller;

    public ProjectInvitationsControllerShould()
    {
        _controller = new ProjectInvitationsController(
            _createMock.Object, _getDetailsMock.Object, _getByTokenMock.Object,
            _getPendingMock.Object, _getMyMock.Object, _acceptMock.Object,
            _rejectMock.Object, _cancelMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.HttpContext.Request.Headers["X-User-Id"] = "100";
    }

    [Fact]
    public async Task CreateInvitation_ReturnsCreatedAtAction()
    {
        var request = new InviteMemberRequest(1, "test@demo.com", "Member");
        var invitation = new ProjectInvitationModel(1, 1, "Project One", "test@demo.com", 3, "Member", "token123", "Pending", 100, "Inviter", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null);

        _createMock.Setup(x => x.ExecuteAsync(request, 100, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        var result = await _controller.CreateInvitation(request, CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>();
        ((CreatedAtActionResult)result).Value.Should().Be(invitation);
    }

    [Fact]
    public async Task GetInvitationById_ReturnsOk()
    {
        var invitation = new ProjectInvitationModel(1, 1, "P", "e@e.com", 3, "Member", "tok", "Pending", 100, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null);
        _getDetailsMock.Setup(x => x.ExecuteAsync(1, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        var result = await _controller.GetInvitationById(1, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(invitation);
    }

    [Fact]
    public async Task GetInvitationByToken_ReturnsOk()
    {
        var invitation = new ProjectInvitationModel(1, 1, "P", "e@e.com", 3, "Member", "tok", "Pending", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null);
        _getByTokenMock.Setup(x => x.ExecuteAsync("tok", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        var result = await _controller.GetInvitationByToken("tok", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)result).Value.Should().Be(invitation);
    }

    [Fact]
    public async Task GetPendingInvitations_ReturnsOk()
    {
        var invitations = new List<ProjectInvitationModel>();
        _getPendingMock.Setup(x => x.ExecuteAsync(1, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitations);

        var result = await _controller.GetPendingInvitations(1, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetMyInvitations_ReturnsOk_WithInvitations()
    {
        var invitations = new List<ProjectInvitationModel>
        {
            new(1, 1, "P", "a@a.com", 3, "Member", "t1", "Pending", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null)
        };
        _getMyMock.Setup(x => x.ExecuteAsync("a@a.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitations);

        var result = await _controller.GetMyInvitations("a@a.com", null, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetMyInvitations_ReturnsBadRequest_WhenEmailEmpty()
    {
        var result = await _controller.GetMyInvitations("", null, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AcceptInvitation_ReturnsOk()
    {
        _acceptMock.Setup(x => x.ExecuteAsync("token123", 100, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.AcceptInvitation(new AcceptInvitationRequest("token123"), CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RejectInvitation_ReturnsOk()
    {
        _rejectMock.Setup(x => x.ExecuteAsync("tok", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.RejectInvitation(new RejectInvitationRequest("tok"), CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteInvitation_ReturnsNoContent()
    {
        _cancelMock.Setup(x => x.ExecuteAsync("tok", 100, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.DeleteInvitation("tok", CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task CreateInvitation_ThrowsUnauthorized_WhenNoUserIdHeader()
    {
        _controller.HttpContext.Request.Headers.Remove("X-User-Id");

        Func<Task> act = () => _controller.CreateInvitation(new InviteMemberRequest(1, "e@e.com", "Member"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task CreateInvitation_ThrowsArgumentException_WhenInvalidUserId()
    {
        _controller.HttpContext.Request.Headers["X-User-Id"] = "not-a-number";

        Func<Task> act = () => _controller.CreateInvitation(new InviteMemberRequest(1, "e@e.com", "Member"), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
