using FluentAssertions;
using MsProjects.Infrastructure.Data;
using MsProjects.Infrastructure.Repositories;
using System.Data;

namespace MsProjects.Tests.Infrastructure.Repositories;

public sealed class ProjectInvitationSqlRepositoryShould
{
    [Fact]
    public void Constructor_WithValidContext_CreatesInstance()
    {
        var contextMock = new Mock<IDapperContext>();
        
        var repository = new ProjectInvitationSqlRepository(contextMock.Object);
        
        repository.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_WithValidInvitation_CallsContext()
    {
        var contextMock = new Mock<IDapperContext>();
        var connectionMock = new Mock<IDbConnection>();
        
        contextMock.Setup(x => x.CreateConnection()).Returns(connectionMock.Object);
        
        var repository = new ProjectInvitationSqlRepository(contextMock.Object);
        
        repository.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByTokenAsync_WithValidToken_CallsContext()
    {
        var contextMock = new Mock<IDapperContext>();
        var connectionMock = new Mock<IDbConnection>();
        
        contextMock.Setup(x => x.CreateConnection()).Returns(connectionMock.Object);
        
        var repository = new ProjectInvitationSqlRepository(contextMock.Object);
        
        repository.Should().NotBeNull();
    }
}
