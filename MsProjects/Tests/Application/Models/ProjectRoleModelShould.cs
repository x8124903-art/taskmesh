namespace MsProjects.Tests.Application.Models;

using MsProjects.Application.Models;

public sealed class ProjectRoleModelShould
{
    [Fact]
    public void CreateWithValidParameters()
    {
        var model = new ProjectRoleModel(1, "Owner");
        
        model.IdProjectRole.Should().Be(1);
        model.Name.Should().Be("Owner");
    }

    [Theory]
    [InlineData(1, "Owner")]
    [InlineData(2, "Admin")]
    [InlineData(3, "Member")]
    [InlineData(4, "Viewer")]
    public void CreateWithDifferentRoles(int id, string name)
    {
        var model = new ProjectRoleModel(id, name);
        
        model.IdProjectRole.Should().Be(id);
        model.Name.Should().Be(name);
    }

    [Fact]
    public void SupportRecordEquality()
    {
        var model1 = new ProjectRoleModel(1, "Owner");
        var model2 = new ProjectRoleModel(1, "Owner");
        var model3 = new ProjectRoleModel(2, "Admin");
        
        model1.Should().Be(model2);
        model1.Should().NotBe(model3);
    }
}
