using System;
using Xunit;
using FluentAssertions;
using MsAuth.Domain.Services;
using MsAuth.Application.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace MsAuth.Tests.Domain.Services;

public sealed class JwtServiceShould
{
    private const string TEST_SECRET_KEY = "this-is-a-very-secure-secret-key-for-testing-purposes-with-at-least-256-bits";
    private const string TEST_ISSUER = "test-taskmesh-auth";
    private const string TEST_AUDIENCE = "test-taskmesh-api";
    private const int TEST_EXPIRATION_MINUTES = 15;

    [Fact]
    public void GenerateToken_WithValidUser_ReturnsJwtToken()
    {
        var jwtService = new JwtService(TEST_SECRET_KEY, TEST_ISSUER, TEST_AUDIENCE, TEST_EXPIRATION_MINUTES);
        var user = new UserModel(1, "test@demo.com", "John", "hash", false, DateTime.UtcNow);

        var token = jwtService.GenerateToken(user);

        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
    }

    [Fact]
    public void GenerateToken_WithValidUser_ContainsCorrectClaims()
    {
        var jwtService = new JwtService(TEST_SECRET_KEY, TEST_ISSUER, TEST_AUDIENCE, TEST_EXPIRATION_MINUTES);
        var user = new UserModel(123, "john.doe@demo.com", "John", "hash", false, DateTime.UtcNow);

        var token = jwtService.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value.Should().Be("123");
        jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value.Should().Be("john.doe@demo.com");
        jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Name).Value.Should().Be("John");
        jwtToken.Issuer.Should().Be(TEST_ISSUER);
        jwtToken.Audiences.Should().Contain(TEST_AUDIENCE);
    }

    [Fact]
    public void GenerateToken_WithValidUser_HasCorrectExpiration()
    {
        var jwtService = new JwtService(TEST_SECRET_KEY, TEST_ISSUER, TEST_AUDIENCE, TEST_EXPIRATION_MINUTES);
        var user = new UserModel(1, "test@demo.com", "John", "hash", false, DateTime.UtcNow);
        var beforeGeneration = DateTime.UtcNow;

        var token = jwtService.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var expectedExpiration = beforeGeneration.AddMinutes(TEST_EXPIRATION_MINUTES);
        
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateToken_CalledTwice_GeneratesDifferentTokens()
    {
        var jwtService = new JwtService(TEST_SECRET_KEY, TEST_ISSUER, TEST_AUDIENCE, TEST_EXPIRATION_MINUTES);
        var user = new UserModel(1, "test@demo.com", "John", "hash", false, DateTime.UtcNow);

        var token1 = jwtService.GenerateToken(user);
        var token2 = jwtService.GenerateToken(user);

        token1.Should().NotBe(token2);
    }
}
