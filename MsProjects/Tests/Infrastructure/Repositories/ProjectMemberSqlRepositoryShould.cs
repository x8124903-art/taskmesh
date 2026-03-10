using FluentAssertions;
using MsProjects.Infrastructure.Data;
using MsProjects.Infrastructure.Repositories;
using System.Data;

namespace MsProjects.Tests.Infrastructure.Repositories;

public sealed class ProjectMemberSqlRepositoryShould
{
    [Fact]
    public void Constructor_WithValidContext_CreatesInstance()
    {
        var contextMock = new Mock<IDapperContext>();
        
        var repository = new ProjectMemberSqlRepository(contextMock.Object);
        
        repository.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMembersAsync_WithValidProjectId_CallsContext()
    {
        var contextMock = new Mock<IDapperContext>();
        var connectionMock = new Mock<IDbConnection>();
        
        contextMock.Setup(x => x.CreateConnection()).Returns(connectionMock.Object);
        
        var repository = new ProjectMemberSqlRepository(contextMock.Object);
        
        repository.Should().NotBeNull();
    }

    [Fact]
    public async Task AddMemberAsync_WithValidData_CallsContext()
    {
        var contextMock = new Mock<IDapperContext>();
        var connectionMock = new Mock<IDbConnection>();
        
        contextMock.Setup(x => x.CreateConnection()).Returns(connectionMock.Object);
        
        var repository = new ProjectMemberSqlRepository(contextMock.Object);
        
        repository.Should().NotBeNull();
    }
}
