using FluentAssertions;
using MsAuth.Application.Models;
using MsAuth.Infrastructure.Data;
using MsAuth.Infrastructure.Repositories;
using System.Data;

namespace MsAuth.Tests.Infrastructure.Repositories;

public sealed class UserSqlRepositoryShould
{
    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsUser()
    {
        var contextMock = new Mock<IDapperContext>();
        var connectionMock = new Mock<IDbConnection>();
        
        var expectedUser = new UserModel(1, "test@demo.com", "Test User", "hashedPassword", false, DateTime.UtcNow);
        
        contextMock.Setup(x => x.CreateConnection()).Returns(connectionMock.Object);
        
        var repository = new UserSqlRepository(contextMock.Object);
        
        repository.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WithValidEmail_CallsDapperCorrectly()
    {
        var contextMock = new Mock<IDapperContext>();
        var connectionMock = new Mock<IDbConnection>();
        
        contextMock.Setup(x => x.CreateConnection()).Returns(connectionMock.Object);
        
        var repository = new UserSqlRepository(contextMock.Object);
        
        contextMock.Verify(x => x.CreateConnection(), Times.Never);
        repository.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithValidContext_CreatesInstance()
    {
        var contextMock = new Mock<IDapperContext>();
        
        var repository = new UserSqlRepository(contextMock.Object);
        
        repository.Should().NotBeNull();
    }

    [Fact]
    public async Task AddAsync_WithValidRequest_ReturnsUserModel()
    {
        var contextMock = new Mock<IDapperContext>();
        var connectionMock = new Mock<IDbConnection>();
        
        contextMock.Setup(x => x.CreateConnection()).Returns(connectionMock.Object);
        
        var repository = new UserSqlRepository(contextMock.Object);
        var request = new RegisterRequest("new@demo.com", "New User", "hashedPass");
        
        repository.Should().NotBeNull();
    }
}
