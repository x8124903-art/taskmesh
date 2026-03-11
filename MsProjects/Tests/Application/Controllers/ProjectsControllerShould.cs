using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Moq;
using MsProjects.Application.Controllers;
using MsProjects.Application.Models;
using MsProjects.Application.UseCases.Project;
using Xunit;

namespace MsProjects.Tests.Application.Controllers;

public sealed class ProjectsControllerShould
{
    private readonly Mock<IGetUserProjectsUseCase> _getUserProjectsMock = new();
    private readonly Mock<IGetProjectDetailsUseCase> _getProjectDetailsMock = new();
    private readonly Mock<ICreateProjectUseCase> _createProjectMock = new();
    private readonly Mock<IUpdateProjectUseCase> _updateProjectMock = new();
    private readonly Mock<IDeleteProjectUseCase> _deleteProjectMock = new();
    private readonly ProjectsController _controller;

    public ProjectsControllerShould()
    {
        _controller = new ProjectsController(
            _getUserProjectsMock.Object, _getProjectDetailsMock.Object,
            _createProjectMock.Object, _updateProjectMock.Object, _deleteProjectMock.Object);
        
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
    public async Task GetAll_ReturnsOk_WithProjects()
    {
        const int USER_ID = 1;
        SetUserIdHeader(USER_ID);

        var projects = new List<ProjectModel>
        {
            new(1, "Project 1", "Desc 1", 1, "Active", USER_ID, "Owner", false, DateTime.UtcNow),
            new(3, "Project 3", "Desc 3", 1, "Active", USER_ID, "Owner", false, DateTime.UtcNow)
        };

        _getUserProjectsMock.Setup(x => x.ExecuteAsync(USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(projects);

        var result = await _controller.GetAll(CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(projects);
    }

    [Fact]
    public async Task GetAsync_ReturnsOk_WhenProjectFound()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 100;
        SetUserIdHeader(USER_ID);

        var project = new ProjectModel(PROJECT_ID, "Test", "Desc", 1, "Active", USER_ID, "Owner", false, DateTime.UtcNow);
        _getProjectDetailsMock.Setup(x => x.ExecuteAsync(PROJECT_ID, USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var result = await _controller.GetAsync(PROJECT_ID, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(project);
    }

    [Fact]
    public async Task GetAsync_ReturnsNotFound_WhenUseCaseReturnsNull()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 999;
        SetUserIdHeader(USER_ID);

        _getProjectDetailsMock.Setup(x => x.ExecuteAsync(PROJECT_ID, USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProjectModel?)null);

        var result = await _controller.GetAsync(PROJECT_ID, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Add_ReturnsCreated_WithProject()
    {
        const int USER_ID = 1;
        SetUserIdHeader(USER_ID);

        var request = new AddProjectRequest("New Project", "Desc");
        var created = new ProjectModel(1, "New Project", "Desc", 1, "Active", USER_ID, "Owner", false, DateTime.UtcNow);

        _createProjectMock.Setup(x => x.ExecuteAsync(request, USER_ID, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var result = await _controller.Add(request, CancellationToken.None);

        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.Location.Should().Be("/api/v1.0/projects/1");
        createdResult.Value.Should().Be(created);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenSuccessful()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 100;
        SetUserIdHeader(USER_ID);

        var request = new UpdateProjectRequest("Updated", "Desc", 2);
        _updateProjectMock.Setup(x => x.ExecuteAsync(PROJECT_ID, request, USER_ID, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Update(PROJECT_ID, request, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccessful()
    {
        const int USER_ID = 1;
        const int PROJECT_ID = 100;
        SetUserIdHeader(USER_ID);

        _deleteProjectMock.Setup(x => x.ExecuteAsync(PROJECT_ID, USER_ID, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Delete(PROJECT_ID, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
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
