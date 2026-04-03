using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MsProjects.Application.Models;
using MsProjects.Application.UseCases.ProjectInvitation;
using MsProjects.Domain.Services;
using Xunit;

namespace MsProjects.Tests.Application.UseCases.ProjectInvitation;

public sealed class GetMyInvitationsUseCaseShould
{
    private readonly Mock<IProjectInvitationService> _invitationServiceMock = new();
    private readonly GetMyInvitationsUseCase _useCase;

    public GetMyInvitationsUseCaseShould()
    {
        _useCase = new GetMyInvitationsUseCase(_invitationServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAllInvitations_WhenNoStatusFilter()
    {
        var invitations = new List<ProjectInvitationModel>
        {
            new(1, 1, "P", "a@a.com", 3, "Member", "t1", "Pending", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null),
            new(2, 1, "P", "a@a.com", 3, "Member", "t2", "Accepted", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), DateTime.UtcNow, null)
        };
        _invitationServiceMock.Setup(x => x.GetInvitationsByEmailAsync("a@a.com", It.IsAny<CancellationToken>())).ReturnsAsync(invitations);

        var result = (await _useCase.ExecuteAsync("a@a.com", null, CancellationToken.None)).ToList();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_FiltersInvitations_WhenStatusProvided()
    {
        var invitations = new List<ProjectInvitationModel>
        {
            new(1, 1, "P", "a@a.com", 3, "Member", "t1", "Pending", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null),
            new(2, 1, "P", "a@a.com", 3, "Member", "t2", "Accepted", 1, "A", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), DateTime.UtcNow, null)
        };
        _invitationServiceMock.Setup(x => x.GetInvitationsByEmailAsync("a@a.com", It.IsAny<CancellationToken>())).ReturnsAsync(invitations);

        var result = (await _useCase.ExecuteAsync("a@a.com", "Pending", CancellationToken.None)).ToList();

        result.Should().HaveCount(1);
        result[0].Status.Should().Be("Pending");
    }
}
