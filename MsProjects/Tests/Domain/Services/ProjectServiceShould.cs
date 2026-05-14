namespace MsProjects.Tests.Domain.Services;

using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;
using MsProjects.Infrastructure.EventBus;
using MsProjects.Infrastructure.Repositories;
using MsProjects.Application.Models;
using Microsoft.Extensions.Logging;

public sealed class ProjectServiceShould
{
    [Fact]
    public async Task ReturnProject_WhenExists()
    {
        var repoMock = new Mock<IProjectRepository>();
        var memberRepoMock = new Mock<IProjectMemberRepository>();
        var authServiceMock = new Mock<IProjectAuthorizationService>();
        var eventBusMock = new Mock<IEventBus>();
        var loggerMock = new Mock<ILogger<ProjectService>>();
        
        repoMock.Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProjectModel(1, "Demo", "Description", 1, "Active", 1, "Owner Name", false, DateTime.UtcNow));
        
        var service = new ProjectService(repoMock.Object, memberRepoMock.Object, authServiceMock.Object, eventBusMock.Object, loggerMock.Object);
        
        var project = await service.GetAsync(1, CancellationToken.None);
        
        project.Should().NotBeNull();
        project!.IdProject.Should().Be(1);
    }

    [Fact]
    public async Task ReturnNull_WhenProjectNotFound()
    {
        var repoMock = new Mock<IProjectRepository>();
        var memberRepoMock = new Mock<IProjectMemberRepository>();
        var authServiceMock = new Mock<IProjectAuthorizationService>();
        var eventBusMock = new Mock<IEventBus>();
        var loggerMock = new Mock<ILogger<ProjectService>>();
        
        repoMock.Setup(x => x.GetAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProjectModel?)null);
        
        var service = new ProjectService(repoMock.Object, memberRepoMock.Object, authServiceMock.Object, eventBusMock.Object, loggerMock.Object);
        
        var project = await service.GetAsync(99, CancellationToken.None);
        
        project.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProjects()
    {
        var repoMock = new Mock<IProjectRepository>();
        var memberRepoMock = new Mock<IProjectMemberRepository>();
        var authServiceMock = new Mock<IProjectAuthorizationService>();
        var eventBusMock = new Mock<IEventBus>();
        var loggerMock = new Mock<ILogger<ProjectService>>();
        
        var projects = new List<ProjectModel>
        {
            new(1, "Project 1", "Desc 1", 1, "Active", 1, "Owner", false, DateTime.UtcNow),
            new(2, "Project 2", "Desc 2", 1, "Active", 2, "Owner 2", false, DateTime.UtcNow)
        };
        
        repoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(projects);
        
        var service = new ProjectService(repoMock.Object, memberRepoMock.Object, authServiceMock.Object, eventBusMock.Object, loggerMock.Object);
        
        var result = await service.GetAllAsync(CancellationToken.None);
        
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.IdProject == 1);
        result.Should().Contain(p => p.IdProject == 2);
    }

    [Fact]
    public async Task AddAsync_CreatesProjectAndAddsMemberAndInvalidatesCache()
    {
        var repoMock = new Mock<IProjectRepository>();
        var memberRepoMock = new Mock<IProjectMemberRepository>();
        var authServiceMock = new Mock<IProjectAuthorizationService>();
        var eventBusMock = new Mock<IEventBus>();
        var loggerMock = new Mock<ILogger<ProjectService>>();
        
        var request = new AddProjectRequest("New Project", "Description");
        var createdProject = new ProjectModel(1, "New Project", "Description", 1, "Active", 100, "Owner", false, DateTime.UtcNow);
        
        repoMock.Setup(x => x.AddAsync(request, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdProject);
        
        var service = new ProjectService(repoMock.Object, memberRepoMock.Object, authServiceMock.Object, eventBusMock.Object, loggerMock.Object);
        
        var result = await service.AddAsync(request, 100, CancellationToken.None);
        
        result.Should().NotBeNull();
        result.IdProject.Should().Be(1);
        
        memberRepoMock.Verify(x => x.AddMemberAsync(1, 100, "Owner", It.IsAny<CancellationToken>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
        
        authServiceMock.Verify(x => x.InvalidateUserProjectsCacheAsync(100, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CallsRepository()
    {
        var repoMock = new Mock<IProjectRepository>();
        var memberRepoMock = new Mock<IProjectMemberRepository>();
        var authServiceMock = new Mock<IProjectAuthorizationService>();
        var eventBusMock = new Mock<IEventBus>();
        var loggerMock = new Mock<ILogger<ProjectService>>();
        
        var request = new UpdateProjectRequest("Updated", "Updated Description", 2);
        
        repoMock.Setup(x => x.UpdateAsync(1, request, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        var service = new ProjectService(repoMock.Object, memberRepoMock.Object, authServiceMock.Object, eventBusMock.Object, loggerMock.Object);
        
        await service.UpdateAsync(1, request, CancellationToken.None);
        
        repoMock.Verify(x => x.UpdateAsync(1, request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CallsRepository()
    {
        var repoMock = new Mock<IProjectRepository>();
        var memberRepoMock = new Mock<IProjectMemberRepository>();
        var authServiceMock = new Mock<IProjectAuthorizationService>();
        var eventBusMock = new Mock<IEventBus>();
        var loggerMock = new Mock<ILogger<ProjectService>>();
        
        repoMock.Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        var service = new ProjectService(repoMock.Object, memberRepoMock.Object, authServiceMock.Object, eventBusMock.Object, loggerMock.Object);
        
        await service.DeleteAsync(1, CancellationToken.None);
        
        repoMock.Verify(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }
}
