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

public sealed class DeleteProjectUseCaseShould
{
    private readonly Mock<IProjectService> _projectServiceMock = new();
    private readonly Mock<IProjectAuthorizationService> _authServiceMock = new();
    private readonly DeleteProjectUseCase _useCase;

    public DeleteProjectUseCaseShould()
    {
        _useCase = new DeleteProjectUseCase(_projectServiceMock.Object, _authServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DeletesProject_WhenOwner()
    {
        var project = new ProjectModel(1, "P", "D", 1, "Active", 1, "O", false, DateTime.UtcNow);
        _projectServiceMock.Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(project);
        _authServiceMock.Setup(x => x.IsProjectOwnerAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _projectServiceMock.Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _useCase.ExecuteAsync(1, 1, CancellationToken.None);

        _projectServiceMock.Verify(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsKeyNotFound_WhenProjectNotFound()
    {
        _projectServiceMock.Setup(x => x.GetAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((ProjectModel?)null);

        var act = () => _useCase.ExecuteAsync(999, 1, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsUnauthorized_WhenNotOwner()
    {
        var project = new ProjectModel(1, "P", "D", 1, "Active", 1, "O", false, DateTime.UtcNow);
        _projectServiceMock.Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(project);
        _authServiceMock.Setup(x => x.IsProjectOwnerAsync(99, 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = () => _useCase.ExecuteAsync(1, 99, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
