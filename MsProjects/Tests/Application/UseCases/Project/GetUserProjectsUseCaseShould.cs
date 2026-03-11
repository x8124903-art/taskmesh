using System;
using System.Collections.Generic;
using System.Linq;
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

public sealed class GetUserProjectsUseCaseShould
{
    private readonly Mock<IProjectService> _projectServiceMock = new();
    private readonly Mock<IProjectAuthorizationService> _authServiceMock = new();
    private readonly GetUserProjectsUseCase _useCase;

    public GetUserProjectsUseCaseShould()
    {
        _useCase = new GetUserProjectsUseCase(_projectServiceMock.Object, _authServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsOnlyUserProjects()
    {
        var allProjects = new List<ProjectModel>
        {
            new(1, "P1", "D", 1, "Active", 1, "O", false, DateTime.UtcNow),
            new(2, "P2", "D", 1, "Active", 2, "O", false, DateTime.UtcNow),
            new(3, "P3", "D", 1, "Active", 1, "O", false, DateTime.UtcNow)
        };

        _authServiceMock.Setup(x => x.GetUserProjectIdsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 1, 3 });
        _projectServiceMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(allProjects);

        var result = (await _useCase.ExecuteAsync(1, CancellationToken.None)).ToList();

        result.Should().HaveCount(2);
        result.Select(p => p.IdProject).Should().BeEquivalentTo(new[] { 1, 3 });
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenUserHasNoProjects()
    {
        _authServiceMock.Setup(x => x.GetUserProjectIdsAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int>());
        _projectServiceMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProjectModel>());

        var result = await _useCase.ExecuteAsync(99, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
