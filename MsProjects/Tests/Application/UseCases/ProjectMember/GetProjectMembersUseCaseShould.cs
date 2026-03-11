using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MsProjects.Application.Models;
using MsProjects.Application.UseCases.ProjectMember;
using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;
using Xunit;

namespace MsProjects.Tests.Application.UseCases.ProjectMember;

public sealed class GetProjectMembersUseCaseShould
{
    private readonly Mock<IProjectMemberService> _memberServiceMock = new();
    private readonly Mock<IProjectAuthorizationService> _authServiceMock = new();
    private readonly GetProjectMembersUseCase _useCase;

    public GetProjectMembersUseCaseShould()
    {
        _useCase = new GetProjectMembersUseCase(_memberServiceMock.Object, _authServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsMembers_WhenUserIsMember()
    {
        var members = new List<ProjectMemberModel>
        {
            new(1, 1, 1, "User1", "u1@test.com", 1, "Owner", DateTime.UtcNow, DateTime.UtcNow)
        };
        _authServiceMock.Setup(x => x.IsProjectMemberAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _memberServiceMock.Setup(x => x.GetMembersAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(members);

        var result = await _useCase.ExecuteAsync(1, 1, CancellationToken.None);

        result.Should().BeEquivalentTo(members);
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsUnauthorized_WhenUserNotMember()
    {
        _authServiceMock.Setup(x => x.IsProjectMemberAsync(99, 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = () => _useCase.ExecuteAsync(1, 99, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
