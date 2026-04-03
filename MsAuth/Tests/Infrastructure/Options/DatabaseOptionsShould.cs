using FluentAssertions;
using MsAuth.Infrastructure.Options;

namespace MsAuth.Tests.Infrastructure.Options;

public sealed class DatabaseOptionsShould
{
    [Fact]
    public void HaveDefaultEmptyValues()
    {
        var options = new DatabaseOptions();
        options.Type.Should().BeEmpty();
        options.DefaultConnection.Should().BeEmpty();
    }

    [Fact]
    public void AllowSettingProperties()
    {
        var options = new DatabaseOptions
        {
            Type = "MySQL",
            DefaultConnection = "Server=localhost;Database=test"
        };
        options.Type.Should().Be("MySQL");
        options.DefaultConnection.Should().Be("Server=localhost;Database=test");
    }
}
