using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MsProjects.Application.UseCases.ProjectMember;
using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;
using Xunit;

namespace MsProjects.Tests.Application.UseCases.ProjectMember;

public sealed class RemoveProjectMemberUseCaseShould
{
    private readonly Mock<IProjectMemberService> _memberServiceMock = new();
    private readonly Mock<IProjectAuthorizationService> _authServiceMock = new();
    private readonly RemoveProjectMemberUseCase _useCase;

    public RemoveProjectMemberUseCaseShould()
    {
        _useCase = new RemoveProjectMemberUseCase(_memberServiceMock.Object, _authServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_RemovesMember_WhenAuthorized()
    {
        _authServiceMock.Setup(x => x.HasProjectRoleAsync(1, 100, It.IsAny<CancellationToken>(), "Owner", "Admin")).ReturnsAsync(true);
        _memberServiceMock.Setup(x => x.RemoveMemberAsync(100, 5, 1, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _useCase.ExecuteAsync(100, 5, 1, CancellationToken.None);

        _memberServiceMock.Verify(x => x.RemoveMemberAsync(100, 5, 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsUnauthorized_WhenUserLacksRole()
    {
        _authServiceMock.Setup(x => x.HasProjectRoleAsync(99, 100, It.IsAny<CancellationToken>(), "Owner", "Admin")).ReturnsAsync(false);

        var act = () => _useCase.ExecuteAsync(100, 5, 99, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
