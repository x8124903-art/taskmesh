using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsProjects.Application.Models;
using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;
using MsProjects.Domain.Services.Exceptions;
using MsProjects.Infrastructure.EventBus;
using MsProjects.Infrastructure.HttpClients;
using MsProjects.Infrastructure.Repositories;
using Xunit;

namespace MsProjects.Tests.Domain.Services;

public sealed class ProjectInvitationServiceShould
{
    private readonly Mock<IProjectInvitationRepository> _invitationRepoMock;
    private readonly Mock<IProjectMemberRepository> _memberRepoMock;
    private readonly Mock<IProjectRepository> _projectRepoMock;
    private readonly Mock<IProjectAuthorizationService> _authServiceMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly Mock<IAuthHttpClient> _authHttpClientMock;
    private readonly Mock<ILogger<ProjectInvitationService>> _loggerMock;
    private readonly ProjectInvitationService _service;

    public ProjectInvitationServiceShould()
    {
        _invitationRepoMock = new Mock<IProjectInvitationRepository>();
        _memberRepoMock = new Mock<IProjectMemberRepository>();
        _projectRepoMock = new Mock<IProjectRepository>();
        _authServiceMock = new Mock<IProjectAuthorizationService>();
        _eventBusMock = new Mock<IEventBus>();
        _authHttpClientMock = new Mock<IAuthHttpClient>();
        _loggerMock = new Mock<ILogger<ProjectInvitationService>>();
        _service = new ProjectInvitationService(
            _invitationRepoMock.Object,
            _memberRepoMock.Object,
            _projectRepoMock.Object,
            _authServiceMock.Object,
            _eventBusMock.Object,
            _authHttpClientMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateInvitationAsync_CreatesInvitation_WhenValidRequest()
    {
        const int PROJECT_ID = 100;
        const string EMAIL = "newuser@test.com";
        const string ROLE = "Member";
        const int INVITED_BY = 1;
        
        var request = new InviteMemberRequest(PROJECT_ID, EMAIL, ROLE);
        var expectedInvitation = new ProjectInvitationModel(
            1, PROJECT_ID, "Test Project", EMAIL, 3, "Member", "token123", 
            "Pending", INVITED_BY, "Admin User", DateTime.UtcNow, 
            DateTime.UtcNow.AddDays(7), null, null);

        _projectRepoMock.Setup(x => x.GetAsync(PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProjectModel(PROJECT_ID, "Test Project", "Desc", 1, "Active", 1, "Owner", false, DateTime.UtcNow));

        _memberRepoMock.Setup(x => x.ExistsMemberByProjectIdAndEmailAsync(PROJECT_ID, EMAIL, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _invitationRepoMock.Setup(x => x.ExistsPendingByProjectAndEmailAsync(PROJECT_ID, EMAIL, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _invitationRepoMock.Setup(x => x.CreateAsync(
            PROJECT_ID, EMAIL, ROLE, It.IsAny<string>(), INVITED_BY, It.IsAny<string?>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedInvitation);

        var result = await _service.CreateInvitationAsync(request, INVITED_BY);

        result.Should().NotBeNull();
        result.Email.Should().Be(EMAIL);
        result.RoleName.Should().Be(ROLE);
    }

    [Fact]
    public async Task CreateInvitationAsync_ThrowsException_WhenProjectNotFound()
    {
        const int PROJECT_ID = 999;
        var request = new InviteMemberRequest(PROJECT_ID, "test@test.com", "Member");

        _projectRepoMock.Setup(x => x.GetAsync(PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProjectModel?)null);

        await Assert.ThrowsAsync<ProjectNotFoundException>(
            () => _service.CreateInvitationAsync(request, 1));
    }

    [Fact]
    public async Task AcceptInvitationAsync_CreatesProjectMember_WhenValidToken()
    {
        const string TOKEN = "valid-token";
        const int USER_ID = 5;
        const int PROJECT_ID = 100;
        
        var invitation = new ProjectInvitationModel(
            1, PROJECT_ID, "Project", "user@test.com", 3, "Member", TOKEN,
            "Pending", 1, "Admin", DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(6), null, null);

        _invitationRepoMock.Setup(x => x.GetByTokenAsync(TOKEN, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        _invitationRepoMock.Setup(x => x.AcceptAsync(invitation.IdProjectInvitation, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _memberRepoMock.Setup(x => x.AddMemberAsync(PROJECT_ID, USER_ID, "Member", It.IsAny<CancellationToken>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        await _service.AcceptInvitationAsync(TOKEN, USER_ID);

        _memberRepoMock.Verify(x => x.AddMemberAsync(PROJECT_ID, USER_ID, "Member", It.IsAny<CancellationToken>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
        _invitationRepoMock.Verify(x => x.AcceptAsync(invitation.IdProjectInvitation, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AcceptInvitationAsync_ThrowsException_WhenTokenNotFound()
    {
        const string TOKEN = "invalid-token";

        _invitationRepoMock.Setup(x => x.GetByTokenAsync(TOKEN, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProjectInvitationModel?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AcceptInvitationAsync(TOKEN, 1));
    }

    [Fact]
    public async Task RejectInvitationAsync_UpdatesStatus_WhenValidToken()
    {
        const string TOKEN = "valid-token";
        var invitation = new ProjectInvitationModel(
            1, 100, "Project", "user@test.com", 3, "Member", TOKEN,
            "Pending", 1, "Admin", DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(6), null, null);

        _invitationRepoMock.Setup(x => x.GetByTokenAsync(TOKEN, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        _invitationRepoMock.Setup(x => x.RejectAsync(invitation.IdProjectInvitation, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.RejectInvitationAsync(TOKEN);

        _invitationRepoMock.Verify(x => x.RejectAsync(invitation.IdProjectInvitation, It.IsAny<CancellationToken>()), Times.Once);
        _memberRepoMock.Verify(x => x.AddMemberAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task GetInvitationByIdAsync_ReturnsInvitation()
    {
        var invitation = new ProjectInvitationModel(1, 1, "P", "e@e.com", 3, "Member", "tok", "Pending", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null);
        _invitationRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(invitation);

        var result = await _service.GetInvitationByIdAsync(1);

        result.Should().NotBeNull();
        result!.IdProjectInvitation.Should().Be(1);
    }

    [Fact]
    public async Task GetInvitationByTokenAsync_ReturnsInvitation()
    {
        var invitation = new ProjectInvitationModel(1, 1, "P", "e@e.com", 3, "Member", "tok", "Pending", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null);
        _invitationRepoMock.Setup(x => x.GetByTokenAsync("tok", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);

        var result = await _service.GetInvitationByTokenAsync("tok");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPendingInvitationsAsync_MarksExpiredAndReturns()
    {
        var invitations = new List<ProjectInvitationModel>
        {
            new(1, 1, "P", "e@e.com", 3, "Member", "tok", "Pending", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null)
        };
        _invitationRepoMock.Setup(x => x.MarkExpiredInvitationsAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _invitationRepoMock.Setup(x => x.GetPendingByProjectIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(invitations);

        var result = await _service.GetPendingInvitationsAsync(1);

        result.Should().ContainSingle();
        _invitationRepoMock.Verify(x => x.MarkExpiredInvitationsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetInvitationsByEmailAsync_MarksExpiredAndReturns()
    {
        var invitations = new List<ProjectInvitationModel>();
        _invitationRepoMock.Setup(x => x.MarkExpiredInvitationsAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _invitationRepoMock.Setup(x => x.GetByEmailAsync("e@e.com", It.IsAny<CancellationToken>())).ReturnsAsync(invitations);

        var result = await _service.GetInvitationsByEmailAsync("e@e.com");

        _invitationRepoMock.Verify(x => x.MarkExpiredInvitationsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteInvitationAsync_WithValidPendingToken_Deletes()
    {
        var invitation = new ProjectInvitationModel(1, 1, "P", "e@e.com", 3, "Member", "tok", "Pending", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null);
        _invitationRepoMock.Setup(x => x.GetByTokenAsync("tok", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);
        _invitationRepoMock.Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _service.DeleteInvitationAsync("tok");

        _invitationRepoMock.Verify(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteInvitationAsync_WithNonPendingToken_Throws()
    {
        var invitation = new ProjectInvitationModel(1, 1, "P", "e@e.com", 3, "Member", "tok", "Accepted", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), DateTime.UtcNow, null);
        _invitationRepoMock.Setup(x => x.GetByTokenAsync("tok", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteInvitationAsync("tok"));
    }

    [Fact]
    public async Task DeleteInvitationAsync_WithInvalidToken_Throws()
    {
        _invitationRepoMock.Setup(x => x.GetByTokenAsync("bad", It.IsAny<CancellationToken>())).ReturnsAsync((ProjectInvitationModel?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteInvitationAsync("bad"));
    }

    [Fact]
    public async Task AcceptInvitationAsync_WithNonPendingStatus_Throws()
    {
        var invitation = new ProjectInvitationModel(1, 1, "P", "e@e.com", 3, "Member", "tok", "Accepted", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), DateTime.UtcNow, null);
        _invitationRepoMock.Setup(x => x.GetByTokenAsync("tok", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AcceptInvitationAsync("tok", 5));
    }

    [Fact]
    public async Task AcceptInvitationAsync_WithExpiredInvitation_Throws()
    {
        var invitation = new ProjectInvitationModel(1, 1, "P", "e@e.com", 3, "Member", "tok", "Pending", 1, "A", DateTime.UtcNow.AddDays(-8), DateTime.UtcNow.AddDays(-1), null, null);
        _invitationRepoMock.Setup(x => x.GetByTokenAsync("tok", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AcceptInvitationAsync("tok", 5));
    }

    [Fact]
    public async Task AcceptInvitationAsync_WithDuplicateMember_Throws()
    {
        var invitation = new ProjectInvitationModel(1, 1, "P", "e@e.com", 3, "Member", "tok", "Pending", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null);
        _invitationRepoMock.Setup(x => x.GetByTokenAsync("tok", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);
        _memberRepoMock.Setup(x => x.ExistsByProjectIdAndUserIdAsync(1, 5, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<MsProjects.Domain.Services.Exceptions.DuplicateMemberException>(() => _service.AcceptInvitationAsync("tok", 5));
    }

    [Fact]
    public async Task RejectInvitationAsync_WithNonPendingStatus_Throws()
    {
        var invitation = new ProjectInvitationModel(1, 1, "P", "e@e.com", 3, "Member", "tok", "Rejected", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, DateTime.UtcNow);
        _invitationRepoMock.Setup(x => x.GetByTokenAsync("tok", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RejectInvitationAsync("tok"));
    }

    [Fact]
    public async Task RejectInvitationAsync_WithInvalidToken_Throws()
    {
        _invitationRepoMock.Setup(x => x.GetByTokenAsync("bad", It.IsAny<CancellationToken>())).ReturnsAsync((ProjectInvitationModel?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RejectInvitationAsync("bad"));
    }

    [Fact]
    public async Task CreateInvitationAsync_WithInvalidRole_ThrowsInvalidRoleException()
    {
        var request = new InviteMemberRequest(1, "e@e.com", "InvalidRole");
        _projectRepoMock.Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProjectModel(1, "P", "D", 1, "Active", 1, "O", false, DateTime.UtcNow));

        await Assert.ThrowsAsync<MsProjects.Domain.Services.Exceptions.InvalidRoleException>(
            () => _service.CreateInvitationAsync(request, 1));
    }

    [Fact]
    public async Task CreateInvitationAsync_WithDuplicatePendingInvitation_ThrowsInvalidOperationException()
    {
        var request = new InviteMemberRequest(1, "e@e.com", "Member");
        _projectRepoMock.Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProjectModel(1, "P", "D", 1, "Active", 1, "O", false, DateTime.UtcNow));
        _memberRepoMock.Setup(x => x.ExistsMemberByProjectIdAndEmailAsync(1, "e@e.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _invitationRepoMock.Setup(x => x.ExistsPendingByProjectAndEmailAsync(1, "e@e.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateInvitationAsync(request, 1));
    }

    [Fact]
    public async Task CreateInvitationAsync_WhenEmailIsExistingMember_ThrowsDuplicateMemberException()
    {
        var request = new InviteMemberRequest(1, "existing@test.com", "Member");
        _projectRepoMock.Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProjectModel(1, "P", "D", 1, "Active", 1, "O", false, DateTime.UtcNow));
        _memberRepoMock.Setup(x => x.ExistsMemberByProjectIdAndEmailAsync(1, "existing@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<DuplicateMemberException>(
            () => _service.CreateInvitationAsync(request, 1));
    }
}
