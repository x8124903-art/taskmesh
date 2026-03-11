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

public sealed class ChangeProjectMemberRoleUseCaseShould
{
    private readonly Mock<IProjectMemberService> _memberServiceMock = new();
    private readonly Mock<IProjectAuthorizationService> _authServiceMock = new();
    private readonly ChangeProjectMemberRoleUseCase _useCase;

    public ChangeProjectMemberRoleUseCaseShould()
    {
        _useCase = new ChangeProjectMemberRoleUseCase(_memberServiceMock.Object, _authServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ChangesRole_WhenAuthorized()
    {
        _authServiceMock.Setup(x => x.HasProjectRoleAsync(1, 100, It.IsAny<CancellationToken>(), "Owner", "Admin")).ReturnsAsync(true);
        _memberServiceMock.Setup(x => x.ChangeRoleAsync(100, 5, "Admin", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _useCase.ExecuteAsync(100, 5, "Admin", 1, CancellationToken.None);

        _memberServiceMock.Verify(x => x.ChangeRoleAsync(100, 5, "Admin", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsUnauthorized_WhenUserLacksRole()
    {
        _authServiceMock.Setup(x => x.HasProjectRoleAsync(99, 100, It.IsAny<CancellationToken>(), "Owner", "Admin")).ReturnsAsync(false);

        var act = () => _useCase.ExecuteAsync(100, 5, "Admin", 99, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
