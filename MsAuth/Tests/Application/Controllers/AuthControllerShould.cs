using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using MsAuth.Application.Controllers;
using MsAuth.Application.Controllers.Auth;
using MsAuth.Application.Models;
using MsAuth.Application.UseCases.Auth;

namespace MsAuth.Tests.Application.Controllers;

public sealed class AuthControllerShould
{
    private readonly Mock<IRegisterUseCase> _registerMock = new();
    private readonly Mock<ILoginUseCase> _loginMock = new();
    private readonly Mock<IRefreshTokenUseCase> _refreshMock = new();
    private readonly Mock<ILogoutUseCase> _logoutMock = new();
    private readonly AuthController _controller;

    public AuthControllerShould()
    {
        _controller = new AuthController(
            _registerMock.Object, _loginMock.Object,
            _refreshMock.Object, _logoutMock.Object);
    }

    [Fact]
    public async Task Register_ReturnsCreated_WhenUseCaseSucceeds()
    {
        var request = new RegisterRequest("test@demo.com", "John", "Pass123!");
        var response = new RegisterResponse("access-token", "refresh-token",
            new UserResponse(1, "test@demo.com", "John", DateTime.UtcNow));
        _registerMock.Setup(x => x.ExecuteAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.Register(request, CancellationToken.None);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(201);
        objectResult.Value.Should().Be(response);
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenUseCaseSucceeds()
    {
        var request = new LoginRequest("test@demo.com", "Pass123!");
        var response = new LoginResponse("access-token", "refresh-token",
            new UserResponse(1, "test@demo.com", "John", DateTime.UtcNow));
        _loginMock.Setup(x => x.ExecuteAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.Login(request, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().Be(response);
    }

    [Fact]
    public async Task Refresh_ReturnsOkWithAccessToken_WhenUseCaseSucceeds()
    {
        var request = new RefreshTokenRequest("valid-refresh-token");
        _refreshMock.Setup(x => x.ExecuteAsync("valid-refresh-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync("new-access-token");

        var result = await _controller.Refresh(request, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Logout_ReturnsNoContent_WhenUseCaseSucceeds()
    {
        var request = new LogoutRequest("token-to-logout");
        _logoutMock.Setup(x => x.ExecuteAsync("token-to-logout", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Logout(request, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _logoutMock.Verify(x => x.ExecuteAsync("token-to-logout", It.IsAny<CancellationToken>()), Times.Once);
    }
}
