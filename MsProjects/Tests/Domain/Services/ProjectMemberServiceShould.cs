using FluentAssertions;
using MsProjects.Application.Models;
using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;
using MsProjects.Infrastructure.Repositories;

namespace MsProjects.Tests.Domain.Services;

public sealed class ProjectMemberServiceShould
{
    [Fact]
    public async Task GetMembersAsync_WithValidProjectId_ReturnsMembers()
    {
        var repositoryMock = new Mock<IProjectMemberRepository>();
        var authServiceMock = new Mock<IProjectAuthorizationService>();
        
        var members = new List<ProjectMemberModel>
        {
            new ProjectMemberModel(1, 1, 100, "User One", "user1@demo.com", 1, "Owner", DateTime.UtcNow, DateTime.UtcNow),
            new ProjectMemberModel(2, 1, 101, "User Two", "user2@demo.com", 3, "Member", DateTime.UtcNow, DateTime.UtcNow)
        };
        
        repositoryMock.Setup(x => x.GetMembersAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(members);
        
        var service = new ProjectMemberService(repositoryMock.Object, authServiceMock.Object);
        
        var result = await service.GetMembersAsync(1);
        
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().ProjectId.Should().Be(1);
    }

    [Fact]
    public async Task ChangeRoleAsync_WithValidData_ChangesRoleAndInvalidatesCache()
    {
        var repositoryMock = new Mock<IProjectMemberRepository>();
        var authServiceMock = new Mock<IProjectAuthorizationService>();
        
        repositoryMock.Setup(x => x.ChangeRoleAsync(1, 100, "Admin", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        authServiceMock.Setup(x => x.InvalidateUserProjectRoleCacheAsync(100, 1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        var service = new ProjectMemberService(repositoryMock.Object, authServiceMock.Object);
        
        await service.ChangeRoleAsync(1, 100, "Admin");
        
        repositoryMock.Verify(x => x.ChangeRoleAsync(1, 100, "Admin", It.IsAny<CancellationToken>()), Times.Once);
        authServiceMock.Verify(x => x.InvalidateUserProjectRoleCacheAsync(100, 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveMemberAsync_WithValidData_RemovesMemberAndInvalidatesCache()
    {
        var repositoryMock = new Mock<IProjectMemberRepository>();
        var authServiceMock = new Mock<IProjectAuthorizationService>();
        
        repositoryMock.Setup(x => x.RemoveMemberAsync(1, 100, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        authServiceMock.Setup(x => x.InvalidateUserProjectsCacheAsync(100, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        var service = new ProjectMemberService(repositoryMock.Object, authServiceMock.Object);
        
        await service.RemoveMemberAsync(1, 100);
        
        repositoryMock.Verify(x => x.RemoveMemberAsync(1, 100, It.IsAny<CancellationToken>()), Times.Once);
        authServiceMock.Verify(x => x.InvalidateUserProjectsCacheAsync(100, It.IsAny<CancellationToken>()), Times.Once);
    }
}
