namespace MsProjects.Tests.Domain.Services;

using MsProjects.Domain.Services;

public sealed class ProjectRolesShould
{
    [Fact]
    public void HaveOwnerIdConstant()
    {
        ProjectRoles.OwnerId.Should().Be(1);
    }

    [Fact]
    public void HaveAdminIdConstant()
    {
        ProjectRoles.AdminId.Should().Be(2);
    }

    [Fact]
    public void HaveMemberIdConstant()
    {
        ProjectRoles.MemberId.Should().Be(3);
    }

    [Fact]
    public void HaveViewerIdConstant()
    {
        ProjectRoles.ViewerId.Should().Be(4);
    }

    [Fact]
    public void HaveOwnerNameConstant()
    {
        ProjectRoles.Owner.Should().Be("Owner");
    }

    [Fact]
    public void HaveAdminNameConstant()
    {
        ProjectRoles.Admin.Should().Be("Admin");
    }

    [Fact]
    public void HaveMemberNameConstant()
    {
        ProjectRoles.Member.Should().Be("Member");
    }

    [Fact]
    public void HaveViewerNameConstant()
    {
        ProjectRoles.Viewer.Should().Be("Viewer");
    }

    [Theory]
    [InlineData("Owner", 1)]
    [InlineData("Admin", 2)]
    [InlineData("Member", 3)]
    [InlineData("Viewer", 4)]
    public void GetIdFromName_WithValidName_ReturnsCorrectId(string name, int expectedId)
    {
        var id = ProjectRoles.GetIdFromName(name);
        
        id.Should().Be(expectedId);
    }

    [Fact]
    public void GetIdFromName_WithInvalidName_ThrowsArgumentException()
    {
        var act = () => ProjectRoles.GetIdFromName("InvalidRole");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid role name: InvalidRole*");
    }

    [Theory]
    [InlineData(1, "Owner")]
    [InlineData(2, "Admin")]
    [InlineData(3, "Member")]
    [InlineData(4, "Viewer")]
    public void GetNameFromId_WithValidId_ReturnsCorrectName(int id, string expectedName)
    {
        var name = ProjectRoles.GetNameFromId(id);
        
        name.Should().Be(expectedName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(-1)]
    public void GetNameFromId_WithInvalidId_ThrowsArgumentException(int id)
    {
        var act = () => ProjectRoles.GetNameFromId(id);
        
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Invalid role ID: {id}*");
    }

    [Theory]
    [InlineData("Owner", true)]
    [InlineData("Admin", true)]
    [InlineData("Member", true)]
    [InlineData("Viewer", true)]
    [InlineData("InvalidRole", false)]
    [InlineData("", false)]
    [InlineData("owner", false)]
    public void IsValidRole_ReturnsExpectedResult(string role, bool expected)
    {
        ProjectRoles.IsValidRole(role).Should().Be(expected);
    }

    [Fact]
    public void AllRoles_ContainsAllFourRoles()
    {
        ProjectRoles.AllRoles.Should().HaveCount(4);
        ProjectRoles.AllRoles.Should().Contain(new[] { "Owner", "Admin", "Member", "Viewer" });
    }

    [Fact]
    public void ManagementRoles_ContainsOwnerAndAdmin()
    {
        ProjectRoles.ManagementRoles.Should().HaveCount(2);
        ProjectRoles.ManagementRoles.Should().Contain(new[] { "Owner", "Admin" });
    }

    [Fact]
    public void EditRoles_ContainsOwnerAndAdmin()
    {
        ProjectRoles.EditRoles.Should().HaveCount(2);
        ProjectRoles.EditRoles.Should().Contain(new[] { "Owner", "Admin" });
    }
}
