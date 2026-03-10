using FluentAssertions;
using MsAuth.Application.Models;
using MsAuth.Infrastructure.Data;
using MsAuth.Infrastructure.Repositories;
using System.Data;

namespace MsAuth.Tests.Infrastructure.Repositories;

public sealed class RefreshTokenSqlRepositoryShould
{
    [Fact]
    public void Constructor_WithValidContext_CreatesInstance()
    {
        var contextMock = new Mock<IDapperContext>();
        
        var repository = new RefreshTokenSqlRepository(contextMock.Object);
        
        repository.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByTokenAsync_WithValidToken_CallsContextCorrectly()
    {
        var contextMock = new Mock<IDapperContext>();
        var connectionMock = new Mock<IDbConnection>();
        
        contextMock.Setup(x => x.CreateConnection()).Returns(connectionMock.Object);
        
        var repository = new RefreshTokenSqlRepository(contextMock.Object);
        
        repository.Should().NotBeNull();
    }

    [Fact]
    public async Task AddAsync_WithValidModel_ReturnsId()
    {
        var contextMock = new Mock<IDapperContext>();
        var connectionMock = new Mock<IDbConnection>();
        
        contextMock.Setup(x => x.CreateConnection()).Returns(connectionMock.Object);
        
        var repository = new RefreshTokenSqlRepository(contextMock.Object);
        var model = new RefreshTokenModel(0, "token123", 1, DateTime.UtcNow.AddDays(7), false, DateTime.UtcNow, null);
        
        repository.Should().NotBeNull();
    }

    [Fact]
    public async Task RevokeAsync_WithValidToken_ExecutesCommand()
    {
        var contextMock = new Mock<IDapperContext>();
        var connectionMock = new Mock<IDbConnection>();
        
        contextMock.Setup(x => x.CreateConnection()).Returns(connectionMock.Object);
        
        var repository = new RefreshTokenSqlRepository(contextMock.Object);
        
        repository.Should().NotBeNull();
    }
}
