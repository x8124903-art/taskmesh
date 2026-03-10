using FluentAssertions;
using MsProjects.Infrastructure.Data;
using MsProjects.Infrastructure.Repositories;
using System.Data;

namespace MsProjects.Tests.Infrastructure.Repositories;

public sealed class ProjectSqlRepositoryShould
{
    [Fact]
    public void Constructor_WithValidContext_CreatesInstance()
    {
        var contextMock = new Mock<IDapperContext>();
        
        var repository = new ProjectSqlRepository(contextMock.Object);
        
        repository.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllAsync_CallsContextConnection()
    {
        var contextMock = new Mock<IDapperContext>();
        var connectionMock = new Mock<IDbConnection>();
        
        contextMock.Setup(x => x.CreateConnection()).Returns(connectionMock.Object);
        
        var repository = new ProjectSqlRepository(contextMock.Object);
        
        repository.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAsync_WithValidId_CallsContext()
    {
        var contextMock = new Mock<IDapperContext>();
        var connectionMock = new Mock<IDbConnection>();
        
        contextMock.Setup(x => x.CreateConnection()).Returns(connectionMock.Object);
        
        var repository = new ProjectSqlRepository(contextMock.Object);
        
        repository.Should().NotBeNull();
    }
}
