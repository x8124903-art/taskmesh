using Newtonsoft.Json;
using MsProjects.Application.Models;

namespace MsProjects.FunctionalTests.Application.Controllers;

[Collection(nameof(ServerFixtureCollection))]
public class ProjectInvitationsControllerShould(ServerFixture fixture)
{
    private readonly ServerFixture _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    
    private HttpClient CreateClientWithUserId(int userId)
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());
        return client;
    }

    [Fact]
    public async Task CreateInvitation_ReturnsCreated_WhenUserIsOwner()
    {
        var request = new InviteMemberRequest(
            1,
            "newinvite@taskmesh.com",
            "Member"
        );
        var content = new StringContent(
            JsonConvert.SerializeObject(request), 
            System.Text.Encoding.UTF8, 
            "application/json");

        var response = await CreateClientWithUserId(1).PostAsync("invitations", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var stringResult = await response.Content.ReadAsStringAsync();
        var invitation = JsonConvert.DeserializeObject<ProjectInvitationModel>(stringResult);
        invitation.Should().NotBeNull();
        invitation!.Email.Should().Be(request.Email);
        invitation.ProjectId.Should().Be(request.ProjectId);
    }

    [Fact]
    public async Task CreateInvitation_ReturnsForbidden_WhenUserIsNotOwnerOrAdmin()
    {
        var request = new InviteMemberRequest(
            1,
            "test@taskmesh.com",
            "Member"
        );
        var content = new StringContent(
            JsonConvert.SerializeObject(request), 
            System.Text.Encoding.UTF8, 
            "application/json");

        var response = await CreateClientWithUserId(999).PostAsync("invitations", content);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetInvitationById_ReturnsInvitation_WhenUserIsOwner()
    {
        var createRequest = new InviteMemberRequest(1, $"test{Guid.NewGuid()}@taskmesh.com", "Member");
        var createContent = new StringContent(
            JsonConvert.SerializeObject(createRequest), 
            System.Text.Encoding.UTF8, 
            "application/json");
        var createResponse = await CreateClientWithUserId(1).PostAsync("invitations", createContent);
        var createdInvitation = JsonConvert.DeserializeObject<ProjectInvitationModel>(
            await createResponse.Content.ReadAsStringAsync());

        var response = await CreateClientWithUserId(1).GetAsync($"invitations/{createdInvitation!.IdProjectInvitation}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stringResult = await response.Content.ReadAsStringAsync();
        var invitation = JsonConvert.DeserializeObject<ProjectInvitationModel>(stringResult);
        invitation.Should().NotBeNull();
        invitation!.IdProjectInvitation.Should().Be(createdInvitation.IdProjectInvitation);
    }

    [Fact]
    public async Task GetInvitationById_ReturnsForbidden_WhenUserIsNotOwner()
    {
        const int invitationId = 1;

        var response = await CreateClientWithUserId(999).GetAsync($"invitations/{invitationId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetInvitationById_ReturnsNotFound_WhenInvitationDoesNotExist()
    {
        const int nonExistentId = 99999;

        var response = await CreateClientWithUserId(1).GetAsync($"invitations/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetInvitationByToken_ReturnsInvitation_WhenTokenExists()
    {
        const string testToken = "test-token-001";

        var client = _fixture.CreateClient();
        var response = await client.GetAsync($"invitations/by-token/{testToken}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stringResult = await response.Content.ReadAsStringAsync();
        var invitation = JsonConvert.DeserializeObject<ProjectInvitationModel>(stringResult);
        invitation.Should().NotBeNull();
        invitation!.Token.Should().Be(testToken);
        invitation.Email.Should().Be("invited@taskmesh.com");
    }

    [Fact]
    public async Task GetInvitationByToken_ReturnsNotFound_WhenTokenDoesNotExist()
    {
        const string nonExistentToken = "non-existent-token";

        var client = _fixture.CreateClient();
        var response = await client.GetAsync($"invitations/by-token/{nonExistentToken}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPendingInvitations_ReturnsInvitations_WhenUserIsOwner()
    {
        const int projectId = 1;

        var response = await CreateClientWithUserId(1).GetAsync($"invitations/project/{projectId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stringResult = await response.Content.ReadAsStringAsync();
        var invitations = JsonConvert.DeserializeObject<List<ProjectInvitationModel>>(stringResult);
        invitations.Should().NotBeNull();
        invitations!.Should().HaveCountGreaterThan(0);
        invitations.All(i => i.ProjectId == projectId).Should().BeTrue();
    }

    [Fact]
    public async Task GetPendingInvitations_ReturnsForbidden_WhenUserIsNotOwner()
    {
        const int projectId = 1;

        var response = await CreateClientWithUserId(999).GetAsync($"invitations/project/{projectId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetMyInvitations_ReturnsInvitations_WhenEmailHasInvitations()
    {
        const string email = "invited@taskmesh.com";

        var client = _fixture.CreateClient();
        var response = await client.GetAsync($"invitations/my-invitations?email={email}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stringResult = await response.Content.ReadAsStringAsync();
        var invitations = JsonConvert.DeserializeObject<List<ProjectInvitationModel>>(stringResult);
        invitations.Should().NotBeNull();
        invitations!.Should().HaveCountGreaterThan(0);
        invitations.All(i => i.Email == email).Should().BeTrue();
    }

    [Fact]
    public async Task GetMyInvitations_ReturnsBadRequest_WhenEmailIsEmpty()
    {
        var client = _fixture.CreateClient();
        var response = await client.GetAsync("invitations/my-invitations?email=");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetMyInvitations_FiltersbyStatus_WhenStatusIsProvided()
    {
        const string email = "invited@taskmesh.com";
        const string status = "Pending";

        var client = _fixture.CreateClient();
        var response = await client.GetAsync($"invitations/my-invitations?email={email}&status={status}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stringResult = await response.Content.ReadAsStringAsync();
        var invitations = JsonConvert.DeserializeObject<List<ProjectInvitationModel>>(stringResult);
        invitations.Should().NotBeNull();
        invitations!.All(i => i.Status == status).Should().BeTrue();
    }

    [Fact]
    public async Task AcceptInvitation_ReturnsOk_WhenTokenIsValid()
    {
        var createRequest = new InviteMemberRequest(1, $"accept{Guid.NewGuid()}@taskmesh.com", "Member");
        var createContent = new StringContent(
            JsonConvert.SerializeObject(createRequest), 
            System.Text.Encoding.UTF8, 
            "application/json");
        var createResponse = await CreateClientWithUserId(1).PostAsync("invitations", createContent);
        var createdInvitation = JsonConvert.DeserializeObject<ProjectInvitationModel>(
            await createResponse.Content.ReadAsStringAsync());

        var acceptRequest = new AcceptInvitationRequest(createdInvitation!.Token);
        var acceptContent = new StringContent(
            JsonConvert.SerializeObject(acceptRequest), 
            System.Text.Encoding.UTF8, 
            "application/json");

        var response = await CreateClientWithUserId(5).PostAsync("invitations/accept", acceptContent);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsStringAsync();
        result.Should().Contain("accepted successfully");
    }

    [Fact]
    public async Task RejectInvitation_ReturnsOk_WhenTokenIsValid()
    {
        var createRequest = new InviteMemberRequest(1, $"reject{Guid.NewGuid()}@taskmesh.com", "Member");
        var createContent = new StringContent(
            JsonConvert.SerializeObject(createRequest), 
            System.Text.Encoding.UTF8, 
            "application/json");
        var createResponse = await CreateClientWithUserId(1).PostAsync("invitations", createContent);
        var createdInvitation = JsonConvert.DeserializeObject<ProjectInvitationModel>(
            await createResponse.Content.ReadAsStringAsync());

        var rejectRequest = new RejectInvitationRequest(createdInvitation!.Token);
        var rejectContent = new StringContent(
            JsonConvert.SerializeObject(rejectRequest), 
            System.Text.Encoding.UTF8, 
            "application/json");

        var response = await _fixture.CreateClient().PostAsync("invitations/reject", rejectContent);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsStringAsync();
        result.Should().Contain("rejected successfully");
    }

    [Fact]
    public async Task DeleteInvitation_ReturnsNoContent_WhenUserIsOwner()
    {
        var createRequest = new InviteMemberRequest(1, $"delete{Guid.NewGuid()}@taskmesh.com", "Member");
        var createContent = new StringContent(
            JsonConvert.SerializeObject(createRequest), 
            System.Text.Encoding.UTF8, 
            "application/json");
        var createResponse = await CreateClientWithUserId(1).PostAsync("invitations", createContent);
        var createdInvitation = JsonConvert.DeserializeObject<ProjectInvitationModel>(
            await createResponse.Content.ReadAsStringAsync());

        var response = await CreateClientWithUserId(1).DeleteAsync($"invitations/{createdInvitation!.Token}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteInvitation_ReturnsForbidden_WhenUserIsNotOwner()
    {
        const string testToken = "test-token-001";

        var response = await CreateClientWithUserId(999).DeleteAsync($"invitations/{testToken}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteInvitation_ReturnsNotFound_WhenTokenDoesNotExist()
    {
        const string nonExistentToken = "non-existent-token-delete";

        var response = await CreateClientWithUserId(1).DeleteAsync($"invitations/{nonExistentToken}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
