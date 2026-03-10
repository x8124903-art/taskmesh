namespace MsProjects.Tests.Domain.Services;

using MsProjects.Domain.Services;

public sealed class ProjectStatusesShould
{
    [Fact]
    public void HaveActiveConstant()
    {
        ProjectStatuses.Active.Should().Be(1);
    }

    [Fact]
    public void HavePausedConstant()
    {
        ProjectStatuses.Paused.Should().Be(2);
    }

    [Fact]
    public void HaveCompletedConstant()
    {
        ProjectStatuses.Completed.Should().Be(3);
    }

    [Fact]
    public void HaveArchivedConstant()
    {
        ProjectStatuses.Archived.Should().Be(4);
    }

    [Theory]
    [InlineData(1, "Active")]
    [InlineData(2, "Paused")]
    [InlineData(3, "Completed")]
    [InlineData(4, "Archived")]
    public void GetName_WithValidId_ReturnsCorrectName(int id, string expectedName)
    {
        var name = ProjectStatuses.GetName(id);
        
        name.Should().Be(expectedName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(-1)]
    [InlineData(999)]
    public void GetName_WithInvalidId_ReturnsUnknown(int id)
    {
        var name = ProjectStatuses.GetName(id);
        
        name.Should().Be("Unknown");
    }
}
