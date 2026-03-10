namespace MsProjects.Tests.Application.Models;

using MsProjects.Application.Models;

public sealed class ProjectModelShould
{
    [Fact]
    public void CreateWithAllProperties()
    {
        var createdAt = DateTime.UtcNow;
        
        var model = new ProjectModel(
            1,
            "Test Project",
            "Test Description",
            1,
            "Active",
            100,
            "John Doe",
            false,
            createdAt);
        
        model.IdProject.Should().Be(1);
        model.Name.Should().Be("Test Project");
        model.Description.Should().Be("Test Description");
        model.Status.Should().Be(1);
        model.StatusName.Should().Be("Active");
        model.CreatedBy.Should().Be(100);
        model.CreatedByName.Should().Be("John Doe");
        model.IsDeleted.Should().BeFalse();
        model.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void SupportRecordEquality()
    {
        var createdAt = DateTime.UtcNow;
        var model1 = new ProjectModel(1, "Name", "Desc", 1, "Active", 100, "Owner", false, createdAt);
        var model2 = new ProjectModel(1, "Name", "Desc", 1, "Active", 100, "Owner", false, createdAt);
        var model3 = new ProjectModel(2, "Name", "Desc", 1, "Active", 100, "Owner", false, createdAt);
        
        model1.Should().Be(model2);
        model1.Should().NotBe(model3);
    }

    [Fact]
    public void SupportDeletedProjects()
    {
        var model = new ProjectModel(1, "Name", "Desc", 1, "Active", 100, "Owner", true, DateTime.UtcNow);
        
        model.IsDeleted.Should().BeTrue();
    }

    [Theory]
    [InlineData(1, "Active")]
    [InlineData(2, "Paused")]
    [InlineData(3, "Completed")]
    [InlineData(4, "Archived")]
    public void SupportDifferentStatuses(int status, string statusName)
    {
        var model = new ProjectModel(1, "Name", "Desc", status, statusName, 100, "Owner", false, DateTime.UtcNow);
        
        model.Status.Should().Be(status);
        model.StatusName.Should().Be(statusName);
    }
}
