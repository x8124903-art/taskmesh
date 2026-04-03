namespace MsProjects.Tests.Application.Models;

using MsProjects.Application.Models;

public sealed class ProjectInvitationModelShould
{
    [Fact]
    public void CreateWithAllProperties()
    {
        var createdAt = DateTime.UtcNow;
        var expiresAt = createdAt.AddDays(7);
        var acceptedAt = createdAt.AddHours(1);
        
        var model = new ProjectInvitationModel(
            1,
            10,
            "Test Project",
            "user@example.com",
            2,
            "Admin",
            "abc123token",
            "Accepted",
            100,
            "Inviter Name",
            createdAt,
            expiresAt,
            acceptedAt,
            null);
        
        model.IdProjectInvitation.Should().Be(1);
        model.ProjectId.Should().Be(10);
        model.ProjectName.Should().Be("Test Project");
        model.Email.Should().Be("user@example.com");
        model.Role.Should().Be(2);
        model.RoleName.Should().Be("Admin");
        model.Token.Should().Be("abc123token");
        model.Status.Should().Be("Accepted");
        model.InvitedBy.Should().Be(100);
        model.InvitedByName.Should().Be("Inviter Name");
        model.CreatedAt.Should().Be(createdAt);
        model.ExpiresAt.Should().Be(expiresAt);
        model.AcceptedAt.Should().Be(acceptedAt);
        model.RejectedAt.Should().BeNull();
    }

    [Fact]
    public void SupportRecordEquality()
    {
        var createdAt = DateTime.UtcNow;
        var expiresAt = createdAt.AddDays(7);
        
        var model1 = new ProjectInvitationModel(1, 10, "Project", "user@test.com", 2, "Admin", "token", "Pending", 100, "Inviter", createdAt, expiresAt, null, null);
        var model2 = new ProjectInvitationModel(1, 10, "Project", "user@test.com", 2, "Admin", "token", "Pending", 100, "Inviter", createdAt, expiresAt, null, null);
        var model3 = new ProjectInvitationModel(2, 10, "Project", "user@test.com", 2, "Admin", "token", "Pending", 100, "Inviter", createdAt, expiresAt, null, null);
        
        model1.Should().Be(model2);
        model1.Should().NotBe(model3);
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Accepted")]
    [InlineData("Rejected")]
    [InlineData("Expired")]
    public void SupportDifferentStatuses(string status)
    {
        var createdAt = DateTime.UtcNow;
        var expiresAt = createdAt.AddDays(7);
        
        var model = new ProjectInvitationModel(1, 10, "Project", "user@test.com", 2, "Admin", "token", status, 100, "Inviter", createdAt, expiresAt, null, null);
        
        model.Status.Should().Be(status);
    }

    [Fact]
    public void SupportPendingInvitation()
    {
        var createdAt = DateTime.UtcNow;
        var expiresAt = createdAt.AddDays(7);
        
        var model = new ProjectInvitationModel(1, 10, "Project", "user@test.com", 2, "Admin", "token", "Pending", 100, "Inviter", createdAt, expiresAt, null, null);
        
        model.Status.Should().Be("Pending");
        model.AcceptedAt.Should().BeNull();
        model.RejectedAt.Should().BeNull();
    }

    [Fact]
    public void SupportRejectedInvitation()
    {
        var createdAt = DateTime.UtcNow;
        var expiresAt = createdAt.AddDays(7);
        var rejectedAt = createdAt.AddHours(2);
        
        var model = new ProjectInvitationModel(1, 10, "Project", "user@test.com", 2, "Admin", "token", "Rejected", 100, "Inviter", createdAt, expiresAt, null, rejectedAt);
        
        model.Status.Should().Be("Rejected");
        model.AcceptedAt.Should().BeNull();
        model.RejectedAt.Should().Be(rejectedAt);
    }

    [Theory]
    [InlineData(1, "Owner")]
    [InlineData(2, "Admin")]
    [InlineData(3, "Member")]
    [InlineData(4, "Viewer")]
    public void SupportDifferentRoles(int role, string roleName)
    {
        var createdAt = DateTime.UtcNow;
        var expiresAt = createdAt.AddDays(7);
        
        var model = new ProjectInvitationModel(1, 10, "Project", "user@test.com", role, roleName, "token", "Pending", 100, "Inviter", createdAt, expiresAt, null, null);
        
        model.Role.Should().Be(role);
        model.RoleName.Should().Be(roleName);
    }
}
