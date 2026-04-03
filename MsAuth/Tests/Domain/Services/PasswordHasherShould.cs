using Xunit;
using FluentAssertions;
using MsAuth.Domain.Services;

namespace MsAuth.Tests.Domain.Services;

public sealed class PasswordHasherShould
{
    [Fact]
    public void HashPassword_WithValidPassword_ReturnsHashedString()
    {
        var hasher = new PasswordHasher();
        const string PASSWORD = "SecurePassword123!";

        var hash = hasher.HashPassword(PASSWORD);

        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(PASSWORD);
        hash.Should().StartWith("$2");
    }

    [Fact]
    public void HashPassword_CalledTwice_GeneratesDifferentHashes()
    {
        var hasher = new PasswordHasher();
        const string PASSWORD = "SecurePassword123!";

        var hash1 = hasher.HashPassword(PASSWORD);
        var hash2 = hasher.HashPassword(PASSWORD);

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        var hasher = new PasswordHasher();
        const string PASSWORD = "SecurePassword123!";
        var hash = hasher.HashPassword(PASSWORD);

        var result = hasher.VerifyPassword(PASSWORD, hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        var hasher = new PasswordHasher();
        const string PASSWORD = "SecurePassword123!";
        const string WRONG_PASSWORD = "WrongPassword456!";
        var hash = hasher.HashPassword(PASSWORD);

        var result = hasher.VerifyPassword(WRONG_PASSWORD, hash);

        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithEmptyPassword_ReturnsFalse()
    {
        var hasher = new PasswordHasher();
        const string PASSWORD = "SecurePassword123!";
        var hash = hasher.HashPassword(PASSWORD);

        var result = hasher.VerifyPassword("", hash);

        result.Should().BeFalse();
    }
}
