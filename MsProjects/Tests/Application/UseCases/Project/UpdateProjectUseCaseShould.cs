using System;
using System.Collections.Generic;
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

public sealed class UpdateProjectUseCaseShould
{
    private readonly Mock<IProjectService> _projectServiceMock = new();
    private readonly Mock<IProjectAuthorizationService> _authServiceMock = new();
    private readonly UpdateProjectUseCase _useCase;

    public UpdateProjectUseCaseShould()
    {
        _useCase = new UpdateProjectUseCase(_projectServiceMock.Object, _authServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesProject_WhenAuthorized()
    {
        var project = new ProjectModel(1, "P", "D", 1, "Active", 1, "O", false, DateTime.UtcNow);
        var request = new UpdateProjectRequest("Updated", "Desc", 2);

        _projectServiceMock.Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(project);
        _authServiceMock.Setup(x => x.HasProjectRoleAsync(1, 1, It.IsAny<CancellationToken>(), "Owner", "Admin")).ReturnsAsync(true);
        _projectServiceMock.Setup(x => x.UpdateAsync(1, request, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _useCase.ExecuteAsync(1, request, 1, CancellationToken.None);

        _projectServiceMock.Verify(x => x.UpdateAsync(1, request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsKeyNotFound_WhenProjectNotFound()
    {
        _projectServiceMock.Setup(x => x.GetAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((ProjectModel?)null);

        var act = () => _useCase.ExecuteAsync(999, new UpdateProjectRequest("U", "D", 1), 1, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsUnauthorized_WhenUserLacksRole()
    {
        var project = new ProjectModel(1, "P", "D", 1, "Active", 1, "O", false, DateTime.UtcNow);
        _projectServiceMock.Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(project);
        _authServiceMock.Setup(x => x.HasProjectRoleAsync(99, 1, It.IsAny<CancellationToken>(), "Owner", "Admin")).ReturnsAsync(false);

        var act = () => _useCase.ExecuteAsync(1, new UpdateProjectRequest("U", "D", 1), 99, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
