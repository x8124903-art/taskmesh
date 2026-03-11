using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MsAuth.Application.Models;
using MsAuth.Application.UseCases.Auth;
using MsAuth.Domain.Services;
using Xunit;

namespace MsAuth.Tests.Application.UseCases.Auth;

public sealed class LoginUseCaseShould
{
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IJwtService> _jwtServiceMock = new();
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock = new();
    private readonly LoginUseCase _useCase;

    public LoginUseCaseShould()
    {
        _useCase = new LoginUseCase(
            _userServiceMock.Object,
            _jwtServiceMock.Object,
            _refreshTokenServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsLoginResponse_WhenCredentialsValid()
    {
        var request = new LoginRequest("test@demo.com", "Pass123!");
        var user = new UserModel(1, "test@demo.com", "John", "hash", false, DateTime.UtcNow);

        _userServiceMock.Setup(x => x.ValidateCredentialsAsync("test@demo.com", "Pass123!", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _jwtServiceMock.Setup(x => x.GenerateToken(user)).Returns("access-token");
        _refreshTokenServiceMock.Setup(x => x.AddAsync(It.IsAny<RefreshTokenModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _useCase.ExecuteAsync(request, CancellationToken.None);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Email.Should().Be("test@demo.com");
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsUnauthorized_WhenCredentialsInvalid()
    {
        var request = new LoginRequest("wrong@demo.com", "WrongPass");
        _userServiceMock.Setup(x => x.ValidateCredentialsAsync("wrong@demo.com", "WrongPass", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserModel?)null);

        var act = () => _useCase.ExecuteAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid credentials*");
    }

    [Fact]
    public async Task ExecuteAsync_CreatesRefreshToken_ForValidUser()
    {
        var request = new LoginRequest("test@demo.com", "Pass123!");
        var user = new UserModel(5, "test@demo.com", "John", "hash", false, DateTime.UtcNow);

        _userServiceMock.Setup(x => x.ValidateCredentialsAsync("test@demo.com", "Pass123!", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _jwtServiceMock.Setup(x => x.GenerateToken(user)).Returns("tok");
        _refreshTokenServiceMock.Setup(x => x.AddAsync(It.IsAny<RefreshTokenModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _useCase.ExecuteAsync(request, CancellationToken.None);

        _refreshTokenServiceMock.Verify(x => x.AddAsync(
            It.Is<RefreshTokenModel>(rt => rt.UserId == 5),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
