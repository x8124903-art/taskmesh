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
using MsProjects.Application.UseCases.ProjectMember;
using Xunit;

namespace MsProjects.Tests.Application.Controllers;

public sealed class ProjectMembersControllerShould
{
    private readonly Mock<IGetProjectMembersUseCase> _getMembersMock = new();
    private readonly Mock<IChangeProjectMemberRoleUseCase> _changeRoleMock = new();
    private readonly Mock<IRemoveProjectMemberUseCase> _removeMemberMock = new();
    private readonly ProjectMembersController _controller;

    public ProjectMembersControllerShould()
    {
        _controller = new ProjectMembersController(
            _getMembersMock.Object, _changeRoleMock.Object, _removeMemberMock.Object);
        
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
    public async Task GetMembers_ReturnsOk_WhenUseCaseSucceeds()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 100;
        SetUserIdHeader(USER_ID);

        var members = new List<ProjectMemberModel>
        {
            new(1, PROJECT_ID, USER_ID, "Test", "Test@test.com", 1, "Owner", DateTime.UtcNow, DateTime.UtcNow),
            new(2, PROJECT_ID, 2, "Test2", "Test2@test.com", 3, "Member", DateTime.UtcNow, DateTime.UtcNow)
        };

        _getMembersMock.Setup(x => x.ExecuteAsync(PROJECT_ID, USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(members);

        var result = await _controller.GetMembers(PROJECT_ID, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(members);
    }

    [Fact]
    public async Task ChangeRole_ReturnsNoContent_WhenSuccessful()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 100;
        const int TARGET_USER_ID = 5;
        SetUserIdHeader(USER_ID);

        var request = new ChangeRoleRequest("Member");
        _changeRoleMock.Setup(x => x.ExecuteAsync(PROJECT_ID, TARGET_USER_ID, "Member", USER_ID, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.ChangeRole(PROJECT_ID, TARGET_USER_ID, request, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task RemoveMember_ReturnsNoContent_WhenSuccessful()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 100;
        const int TARGET_USER_ID = 5;
        SetUserIdHeader(USER_ID);

        _removeMemberMock.Setup(x => x.ExecuteAsync(PROJECT_ID, TARGET_USER_ID, USER_ID, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.RemoveMember(PROJECT_ID, TARGET_USER_ID, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
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
