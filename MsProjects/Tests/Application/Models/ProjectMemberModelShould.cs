namespace MsProjects.Tests.Application.Models;

using MsProjects.Application.Models;

public sealed class ProjectMemberModelShould
{
    [Fact]
    public void CreateWithAllProperties()
    {
        var invitedAt = DateTime.UtcNow;
        var joinedAt = DateTime.UtcNow.AddHours(1);
        
        var model = new ProjectMemberModel(
            1,
            10,
            100,
            "John Doe",
            "john@example.com",
            1,
            "Owner",
            invitedAt,
            joinedAt);
        
        model.IdProjectMember.Should().Be(1);
        model.ProjectId.Should().Be(10);
        model.UserId.Should().Be(100);
        model.UserName.Should().Be("John Doe");
        model.Email.Should().Be("john@example.com");
        model.Role.Should().Be(1);
        model.RoleName.Should().Be("Owner");
        model.InvitedAt.Should().Be(invitedAt);
        model.JoinedAt.Should().Be(joinedAt);
    }

    [Fact]
    public void SupportRecordEquality()
    {
        var invitedAt = DateTime.UtcNow;
        var model1 = new ProjectMemberModel(1, 10, 100, "John", "john@test.com", 1, "Owner", invitedAt, null);
        var model2 = new ProjectMemberModel(1, 10, 100, "John", "john@test.com", 1, "Owner", invitedAt, null);
        var model3 = new ProjectMemberModel(2, 10, 100, "John", "john@test.com", 1, "Owner", invitedAt, null);
        
        model1.Should().Be(model2);
        model1.Should().NotBe(model3);
    }

    [Fact]
    public void SupportNullJoinedAt()
    {
        var model = new ProjectMemberModel(1, 10, 100, "John", "john@test.com", 1, "Owner", DateTime.UtcNow, null);
        
        model.JoinedAt.Should().BeNull();
    }

    [Theory]
    [InlineData(1, "Owner")]
    [InlineData(2, "Admin")]
    [InlineData(3, "Member")]
    [InlineData(4, "Viewer")]
    public void SupportDifferentRoles(int role, string roleName)
    {
        var model = new ProjectMemberModel(1, 10, 100, "User", "user@test.com", role, roleName, DateTime.UtcNow, null);
        
        model.Role.Should().Be(role);
        model.RoleName.Should().Be(roleName);
    }
}
