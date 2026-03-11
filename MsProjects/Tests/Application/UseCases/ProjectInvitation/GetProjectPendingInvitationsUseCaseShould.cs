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

public sealed class GetProjectPendingInvitationsUseCaseShould
{
    private readonly Mock<IProjectInvitationService> _invitationServiceMock = new();
    private readonly Mock<IProjectAuthorizationService> _authServiceMock = new();
    private readonly GetProjectPendingInvitationsUseCase _useCase;

    public GetProjectPendingInvitationsUseCaseShould()
    {
        _useCase = new GetProjectPendingInvitationsUseCase(_invitationServiceMock.Object, _authServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsPendingInvitations_WhenAuthorized()
    {
        var invitations = new List<ProjectInvitationModel>
        {
            new(1, 1, "P", "e@e.com", 3, "Member", "t1", "Pending", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null)
        };
        _authServiceMock.Setup(x => x.HasProjectRoleAsync(1, 1, It.IsAny<CancellationToken>(), "Owner", "Admin")).ReturnsAsync(true);
        _invitationServiceMock.Setup(x => x.GetPendingInvitationsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(invitations);

        var result = await _useCase.ExecuteAsync(1, 1, CancellationToken.None);

        result.Should().BeEquivalentTo(invitations);
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsUnauthorized_WhenUserLacksRole()
    {
        _authServiceMock.Setup(x => x.HasProjectRoleAsync(99, 1, It.IsAny<CancellationToken>(), "Owner", "Admin")).ReturnsAsync(false);

        var act = () => _useCase.ExecuteAsync(1, 99, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
