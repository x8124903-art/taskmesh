using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MsProjects.Application.Models;
using MsProjects.Application.UseCases.Project;
using MsProjects.Domain.Services;
using Xunit;

namespace MsProjects.Tests.Application.UseCases.Project;

public sealed class CreateProjectUseCaseShould
{
    private readonly Mock<IProjectService> _projectServiceMock = new();
    private readonly CreateProjectUseCase _useCase;

    public CreateProjectUseCaseShould()
    {
        _useCase = new CreateProjectUseCase(_projectServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DelegatesToService_AndReturnsProject()
    {
        var request = new AddProjectRequest("New", "Desc");
        var created = new ProjectModel(1, "New", "Desc", 1, "Active", 1, "Owner", false, DateTime.UtcNow);

        _projectServiceMock.Setup(x => x.AddAsync(request, 1, It.IsAny<CancellationToken>(), "John", "john@demo.com"))
            .ReturnsAsync(created);

        var result = await _useCase.ExecuteAsync(request, 1, "John", "john@demo.com", CancellationToken.None);

        result.Should().Be(created);
    }
}
