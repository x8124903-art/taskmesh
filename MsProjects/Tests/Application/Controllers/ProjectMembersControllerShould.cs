using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Moq;
using MsProjects.Application.Controllers;
using MsProjects.Application.Models;
using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;
using Xunit;

namespace MsProjects.Tests.Application.Controllers;

public sealed class ProjectMembersControllerShould
{
    private readonly Mock<IProjectMemberService> _serviceMock;
    private readonly Mock<IProjectAuthorizationService> _authServiceMock;
    private readonly ProjectMembersController _controller;

    public ProjectMembersControllerShould()
    {
        _serviceMock = new Mock<IProjectMemberService>();
        _authServiceMock = new Mock<IProjectAuthorizationService>();
        _controller = new ProjectMembersController(_serviceMock.Object, _authServiceMock.Object);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    private void SetUserIdHeader(int userId)
    {
        _controller.ControllerContext.HttpContext.Request.Headers["X-User-Id"] = new StringValues(userId.ToString());
    }

    [Fact]
    public async Task GetMembers_ReturnsOk_WhenUserIsMember()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 100;
        SetUserIdHeader(USER_ID);

        var members = new List<ProjectMemberModel>
        {
            new(1, PROJECT_ID, USER_ID, "Test", "Test@test.com", 1, "Owner", DateTime.UtcNow, DateTime.UtcNow),
            new(2, PROJECT_ID, 2, "Test2", "Test2@test.com", 3, "Member", DateTime.UtcNow, DateTime.UtcNow)
        };

        _authServiceMock.Setup(x => x.IsProjectMemberAsync(USER_ID, PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _serviceMock.Setup(x => x.GetMembersAsync(PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(members);

        var result = await _controller.GetMembers(PROJECT_ID, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedMembers = okResult.Value.Should().BeAssignableTo<IEnumerable<ProjectMemberModel>>().Subject;
        returnedMembers.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMembers_ReturnsForbidden_WhenUserIsNotMember()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 100;
        SetUserIdHeader(USER_ID);

        _authServiceMock.Setup(x => x.IsProjectMemberAsync(USER_ID, PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.GetMembers(PROJECT_ID, CancellationToken.None);

        result.Should().BeOfType<ForbidResult>();
        _serviceMock.Verify(x => x.GetMembersAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ChangeRole_ReturnsForbidden_WhenUserLacksOwnerOrAdminRole()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 100;
        const int TARGET_USER_ID = 5;
        SetUserIdHeader(USER_ID);

        var request = new ChangeRoleRequest("Admin");

        _authServiceMock.Setup(x => x.HasProjectRoleAsync(
            USER_ID, PROJECT_ID, It.IsAny<CancellationToken>(), "Owner", "Admin"))
            .ReturnsAsync(false);

        var result = await _controller.ChangeRole(PROJECT_ID, TARGET_USER_ID, request, CancellationToken.None);

        result.Should().BeOfType<ForbidResult>();
        _serviceMock.Verify(x => x.ChangeRoleAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveMember_ReturnsForbidden_WhenUserLacksOwnerOrAdminRole()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 100;
        const int TARGET_USER_ID = 5;
        SetUserIdHeader(USER_ID);

        _authServiceMock.Setup(x => x.HasProjectRoleAsync(
            USER_ID, PROJECT_ID, It.IsAny<CancellationToken>(), "Owner", "Admin"))
            .ReturnsAsync(false);

        var result = await _controller.RemoveMember(PROJECT_ID, TARGET_USER_ID, CancellationToken.None);

        result.Should().BeOfType<ForbidResult>();
        _serviceMock.Verify(x => x.RemoveMemberAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ChangeRole_ReturnsNoContent_WhenUserIsAdminOrOwner()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 100;
        const int TARGET_USER_ID = 5;
        SetUserIdHeader(USER_ID);

        var request = new ChangeRoleRequest("Member");

        _authServiceMock.Setup(x => x.HasProjectRoleAsync(
            USER_ID, PROJECT_ID, It.IsAny<CancellationToken>(), "Owner", "Admin"))
            .ReturnsAsync(true);

        _serviceMock.Setup(x => x.ChangeRoleAsync(PROJECT_ID, TARGET_USER_ID, "Member", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.ChangeRole(PROJECT_ID, TARGET_USER_ID, request, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(x => x.ChangeRoleAsync(PROJECT_ID, TARGET_USER_ID, "Member", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveMember_ReturnsNoContent_WhenUserIsAdminOrOwner()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 100;
        const int TARGET_USER_ID = 5;
        SetUserIdHeader(USER_ID);

        _authServiceMock.Setup(x => x.HasProjectRoleAsync(
            USER_ID, PROJECT_ID, It.IsAny<CancellationToken>(), "Owner", "Admin"))
            .ReturnsAsync(true);

        _serviceMock.Setup(x => x.RemoveMemberAsync(PROJECT_ID, TARGET_USER_ID, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.RemoveMember(PROJECT_ID, TARGET_USER_ID, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(x => x.RemoveMemberAsync(PROJECT_ID, TARGET_USER_ID, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMembers_ThrowsUnauthorizedAccessException_WhenNoUserIdHeader()
    {
        const int PROJECT_ID = 100;

        Func<Task> act = () => _controller.GetMembers(PROJECT_ID, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetMembers_ThrowsArgumentException_WhenUserIdHeaderInvalid()
    {
        const int PROJECT_ID = 100;
        _controller.ControllerContext.HttpContext.Request.Headers["X-User-Id"] = new StringValues("not-a-number");

        Func<Task> act = () => _controller.GetMembers(PROJECT_ID, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
