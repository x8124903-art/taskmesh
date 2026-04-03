using FluentAssertions;

namespace MsProjects.Tests.Infrastructure.Data;

public sealed class DatabaseOptionsShould
{
    [Fact]
    public void HaveEmptyDefaultValues()
    {
        var options = new MsProjects.Infrastructure.Data.DatabaseOptions();
        options.Type.Should().BeEmpty();
        options.DefaultConnection.Should().BeEmpty();
    }

    [Fact]
    public void AllowSettingProperties()
    {
        var options = new MsProjects.Infrastructure.Data.DatabaseOptions
        {
            Type = "MySQL",
            DefaultConnection = "Server=localhost;Database=test"
        };
        options.Type.Should().Be("MySQL");
        options.DefaultConnection.Should().Be("Server=localhost;Database=test");
    }
}
