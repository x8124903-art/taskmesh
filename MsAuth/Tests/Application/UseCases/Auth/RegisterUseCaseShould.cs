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

public sealed class RegisterUseCaseShould
{
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IJwtService> _jwtServiceMock = new();
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock = new();
    private readonly RegisterUseCase _useCase;

    public RegisterUseCaseShould()
    {
        _useCase = new RegisterUseCase(
            _userServiceMock.Object,
            _jwtServiceMock.Object,
            _refreshTokenServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsRegisterResponse_WithTokensAndUser()
    {
        var request = new RegisterRequest("test@demo.com", "John", "Pass123!");
        var user = new UserModel(1, "test@demo.com", "John", "hash", false, DateTime.UtcNow);

        _userServiceMock.Setup(x => x.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _jwtServiceMock.Setup(x => x.GenerateToken(user))
            .Returns("access-token");
        _refreshTokenServiceMock.Setup(x => x.AddAsync(It.IsAny<RefreshTokenModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _useCase.ExecuteAsync(request, CancellationToken.None);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.IdUser.Should().Be(1);
        result.User.Email.Should().Be("test@demo.com");
        result.User.Name.Should().Be("John");
    }

    [Fact]
    public async Task ExecuteAsync_CreatesRefreshToken_WithCorrectUserId()
    {
        var request = new RegisterRequest("test@demo.com", "John", "Pass123!");
        var user = new UserModel(1, "test@demo.com", "John", "hash", false, DateTime.UtcNow);

        _userServiceMock.Setup(x => x.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _jwtServiceMock.Setup(x => x.GenerateToken(user)).Returns("tok");
        _refreshTokenServiceMock.Setup(x => x.AddAsync(It.IsAny<RefreshTokenModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _useCase.ExecuteAsync(request, CancellationToken.None);

        _refreshTokenServiceMock.Verify(x => x.AddAsync(
            It.Is<RefreshTokenModel>(r => r.UserId == 1 && !r.IsRevoked),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesException_WhenDuplicateEmail()
    {
        var request = new RegisterRequest("exists@demo.com", "John", "Pass123!");
        _userServiceMock.Setup(x => x.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("User already exists"));

        var act = () => _useCase.ExecuteAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }
}
