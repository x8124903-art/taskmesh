using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MsProjects.Application.Models;
using MsProjects.Application.UseCases.ProjectMember;
using MsProjects.Domain.Services.Authorization;
using Xunit;

namespace MsProjects.Tests.Application.UseCases.ProjectMember;

public sealed class GetProjectMemberRoleUseCaseShould
{
    private readonly Mock<IProjectAuthorizationService> _authServiceMock = new();
    private readonly GetProjectMemberRoleUseCase _useCase;

    public GetProjectMemberRoleUseCaseShould()
    {
        _useCase = new GetProjectMemberRoleUseCase(_authServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsRole_WhenUserIsMember()
    {
        const int GIVEN_PROJECT_ID = 1;
        const int GIVEN_USER_ID = 2;
        const string EXPECTED_ROLE = "Member";
        
        _authServiceMock
            .Setup(x => x.GetUserProjectRoleAsync(GIVEN_USER_ID, GIVEN_PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(EXPECTED_ROLE);

        var result = await _useCase.ExecuteAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Role.Should().Be(EXPECTED_ROLE);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenUserNotMember()
    {
        const int GIVEN_PROJECT_ID = 1;
        const int GIVEN_USER_ID = 99;
        
        _authServiceMock
            .Setup(x => x.GetUserProjectRoleAsync(GIVEN_USER_ID, GIVEN_PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var result = await _useCase.ExecuteAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, CancellationToken.None);

        result.Should().BeNull();
    }
}
