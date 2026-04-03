using WebApp.Models.Projects;
using WebApp.Services;
using WebApp.Tests.Helpers;

namespace WebApp.Tests.Services;

public sealed class ProjectApiServiceShould
{
    private static ProjectApiService CreateService(HttpMessageHandler handler)
    {
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        return new ProjectApiService(client);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsProjects()
    {
        var projects = new List<Project>
        {
            new() { IdProject = 1, Name = "P1" },
            new() { IdProject = 2, Name = "P2" }
        };
        var service = CreateService(MockHttpMessageHandler.WithJsonResponse(projects));

        var result = await service.GetAllAsync();

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("P1");
    }

    [Fact]
    public async Task GetAllAsync_SendsGetToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new List<Project>());
        var service = CreateService(handler);

        await service.GetAllAsync();

        handler.Requests.Should().ContainSingle();
        handler.Requests[0].Method.Should().Be(HttpMethod.Get);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/projects");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsProject()
    {
        var project = new Project { IdProject = 5, Name = "Test" };
        var service = CreateService(MockHttpMessageHandler.WithJsonResponse(project));

        var result = await service.GetByIdAsync(5);

        result.Should().NotBeNull();
        result!.IdProject.Should().Be(5);
    }

    [Fact]
    public async Task GetByIdAsync_SendsGetToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new Project());
        var service = CreateService(handler);

        await service.GetByIdAsync(42);

        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/projects/42");
    }

    [Fact]
    public async Task CreateAsync_ReturnsProject_WhenSuccess()
    {
        var project = new Project { IdProject = 10, Name = "New" };
        var service = CreateService(MockHttpMessageHandler.WithJsonResponse(project));

        var result = await service.CreateAsync(new CreateProjectRequest("New", "Desc"));

        result.Should().NotBeNull();
        result!.Name.Should().Be("New");
    }

    [Fact]
    public async Task CreateAsync_ReturnsNull_WhenFails()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.BadRequest));

        var result = await service.CreateAsync(new CreateProjectRequest("X", null));

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_SendsPostToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new Project());
        var service = CreateService(handler);

        await service.CreateAsync(new CreateProjectRequest("Test", "D"));

        handler.Requests[0].Method.Should().Be(HttpMethod.Post);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/projects");
    }

    [Fact]
    public async Task UpdateAsync_ReturnsTrue_WhenSuccess()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK));

        var result = await service.UpdateAsync(1, "Name", "Desc", "Active");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenFails()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.NotFound));

        var result = await service.UpdateAsync(999, "Name", "Desc", "Active");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_SendsPutToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK);
        var service = CreateService(handler);

        await service.UpdateAsync(7, "N", "D", "Paused");

        handler.Requests[0].Method.Should().Be(HttpMethod.Put);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/projects/7");
    }

    [Fact]
    public async Task UpdateAsync_MapsCompletedStatus()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK);
        var service = CreateService(handler);

        var result = await service.UpdateAsync(1, "N", "D", "Completed");

        result.Should().BeTrue();
        handler.Requests[0].Method.Should().Be(HttpMethod.Put);
    }

    [Fact]
    public async Task UpdateAsync_MapsArchivedStatus()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK);
        var service = CreateService(handler);

        var result = await service.UpdateAsync(1, "N", "D", "Archived");

        result.Should().BeTrue();
        handler.Requests[0].Method.Should().Be(HttpMethod.Put);
    }

    [Fact]
    public async Task UpdateAsync_MapsUnknownStatusToDefault()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK);
        var service = CreateService(handler);

        var result = await service.UpdateAsync(1, "N", "D", "UnknownStatus");

        result.Should().BeTrue();
        handler.Requests[0].Method.Should().Be(HttpMethod.Put);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenSuccess()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.NoContent));

        var result = await service.DeleteAsync(1);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenFails()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.Forbidden));

        var result = await service.DeleteAsync(1);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.NoContent);
        var service = CreateService(handler);

        await service.DeleteAsync(3);

        handler.Requests[0].Method.Should().Be(HttpMethod.Delete);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/projects/3");
    }

    [Fact]
    public async Task GetMembersAsync_ReturnsMembers()
    {
        var members = new List<ProjectMember>
        {
            new() { IdProjectMember = 1, UserName = "Alice" },
            new() { IdProjectMember = 2, UserName = "Bob" }
        };
        var service = CreateService(MockHttpMessageHandler.WithJsonResponse(members));

        var result = await service.GetMembersAsync(1);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMembersAsync_SendsGetToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new List<ProjectMember>());
        var service = CreateService(handler);

        await service.GetMembersAsync(5);

        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/projects/5/members");
    }

    [Fact]
    public async Task ChangeRoleAsync_ReturnsTrue_WhenSuccess()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK));

        var result = await service.ChangeRoleAsync(1, 2, "Admin");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ChangeRoleAsync_SendsPutToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK);
        var service = CreateService(handler);

        await service.ChangeRoleAsync(10, 20, "Editor");

        handler.Requests[0].Method.Should().Be(HttpMethod.Put);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/projects/10/members/20/role");
    }

    [Fact]
    public async Task RemoveMemberAsync_ReturnsTrue_WhenSuccess()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.NoContent));

        var result = await service.RemoveMemberAsync(1, 2);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveMemberAsync_SendsDeleteToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.NoContent);
        var service = CreateService(handler);

        await service.RemoveMemberAsync(10, 20);

        handler.Requests[0].Method.Should().Be(HttpMethod.Delete);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/projects/10/members/20");
    }

    [Fact]
    public async Task CreateInvitationAsync_ReturnsInvitation_WhenSuccess()
    {
        var invitation = new ProjectInvitation { IdProjectInvitation = 1, Token = "abc" };
        var service = CreateService(MockHttpMessageHandler.WithJsonResponse(invitation));

        var req = new InviteMemberRequest { ProjectId = 1, Email = "x@x.com", Role = "Member" };
        var result = await service.CreateInvitationAsync(req);

        result.Should().NotBeNull();
        result!.Token.Should().Be("abc");
    }

    [Fact]
    public async Task CreateInvitationAsync_ReturnsNull_WhenFails()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.BadRequest));

        var req = new InviteMemberRequest { ProjectId = 1, Email = "x@x.com", Role = "Member" };
        var result = await service.CreateInvitationAsync(req);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetInvitationByTokenAsync_ReturnsInvitation()
    {
        var invitation = new ProjectInvitation { Token = "tok123" };
        var service = CreateService(MockHttpMessageHandler.WithJsonResponse(invitation));

        var result = await service.GetInvitationByTokenAsync("tok123");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetInvitationByTokenAsync_SendsGetToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new ProjectInvitation());
        var service = CreateService(handler);

        await service.GetInvitationByTokenAsync("my-token");

        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/invitations/by-token/my-token");
    }

    [Fact]
    public async Task GetPendingInvitationsAsync_ReturnsInvitations()
    {
        var invitations = new List<ProjectInvitation> { new() { Token = "a" } };
        var service = CreateService(MockHttpMessageHandler.WithJsonResponse(invitations));

        var result = await service.GetPendingInvitationsAsync(1);

        result.Should().ContainSingle();
    }

    [Fact]
    public async Task GetPendingInvitationsAsync_SendsGetToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new List<ProjectInvitation>());
        var service = CreateService(handler);

        await service.GetPendingInvitationsAsync(5);

        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/invitations/project/5");
    }

    [Fact]
    public async Task GetMyInvitationsAsync_SendsCorrectEndpointWithEmail()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new List<ProjectInvitation>());
        var service = CreateService(handler);

        await service.GetMyInvitationsAsync("test@test.com");

        handler.Requests[0].RequestUri!.PathAndQuery.Should().Contain("/api/v1/invitations/my-invitations");
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Contain("email=test%40test.com");
    }

    [Fact]
    public async Task AcceptInvitationAsync_ReturnsTrue_WhenSuccess()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK));

        var result = await service.AcceptInvitationAsync("token123");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task AcceptInvitationAsync_ReturnsFalse_WhenFails()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.BadRequest));

        var result = await service.AcceptInvitationAsync("token123");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task RejectInvitationAsync_ReturnsTrue_WhenSuccess()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK));

        var result = await service.RejectInvitationAsync("token123");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task RejectInvitationAsync_ReturnsFalse_WhenFails()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.BadRequest));

        var result = await service.RejectInvitationAsync("token123");

        result.Should().BeFalse();
    }
}
