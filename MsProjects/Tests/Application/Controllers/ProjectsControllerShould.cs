using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using MsProjects.Application.Controllers;
using MsProjects.Application.Models;
using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;
using Xunit;

namespace MsProjects.Tests.Application.Controllers;

public sealed class ProjectsControllerShould
{
    private readonly Mock<IProjectService> _serviceMock;
    private readonly Mock<IProjectAuthorizationService> _authServiceMock;
    private readonly Mock<ILogger<ProjectsController>> _loggerMock;
    private readonly ProjectsController _controller;

    public ProjectsControllerShould()
    {
        _serviceMock = new Mock<IProjectService>();
        _authServiceMock = new Mock<IProjectAuthorizationService>();
        _loggerMock = new Mock<ILogger<ProjectsController>>();
        _controller = new ProjectsController(_serviceMock.Object, _authServiceMock.Object, _loggerMock.Object);
        
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
    public async Task GetAll_ReturnsFilteredProjects_WhenUserHasAccess()
    {
        const int USER_ID = 1;
        SetUserIdHeader(USER_ID);

        var allProjects = new List<ProjectModel>
        {
            new(1, "Project 1", "Desc 1", 1, "Active", USER_ID, "Owner", false, DateTime.UtcNow),
            new(2, "Project 2", "Desc 2", 1, "Active", 2, "Other Owner", false, DateTime.UtcNow),
            new(3, "Project 3", "Desc 3", 1, "Active", USER_ID, "Owner", false, DateTime.UtcNow)
        };

        var userProjectIds = new List<int> { 1, 3 };

        _authServiceMock.Setup(x => x.GetUserProjectIdsAsync(USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userProjectIds);

        _serviceMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(allProjects);

        var result = await _controller.GetAll(CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var projects = okResult.Value.Should().BeAssignableTo<IEnumerable<ProjectModel>>().Subject.ToList();
        projects.Should().HaveCount(2);
        projects.Should().Contain(p => p.IdProject == 1);
        projects.Should().Contain(p => p.IdProject == 3);
    }

    [Fact]
    public async Task Get_ReturnsForbidden_WhenUserIsNotMember()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 100;
        SetUserIdHeader(USER_ID);

        var existingProject = new ProjectModel(PROJECT_ID, "Existing Project", "Description", 1, "Active", 1, "Owner", false, DateTime.UtcNow);
        _serviceMock.Setup(x => x.GetAsync(PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProject);
        
        _authServiceMock.Setup(x => x.IsProjectMemberAsync(USER_ID, PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.GetAsync(PROJECT_ID, CancellationToken.None);

        result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task Update_ReturnsForbidden_WhenUserLacksOwnerOrAdminRole()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 100;
        SetUserIdHeader(USER_ID);

        var request = new UpdateProjectRequest("Updated Name", "Updated Desc", 1);
        var existingProject = new ProjectModel(PROJECT_ID, "Existing Project", "Description", 1, "Active", 1, "Owner", false, DateTime.UtcNow);
        
        _serviceMock.Setup(x => x.GetAsync(PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProject);

        _authServiceMock.Setup(x => x.HasProjectRoleAsync(
            USER_ID, PROJECT_ID, It.IsAny<CancellationToken>(), "Owner", "Admin"))
            .ReturnsAsync(false);

        var result = await _controller.Update(PROJECT_ID, request, CancellationToken.None);

        result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(403);
        _serviceMock.Verify(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateProjectRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_ReturnsForbidden_WhenUserIsNotOwner()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 100;
        SetUserIdHeader(USER_ID);

        var existingProject = new ProjectModel(PROJECT_ID, "Existing Project", "Description", 1, "Active", 1, "Owner", false, DateTime.UtcNow);
        _serviceMock.Setup(x => x.GetAsync(PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProject);

        _authServiceMock.Setup(x => x.IsProjectOwnerAsync(USER_ID, PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.Delete(PROJECT_ID, CancellationToken.None);

        result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(403);
        _serviceMock.Verify(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Get_ReturnsProject_WhenUserIsMember()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 100;
        SetUserIdHeader(USER_ID);

        var project = new ProjectModel(PROJECT_ID, "Test Project", "Description", 1, "Active", USER_ID, "Owner", false, DateTime.UtcNow);

        _authServiceMock.Setup(x => x.IsProjectMemberAsync(USER_ID, PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        _serviceMock.Setup(x => x.GetAsync(PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var result = await _controller.GetAsync(PROJECT_ID, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProject = okResult.Value.Should().BeAssignableTo<ProjectModel>().Subject;
        returnedProject.IdProject.Should().Be(PROJECT_ID);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenProjectDoesNotExist()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 999;
        SetUserIdHeader(USER_ID);

        _authServiceMock.Setup(x => x.IsProjectMemberAsync(USER_ID, PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        _serviceMock.Setup(x => x.GetAsync(PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProjectModel?)null);

        var result = await _controller.GetAsync(PROJECT_ID, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Add_CreatesProject_AndReturnsCreated()
    {
        const int USER_ID = 1;
        SetUserIdHeader(USER_ID);

        var request = new AddProjectRequest("New Project", "New Description");
        var createdProject = new ProjectModel(1, "New Project", "New Description", 1, "Active", USER_ID, "User", false, DateTime.UtcNow);

        _serviceMock.Setup(x => x.AddAsync(request, USER_ID, It.IsAny<CancellationToken>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(createdProject);

        var result = await _controller.Add(request, CancellationToken.None);

        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.Location.Should().Be("/api/v1.0/projects/1");
        var returnedProject = createdResult.Value.Should().BeAssignableTo<ProjectModel>().Subject;
        returnedProject.IdProject.Should().Be(1);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenUserHasOwnerRole()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 100;
        SetUserIdHeader(USER_ID);

        var request = new UpdateProjectRequest("Updated Name", "Updated Desc", 2);
        var existingProject = new ProjectModel(PROJECT_ID, "Existing Project", "Description", 1, "Active", 1, "Owner", false, DateTime.UtcNow);
        
        _serviceMock.Setup(x => x.GetAsync(PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProject);

        _authServiceMock.Setup(x => x.HasProjectRoleAsync(
            USER_ID, PROJECT_ID, It.IsAny<CancellationToken>(), "Owner", "Admin"))
            .ReturnsAsync(true);

        _serviceMock.Setup(x => x.UpdateAsync(PROJECT_ID, request, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Update(PROJECT_ID, request, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(x => x.UpdateAsync(PROJECT_ID, request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenUserIsOwner()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 100;
        SetUserIdHeader(USER_ID);

        var existingProject = new ProjectModel(PROJECT_ID, "Existing Project", "Description", 1, "Active", 1, "Owner", false, DateTime.UtcNow);
        _serviceMock.Setup(x => x.GetAsync(PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProject);

        _authServiceMock.Setup(x => x.IsProjectOwnerAsync(USER_ID, PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _serviceMock.Setup(x => x.DeleteAsync(PROJECT_ID, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Delete(PROJECT_ID, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(x => x.DeleteAsync(PROJECT_ID, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenProjectDoesNotExist()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 999;
        SetUserIdHeader(USER_ID);

        var request = new UpdateProjectRequest("Updated", "Updated Desc", 1);
        _serviceMock.Setup(x => x.GetAsync(PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProjectModel?)null);

        var result = await _controller.Update(PROJECT_ID, request, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenProjectDoesNotExist()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 999;
        SetUserIdHeader(USER_ID);

        _serviceMock.Setup(x => x.GetAsync(PROJECT_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProjectModel?)null);

        var result = await _controller.Delete(PROJECT_ID, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetAll_ThrowsUnauthorizedAccessException_WhenNoUserIdHeader()
    {

        Func<Task> act = () => _controller.GetAll(CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetAll_ThrowsArgumentException_WhenUserIdHeaderInvalid()
    {
        _controller.ControllerContext.HttpContext.Request.Headers["X-User-Id"] = new StringValues("invalid");

        Func<Task> act = () => _controller.GetAll(CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
