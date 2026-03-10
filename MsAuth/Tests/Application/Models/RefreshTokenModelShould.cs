using FluentAssertions;
using MsAuth.Application.Models;

namespace MsAuth.Tests.Application.Models;

public sealed class RefreshTokenModelShould
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddDays(7);
        
        var model = new RefreshTokenModel(1, "token123", 100, expiresAt, false, now, null);
        
        model.IdRefreshToken.Should().Be(1);
        model.Token.Should().Be("token123");
        model.UserId.Should().Be(100);
        model.ExpiresAt.Should().Be(expiresAt);
        model.IsRevoked.Should().BeFalse();
        model.CreatedAt.Should().Be(now);
        model.RevokedAt.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithRevokedToken_SetsRevokedAtCorrectly()
    {
        var now = DateTime.UtcNow;
        var revokedAt = now.AddHours(1);
        
        var model = new RefreshTokenModel(2, "revoked_token", 200, now.AddDays(7), true, now, revokedAt);
        
        model.IsRevoked.Should().BeTrue();
        model.RevokedAt.Should().Be(revokedAt);
    }

    [Fact]
    public void RecordEquality_WithSameValues_ReturnsTrue()
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddDays(7);
        var model1 = new RefreshTokenModel(1, "token", 100, expiresAt, false, now, null);
        var model2 = new RefreshTokenModel(1, "token", 100, expiresAt, false, now, null);
        
        model1.Should().Be(model2);
    }

    [Fact]
    public void RecordEquality_WithDifferentValues_ReturnsFalse()
    {
        var now = DateTime.UtcNow;
        var model1 = new RefreshTokenModel(1, "token1", 100, now.AddDays(7), false, now, null);
        var model2 = new RefreshTokenModel(2, "token2", 200, now.AddDays(7), false, now, null);
        
        model1.Should().NotBe(model2);
    }
}
