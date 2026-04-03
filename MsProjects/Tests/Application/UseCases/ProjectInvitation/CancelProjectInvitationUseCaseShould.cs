using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MsProjects.Application.Models;
using MsProjects.Application.UseCases.ProjectInvitation;
using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;
using Xunit;

namespace MsProjects.Tests.Application.UseCases.ProjectInvitation;

public sealed class CancelProjectInvitationUseCaseShould
{
    private readonly Mock<IProjectInvitationService> _invitationServiceMock = new();
    private readonly Mock<IProjectAuthorizationService> _authServiceMock = new();
    private readonly CancelProjectInvitationUseCase _useCase;

    public CancelProjectInvitationUseCaseShould()
    {
        _useCase = new CancelProjectInvitationUseCase(_invitationServiceMock.Object, _authServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DeletesInvitation_WhenAuthorized()
    {
        var invitation = new ProjectInvitationModel(1, 1, "P", "e@e.com", 3, "Member", "tok", "Pending", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null);
        _invitationServiceMock.Setup(x => x.GetInvitationByTokenAsync("tok", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);
        _authServiceMock.Setup(x => x.HasProjectRoleAsync(1, 1, It.IsAny<CancellationToken>(), "Owner", "Admin")).ReturnsAsync(true);
        _invitationServiceMock.Setup(x => x.DeleteInvitationAsync("tok", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _useCase.ExecuteAsync("tok", 1, CancellationToken.None);

        _invitationServiceMock.Verify(x => x.DeleteInvitationAsync("tok", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsKeyNotFound_WhenInvitationNotFound()
    {
        _invitationServiceMock.Setup(x => x.GetInvitationByTokenAsync("bad", It.IsAny<CancellationToken>())).ReturnsAsync((ProjectInvitationModel?)null);

        var act = () => _useCase.ExecuteAsync("bad", 1, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsUnauthorized_WhenUserLacksRole()
    {
        var invitation = new ProjectInvitationModel(1, 1, "P", "e@e.com", 3, "Member", "tok", "Pending", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null);
        _invitationServiceMock.Setup(x => x.GetInvitationByTokenAsync("tok", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);
        _authServiceMock.Setup(x => x.HasProjectRoleAsync(99, 1, It.IsAny<CancellationToken>(), "Owner", "Admin")).ReturnsAsync(false);

        var act = () => _useCase.ExecuteAsync("tok", 99, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
