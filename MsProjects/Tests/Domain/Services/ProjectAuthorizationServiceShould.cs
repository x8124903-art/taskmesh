using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using MsProjects.Domain.Services.Authorization;
using MsProjects.Infrastructure.Repositories;
using Xunit;

namespace MsProjects.Tests.Domain.Services;

public sealed class ProjectAuthorizationServiceShould
{
    private readonly Mock<IProjectMemberRepository> _memberRepoMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly ProjectAuthorizationService _service;

    public ProjectAuthorizationServiceShould()
    {
        _memberRepoMock = new Mock<IProjectMemberRepository>();
        _cacheMock = new Mock<IDistributedCache>();
        _service = new ProjectAuthorizationService(_memberRepoMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task GetUserProjectRoleAsync_ReturnsRole_WhenUserIsMember()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 100;
        const string EXPECTED_ROLE = "Admin";

        _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        _memberRepoMock.Setup(x => x.GetUserRoleInProjectAsync(USER_ID, PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(EXPECTED_ROLE);

        var result = await _service.GetUserProjectRoleAsync(USER_ID, PROJECT_ID);

        result.Should().Be(EXPECTED_ROLE);
    }

    [Fact]
    public async Task IsProjectMemberAsync_ReturnsTrue_WhenUserHasRole()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 100;

        _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        _memberRepoMock.Setup(x => x.GetUserRoleInProjectAsync(USER_ID, PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Member");

        var result = await _service.IsProjectMemberAsync(USER_ID, PROJECT_ID);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasProjectRoleAsync_ReturnsTrue_WhenUserHasOwnerRole()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 100;

        _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        _memberRepoMock.Setup(x => x.GetUserRoleInProjectAsync(USER_ID, PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Owner");

        var result = await _service.HasProjectRoleAsync(USER_ID, PROJECT_ID, CancellationToken.None, "Owner", "Admin");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsProjectOwnerAsync_ReturnsTrue_WhenUserIsOwner()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 100;

        _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        _memberRepoMock.Setup(x => x.GetUserRoleInProjectAsync(USER_ID, PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Owner");

        var result = await _service.IsProjectOwnerAsync(USER_ID, PROJECT_ID);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetUserProjectIdsAsync_ReturnsListOfProjectIds()
    {
        const int USER_ID = 1;
        var expectedIds = new List<int> { 100, 101, 102 };

        _memberRepoMock.Setup(x => x.GetProjectIdsByUserIdAsync(USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedIds);

        var result = await _service.GetUserProjectIdsAsync(USER_ID);

        result.Should().BeEquivalentTo(expectedIds);
    }

    [Fact]
    public async Task IsProjectMemberAsync_ReturnsFalse_WhenUserHasNoRole()
    {
        _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);
        _memberRepoMock.Setup(x => x.GetUserRoleInProjectAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);

        var result = await _service.IsProjectMemberAsync(1, 1);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasProjectRoleAsync_ReturnsFalse_WhenUserHasDifferentRole()
    {
        _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);
        _memberRepoMock.Setup(x => x.GetUserRoleInProjectAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync("Viewer");

        var result = await _service.HasProjectRoleAsync(1, 1, CancellationToken.None, "Owner", "Admin");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsProjectOwnerAsync_ReturnsFalse_WhenUserIsNotOwner()
    {
        _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);
        _memberRepoMock.Setup(x => x.GetUserRoleInProjectAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync("Member");

        var result = await _service.IsProjectOwnerAsync(1, 1);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserProjectRoleAsync_ReturnsCachedValue_WhenCacheHit()
    {
        _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("Admin"));

        var result = await _service.GetUserProjectRoleAsync(1, 1);

        result.Should().Be("Admin");
        _memberRepoMock.Verify(x => x.GetUserRoleInProjectAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetUserProjectRoleAsync_ReturnsNull_WhenUserNotMember()
    {
        _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);
        _memberRepoMock.Setup(x => x.GetUserRoleInProjectAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);

        var result = await _service.GetUserProjectRoleAsync(1, 1);

        result.Should().BeNull();
    }

    [Fact]
    public async Task InvalidateUserProjectsCacheAsync_RemovesCacheEntry()
    {
        await _service.InvalidateUserProjectsCacheAsync(1);

        _cacheMock.Verify(x => x.RemoveAsync("user_projects:1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvalidateUserProjectRoleCacheAsync_RemovesCacheEntry()
    {
        await _service.InvalidateUserProjectRoleCacheAsync(1, 100);

        _cacheMock.Verify(x => x.RemoveAsync("project_role:1:100", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserProjectIdsAsync_ReturnsCachedValue_WhenCacheHit()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new List<int> { 1, 2, 3 });
        _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes(json));

        var result = await _service.GetUserProjectIdsAsync(1);

        result.Should().BeEquivalentTo(new List<int> { 1, 2, 3 });
        _memberRepoMock.Verify(x => x.GetProjectIdsByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
