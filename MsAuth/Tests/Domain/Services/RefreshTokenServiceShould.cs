using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using MsAuth.Domain.Services;
using MsAuth.Application.Models;
using MsAuth.Infrastructure.Repositories;

namespace MsAuth.Tests.Domain.Services;

public sealed class RefreshTokenServiceShould
{
    [Fact]
    public async Task GetByTokenAsync_WithValidToken_ReturnsRefreshToken()
    {
        const string TOKEN = "valid-refresh-token";
        var expectedToken = new RefreshTokenModel(1, TOKEN, 123, DateTime.UtcNow.AddDays(7), false, DateTime.UtcNow, null);
        
        var repoMock = new Mock<IRefreshTokenRepository>();
        repoMock.Setup(x => x.GetByTokenAsync(TOKEN, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedToken);
        
        var service = new RefreshTokenService(repoMock.Object);

        var result = await service.GetByTokenAsync(TOKEN);

        result.Should().NotBeNull();
        result!.Token.Should().Be(TOKEN);
        result.UserId.Should().Be(123);
        repoMock.Verify(x => x.GetByTokenAsync(TOKEN, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByTokenAsync_WithInvalidToken_ReturnsNull()
    {
        const string TOKEN = "invalid-token";
        
        var repoMock = new Mock<IRefreshTokenRepository>();
        repoMock.Setup(x => x.GetByTokenAsync(TOKEN, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshTokenModel?)null);
        
        var service = new RefreshTokenService(repoMock.Object);

        var result = await service.GetByTokenAsync(TOKEN);

        result.Should().BeNull();
        repoMock.Verify(x => x.GetByTokenAsync(TOKEN, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_WithValidModel_ReturnsGeneratedId()
    {
        const int EXPECTED_ID = 42;
        var model = new RefreshTokenModel(0, "new-token", 123, DateTime.UtcNow.AddDays(7), false, DateTime.UtcNow, null);
        
        var repoMock = new Mock<IRefreshTokenRepository>();
        repoMock.Setup(x => x.AddAsync(model, It.IsAny<CancellationToken>()))
            .ReturnsAsync(EXPECTED_ID);
        
        var service = new RefreshTokenService(repoMock.Object);

        var result = await service.AddAsync(model);

        result.Should().Be(EXPECTED_ID);
        repoMock.Verify(x => x.AddAsync(model, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeAsync_WithValidToken_CompletesSuccessfully()
    {
        const string TOKEN = "token-to-revoke";
        
        var repoMock = new Mock<IRefreshTokenRepository>();
        repoMock.Setup(x => x.RevokeAsync(TOKEN, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        var service = new RefreshTokenService(repoMock.Object);

        await service.RevokeAsync(TOKEN);

        repoMock.Verify(x => x.RevokeAsync(TOKEN, It.IsAny<CancellationToken>()), Times.Once);
    }
}
