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

public sealed class RefreshTokenUseCaseShould
{
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IJwtService> _jwtServiceMock = new();
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock = new();
    private readonly RefreshTokenUseCase _useCase;

    public RefreshTokenUseCaseShould()
    {
        _useCase = new RefreshTokenUseCase(
            _userServiceMock.Object,
            _jwtServiceMock.Object,
            _refreshTokenServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNewAccessToken_WhenRefreshTokenValid()
    {
        var token = new RefreshTokenModel(1, "valid-token", 1, DateTime.UtcNow.AddDays(7), false, DateTime.UtcNow, null);
        var user = new UserModel(1, "test@demo.com", "John", "hash", false, DateTime.UtcNow);

        _refreshTokenServiceMock.Setup(x => x.GetByTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);
        _userServiceMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _jwtServiceMock.Setup(x => x.GenerateToken(user)).Returns("new-access-token");

        var result = await _useCase.ExecuteAsync("valid-token", CancellationToken.None);

        result.Should().Be("new-access-token");
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsUnauthorized_WhenTokenNotFound()
    {
        _refreshTokenServiceMock.Setup(x => x.GetByTokenAsync("unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshTokenModel?)null);

        var act = () => _useCase.ExecuteAsync("unknown", CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsUnauthorized_WhenTokenRevoked()
    {
        var token = new RefreshTokenModel(1, "revoked", 1, DateTime.UtcNow.AddDays(7), true, DateTime.UtcNow, DateTime.UtcNow);
        _refreshTokenServiceMock.Setup(x => x.GetByTokenAsync("revoked", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        var act = () => _useCase.ExecuteAsync("revoked", CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsUnauthorized_WhenTokenExpired()
    {
        var token = new RefreshTokenModel(1, "expired", 1, DateTime.UtcNow.AddDays(-1), false, DateTime.UtcNow.AddDays(-8), null);
        _refreshTokenServiceMock.Setup(x => x.GetByTokenAsync("expired", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        var act = () => _useCase.ExecuteAsync("expired", CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsUnauthorized_WhenUserNotFound()
    {
        var token = new RefreshTokenModel(1, "tok", 999, DateTime.UtcNow.AddDays(7), false, DateTime.UtcNow, null);
        _refreshTokenServiceMock.Setup(x => x.GetByTokenAsync("tok", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);
        _userServiceMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserModel?)null);

        var act = () => _useCase.ExecuteAsync("tok", CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
