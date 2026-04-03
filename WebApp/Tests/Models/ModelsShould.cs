using WebApp.Models;
using WebApp.Models.Auth;
using WebApp.Models.Projects;

namespace WebApp.Tests.Models;

public sealed class ModelsShould
{

    [Fact]
    public void User_HasDefaultValues()
    {
        var user = new User();

        user.Id.Should().Be(0);
        user.Name.Should().BeEmpty();
        user.Email.Should().BeEmpty();
    }

    [Fact]
    public void User_CanSetProperties()
    {
        var date = new DateTime(2024, 1, 1);
        var user = new User { Id = 1, Name = "Test", Email = "t@t.com", CreatedAt = date };

        user.Id.Should().Be(1);
        user.Name.Should().Be("Test");
        user.Email.Should().Be("t@t.com");
        user.CreatedAt.Should().Be(date);
    }

    [Fact]
    public void LoginRequest_HasDefaultValues()
    {
        var req = new LoginRequest();

        req.Email.Should().BeEmpty();
        req.Password.Should().BeEmpty();
    }

    [Fact]
    public void LoginRequest_CanSetProperties()
    {
        var req = new LoginRequest { Email = "a@a.com", Password = "pass" };

        req.Email.Should().Be("a@a.com");
        req.Password.Should().Be("pass");
    }

    [Fact]
    public void LoginResponse_HasDefaultValues()
    {
        var resp = new LoginResponse();

        resp.AccessToken.Should().BeEmpty();
        resp.RefreshToken.Should().BeEmpty();
        resp.User.Should().BeNull();
    }

    [Fact]
    public void LoginResponse_CanSetProperties()
    {
        var user = new User { Id = 1 };
        var resp = new LoginResponse { AccessToken = "at", RefreshToken = "rt", User = user };

        resp.AccessToken.Should().Be("at");
        resp.RefreshToken.Should().Be("rt");
        resp.User.Should().Be(user);
    }

    [Fact]
    public void Project_HasDefaultValues()
    {
        var p = new Project();

        p.IdProject.Should().Be(0);
        p.Name.Should().BeEmpty();
        p.Description.Should().BeEmpty();
        p.Status.Should().Be(0);
        p.StatusName.Should().BeEmpty();
        p.CreatedByName.Should().BeEmpty();
    }

    [Fact]
    public void Project_CanSetAllProperties()
    {
        var date = new DateTime(2024, 6, 1);
        var p = new Project
        {
            IdProject = 1, Name = "P", Description = "D",
            Status = 1, StatusName = "Active",
            CreatedByName = "User", CreatedAt = date
        };

        p.IdProject.Should().Be(1);
        p.Status.Should().Be(1);
        p.CreatedAt.Should().Be(date);
    }

    [Fact]
    public void CreateProjectRequest_IsRecord()
    {
        var req = new CreateProjectRequest("Name", "Desc");

        req.Name.Should().Be("Name");
        req.Description.Should().Be("Desc");
    }

    [Fact]
    public void CreateProjectRequest_SupportsNullDescription()
    {
        var req = new CreateProjectRequest("Name", null);

        req.Description.Should().BeNull();
    }

    [Fact]
    public void CreateProjectRequest_SupportsRecordEquality()
    {
        var a = new CreateProjectRequest("N", "D");
        var b = new CreateProjectRequest("N", "D");

        a.Should().Be(b);
    }

    [Fact]
    public void ProjectMember_HasDefaultValues()
    {
        var m = new ProjectMember();

        m.IdProjectMember.Should().Be(0);
        m.IdProject.Should().Be(0);
        m.UserId.Should().Be(0);
        m.UserName.Should().BeEmpty();
        m.Email.Should().BeEmpty();
        m.Role.Should().Be(0);
        m.RoleName.Should().BeEmpty();
        m.JoinedAt.Should().BeNull();
    }

    [Fact]
    public void ProjectInvitation_HasDefaultValues()
    {
        var inv = new ProjectInvitation();

        inv.IdProjectInvitation.Should().Be(0);
        inv.ProjectId.Should().Be(0);
        inv.ProjectName.Should().BeEmpty();
        inv.Email.Should().BeEmpty();
        inv.Token.Should().BeEmpty();
        inv.Status.Should().BeEmpty();
        inv.AcceptedAt.Should().BeNull();
        inv.RejectedAt.Should().BeNull();
    }

    [Fact]
    public void ProjectInvitation_CanSetAllProperties()
    {
        var now = DateTime.UtcNow;
        var inv = new ProjectInvitation
        {
            IdProjectInvitation = 1, ProjectId = 2, ProjectName = "P",
            Email = "e@e.com", Role = 1, RoleName = "Member",
            Token = "tok", Status = "Pending",
            InvitedByUserId = 3, InvitedByName = "Admin",
            InvitedAt = now, ExpiresAt = now.AddDays(7),
            AcceptedAt = now, RejectedAt = null
        };

        inv.IdProjectInvitation.Should().Be(1);
        inv.ProjectId.Should().Be(2);
        inv.Token.Should().Be("tok");
        inv.AcceptedAt.Should().Be(now);
    }

    [Fact]
    public void InviteMemberRequest_HasDefaultValues()
    {
        var req = new InviteMemberRequest();

        req.ProjectId.Should().Be(0);
        req.Email.Should().BeEmpty();
        req.Role.Should().BeEmpty();
    }

    [Fact]
    public void AcceptInvitationRequest_HasDefaultValues()
    {
        var req = new AcceptInvitationRequest();
        req.Token.Should().BeEmpty();
    }

    [Fact]
    public void RejectInvitationRequest_HasDefaultValues()
    {
        var req = new RejectInvitationRequest();
        req.Token.Should().BeEmpty();
    }
}
