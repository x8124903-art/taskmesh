using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MsProjects.Application.Models;
using MsProjects.Application.UseCases.ProjectInvitation;
using MsProjects.Domain.Services;
using Xunit;

namespace MsProjects.Tests.Application.UseCases.ProjectInvitation;

public sealed class GetInvitationByTokenUseCaseShould
{
    private readonly Mock<IProjectInvitationService> _invitationServiceMock = new();
    private readonly GetInvitationByTokenUseCase _useCase;

    public GetInvitationByTokenUseCaseShould()
    {
        _useCase = new GetInvitationByTokenUseCase(_invitationServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsInvitation_WhenFound()
    {
        var invitation = new ProjectInvitationModel(1, 1, "P", "e@e.com", 3, "Member", "tok", "Pending", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null);
        _invitationServiceMock.Setup(x => x.GetInvitationByTokenAsync("tok", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);

        var result = await _useCase.ExecuteAsync("tok", CancellationToken.None);

        result.Should().Be(invitation);
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsKeyNotFound_WhenNotFound()
    {
        _invitationServiceMock.Setup(x => x.GetInvitationByTokenAsync("bad", It.IsAny<CancellationToken>())).ReturnsAsync((ProjectInvitationModel?)null);

        var act = () => _useCase.ExecuteAsync("bad", CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
