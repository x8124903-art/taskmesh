using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MsAuth.Application.UseCases.Auth;
using MsAuth.Domain.Services;
using Xunit;

namespace MsAuth.Tests.Application.UseCases.Auth;

public sealed class LogoutUseCaseShould
{
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock = new();
    private readonly LogoutUseCase _useCase;

    public LogoutUseCaseShould()
    {
        _useCase = new LogoutUseCase(_refreshTokenServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_CallsRevokeAsync_WithCorrectToken()
    {
        _refreshTokenServiceMock.Setup(x => x.RevokeAsync("token-to-logout", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _useCase.ExecuteAsync("token-to-logout", CancellationToken.None);

        _refreshTokenServiceMock.Verify(x => x.RevokeAsync("token-to-logout", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotThrow_WhenServiceSucceeds()
    {
        _refreshTokenServiceMock.Setup(x => x.RevokeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var act = () => _useCase.ExecuteAsync("any-token", CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
