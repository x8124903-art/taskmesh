using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MsProjects.Application.Models;
using MsProjects.Application.UseCases.Project;
using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;
using Xunit;

namespace MsProjects.Tests.Application.UseCases.Project;

public sealed class GetProjectDetailsUseCaseShould
{
    private readonly Mock<IProjectService> _projectServiceMock = new();
    private readonly Mock<IProjectAuthorizationService> _authServiceMock = new();
    private readonly GetProjectDetailsUseCase _useCase;

    public GetProjectDetailsUseCaseShould()
    {
        _useCase = new GetProjectDetailsUseCase(_projectServiceMock.Object, _authServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsProject_WhenUserIsMember()
    {
        var project = new ProjectModel(1, "P", "D", 1, "Active", 1, "O", false, DateTime.UtcNow);
        _projectServiceMock.Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(project);
        _authServiceMock.Setup(x => x.IsProjectMemberAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _useCase.ExecuteAsync(1, 1, CancellationToken.None);

        result.Should().Be(project);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenProjectNotFound()
    {
        _projectServiceMock.Setup(x => x.GetAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((ProjectModel?)null);

        var result = await _useCase.ExecuteAsync(999, 1, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsUnauthorized_WhenUserIsNotMember()
    {
        var project = new ProjectModel(1, "P", "D", 1, "Active", 1, "O", false, DateTime.UtcNow);
        _projectServiceMock.Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(project);
        _authServiceMock.Setup(x => x.IsProjectMemberAsync(99, 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = () => _useCase.ExecuteAsync(1, 99, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
