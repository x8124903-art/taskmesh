using System;
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

public sealed class CreateProjectInvitationUseCaseShould
{
    private readonly Mock<IProjectInvitationService> _invitationServiceMock = new();
    private readonly Mock<IProjectAuthorizationService> _authServiceMock = new();
    private readonly CreateProjectInvitationUseCase _useCase;

    public CreateProjectInvitationUseCaseShould()
    {
        _useCase = new CreateProjectInvitationUseCase(_invitationServiceMock.Object, _authServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsInvitation_WhenAuthorized()
    {
        var request = new InviteMemberRequest(1, "test@demo.com", "Member");
        var invitation = new ProjectInvitationModel(1, 1, "P", "test@demo.com", 3, "Member", "tok", "Pending", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null);

        _authServiceMock.Setup(x => x.HasProjectRoleAsync(1, 1, It.IsAny<CancellationToken>(), "Owner", "Admin")).ReturnsAsync(true);
        _invitationServiceMock.Setup(x => x.CreateInvitationAsync(request, 1, It.IsAny<CancellationToken>(), "John")).ReturnsAsync(invitation);

        var result = await _useCase.ExecuteAsync(request, 1, "John", CancellationToken.None);

        result.Should().Be(invitation);
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsUnauthorized_WhenUserLacksRole()
    {
        var request = new InviteMemberRequest(1, "test@demo.com", "Member");
        _authServiceMock.Setup(x => x.HasProjectRoleAsync(99, 1, It.IsAny<CancellationToken>(), "Owner", "Admin")).ReturnsAsync(false);

        var act = () => _useCase.ExecuteAsync(request, 99, null, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
