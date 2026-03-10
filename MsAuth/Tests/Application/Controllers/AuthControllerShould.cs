using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using MsAuth.Application.Controllers;
using MsAuth.Application.Models;
using MsAuth.Domain.Services;

namespace MsAuth.Tests.Application.Controllers;

public sealed class AuthControllerShould
{
    [Fact]
    public async Task Register_WithValidRequest_ReturnsOkWithTokens()
    {
        var userServiceMock = new Mock<IUserService>();
        var jwtServiceMock = new Mock<IJwtService>();
        var refreshTokenServiceMock = new Mock<IRefreshTokenService>();

       var user = new UserModel(1, "test@demo.com", "John", "hash", false, DateTime.UtcNow);
        var registerRequest = new RegisterRequest("test@demo.com", "John Doe", "Password123!");

        userServiceMock.Setup(x => x.RegisterAsync(registerRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        jwtServiceMock.Setup(x => x.GenerateToken(user))
            .Returns("access-token");
        refreshTokenServiceMock.Setup(x => x.AddAsync(It.IsAny<RefreshTokenModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var controller = new AuthController(userServiceMock.Object, jwtServiceMock.Object, refreshTokenServiceMock.Object);

        var result = await controller.Register(registerRequest, CancellationToken.None);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(201);
        objectResult.Value.Should().BeOfType<RegisterResponse>();
        var response = (RegisterResponse)objectResult.Value!;
        response.AccessToken.Should().Be("access-token");
        response.RefreshToken.Should().NotBeNullOrEmpty();
        response.User.Email.Should().Be("test@demo.com");
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithTokens()
    {
        var userServiceMock = new Mock<IUserService>();
        var jwtServiceMock = new Mock<IJwtService>();
        var refreshTokenServiceMock = new Mock<IRefreshTokenService>();

        var user = new UserModel(1, "test@demo.com", "John", "hash", false, DateTime.UtcNow);
        var loginRequest = new LoginRequest("test@demo.com", "Password123!");

        userServiceMock.Setup(x => x.ValidateCredentialsAsync("test@demo.com", "Password123!", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        jwtServiceMock.Setup(x => x.GenerateToken(user))
            .Returns("access-token");
        refreshTokenServiceMock.Setup(x => x.AddAsync(It.IsAny<RefreshTokenModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var controller = new AuthController(userServiceMock.Object, jwtServiceMock.Object, refreshTokenServiceMock.Object);

        var result = await controller.Login(loginRequest, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeOfType<LoginResponse>();
        var response = (LoginResponse)okResult.Value!;
        response.AccessToken.Should().Be("access-token");
        response.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var userServiceMock = new Mock<IUserService>();
        var jwtServiceMock = new Mock<IJwtService>();
        var refreshTokenServiceMock = new Mock<IRefreshTokenService>();

        var loginRequest = new LoginRequest("wrong@demo.com", "WrongPassword");

        userServiceMock.Setup(x => x.ValidateCredentialsAsync("wrong@demo.com", "WrongPassword", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserModel?)null);

        var controller = new AuthController(userServiceMock.Object, jwtServiceMock.Object, refreshTokenServiceMock.Object);

        var result = await controller.Login(loginRequest, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewAccessToken()
    {
        var userServiceMock = new Mock<IUserService>();
        var jwtServiceMock = new Mock<IJwtService>();
        var refreshTokenServiceMock = new Mock<IRefreshTokenService>();

        var user = new UserModel(1, "test@demo.com", "John", "hash", false, DateTime.UtcNow);
        var refreshToken = new RefreshTokenModel(1, "valid-refresh-token", 1, DateTime.UtcNow.AddDays(7), false, DateTime.UtcNow, null);
        var refreshRequest = new MsAuth.Application.Controllers.Auth.RefreshTokenRequest("valid-refresh-token");

        refreshTokenServiceMock.Setup(x => x.GetByTokenAsync("valid-refresh-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);
        userServiceMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        jwtServiceMock.Setup(x => x.GenerateToken(user))
            .Returns("new-access-token");

        var controller = new AuthController(userServiceMock.Object, jwtServiceMock.Object, refreshTokenServiceMock.Object);

        var result = await controller.Refresh(refreshRequest, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Refresh_WithRevokedToken_ReturnsUnauthorized()
    {
        var userServiceMock = new Mock<IUserService>();
        var jwtServiceMock = new Mock<IJwtService>();
        var refreshTokenServiceMock = new Mock<IRefreshTokenService>();

        var refreshToken = new RefreshTokenModel(1, "revoked-token", 1, DateTime.UtcNow.AddDays(7), true, DateTime.UtcNow, DateTime.UtcNow);
        var refreshRequest = new MsAuth.Application.Controllers.Auth.RefreshTokenRequest("revoked-token");

        refreshTokenServiceMock.Setup(x => x.GetByTokenAsync("revoked-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        var controller = new AuthController(userServiceMock.Object, jwtServiceMock.Object, refreshTokenServiceMock.Object);

        var result = await controller.Refresh(refreshRequest, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Logout_WithValidToken_ReturnsNoContent()
    {
        var userServiceMock = new Mock<IUserService>();
        var jwtServiceMock = new Mock<IJwtService>();
        var refreshTokenServiceMock = new Mock<IRefreshTokenService>();

        var logoutRequest = new MsAuth.Application.Controllers.Auth.LogoutRequest("token-to-logout");

        refreshTokenServiceMock.Setup(x => x.RevokeAsync("token-to-logout", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var controller = new AuthController(userServiceMock.Object, jwtServiceMock.Object, refreshTokenServiceMock.Object);

        var result = await controller.Logout(logoutRequest, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        refreshTokenServiceMock.Verify(x => x.RevokeAsync("token-to-logout", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        var userServiceMock = new Mock<IUserService>();
        var jwtServiceMock = new Mock<IJwtService>();
        var refreshTokenServiceMock = new Mock<IRefreshTokenService>();

        var request = new RegisterRequest("exists@demo.com", "John", "Pass123!");
        userServiceMock.Setup(x => x.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("User with email exists@demo.com already exists"));

        var controller = new AuthController(userServiceMock.Object, jwtServiceMock.Object, refreshTokenServiceMock.Object);

        var result = await controller.Register(request, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Refresh_WithExpiredToken_ReturnsUnauthorized()
    {
        var userServiceMock = new Mock<IUserService>();
        var jwtServiceMock = new Mock<IJwtService>();
        var refreshTokenServiceMock = new Mock<IRefreshTokenService>();

        var expiredToken = new RefreshTokenModel(1, "expired-token", 1, DateTime.UtcNow.AddDays(-1), false, DateTime.UtcNow.AddDays(-8), null);
        var request = new MsAuth.Application.Controllers.Auth.RefreshTokenRequest("expired-token");

        refreshTokenServiceMock.Setup(x => x.GetByTokenAsync("expired-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredToken);

        var controller = new AuthController(userServiceMock.Object, jwtServiceMock.Object, refreshTokenServiceMock.Object);

        var result = await controller.Refresh(request, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Refresh_WithNullToken_ReturnsUnauthorized()
    {
        var userServiceMock = new Mock<IUserService>();
        var jwtServiceMock = new Mock<IJwtService>();
        var refreshTokenServiceMock = new Mock<IRefreshTokenService>();

        var request = new MsAuth.Application.Controllers.Auth.RefreshTokenRequest("unknown-token");
        refreshTokenServiceMock.Setup(x => x.GetByTokenAsync("unknown-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshTokenModel?)null);

        var controller = new AuthController(userServiceMock.Object, jwtServiceMock.Object, refreshTokenServiceMock.Object);

        var result = await controller.Refresh(request, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Refresh_WithUserNotFound_ReturnsUnauthorized()
    {
        var userServiceMock = new Mock<IUserService>();
        var jwtServiceMock = new Mock<IJwtService>();
        var refreshTokenServiceMock = new Mock<IRefreshTokenService>();

        var refreshToken = new RefreshTokenModel(1, "tok", 999, DateTime.UtcNow.AddDays(7), false, DateTime.UtcNow, null);
        var request = new MsAuth.Application.Controllers.Auth.RefreshTokenRequest("tok");

        refreshTokenServiceMock.Setup(x => x.GetByTokenAsync("tok", It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);
        userServiceMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserModel?)null);

        var controller = new AuthController(userServiceMock.Object, jwtServiceMock.Object, refreshTokenServiceMock.Object);

        var result = await controller.Refresh(request, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }
}
