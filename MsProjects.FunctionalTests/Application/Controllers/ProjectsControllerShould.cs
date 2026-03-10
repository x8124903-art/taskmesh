using Newtonsoft.Json;
using MsProjects.Application.Models;

namespace MsProjects.FunctionalTests.Application.Controllers;

[Collection(nameof(ServerFixtureCollection))]
public class ProjectsControllerShould(ServerFixture fixture)
{
    private readonly ServerFixture _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    
    private HttpClient CreateClientWithUserId(int userId)
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());
        return client;
    }

    [Fact]
    public async Task GetAll_ReturnsProjects_ForAuthenticatedUser()
    {
        var response = await CreateClientWithUserId(1).GetAsync("projects");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stringResult = await response.Content.ReadAsStringAsync();
        var projects = JsonConvert.DeserializeObject<List<ProjectModel>>(stringResult);
        projects.Should().NotBeNull();
        projects.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task Get_ReturnsProject_WhenUserIsMember()
    {
        const int projectId = 1;

        var response = await CreateClientWithUserId(1).GetAsync($"projects/{projectId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stringResult = await response.Content.ReadAsStringAsync();
        var project = JsonConvert.DeserializeObject<ProjectModel>(stringResult);
        project.Should().NotBeNull();
        project!.IdProject.Should().Be(projectId);
        project.Name.Should().Be("Functional Test Project 1");
    }

    [Fact]
    public async Task Get_ReturnsForbidden_WhenUserIsNotMember()
    {
        const int projectId = 1;

        var response = await CreateClientWithUserId(999).GetAsync($"projects/{projectId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Add_CreatesProject_AndReturnsCreated()
    {
        var request = new AddProjectRequest(
            $"New Project {Guid.NewGuid()}",
            "A new test project"
        );
        var content = new StringContent(
            JsonConvert.SerializeObject(request), 
            System.Text.Encoding.UTF8, 
            "application/json");

        var response = await CreateClientWithUserId(1).PostAsync("projects", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var stringResult = await response.Content.ReadAsStringAsync();
        var project = JsonConvert.DeserializeObject<ProjectModel>(stringResult);
        project.Should().NotBeNull();
        project!.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task Update_UpdatesProject_WhenUserIsOwner()
    {
        const int projectId = 1;
        var request = new UpdateProjectRequest(
            "Updated Project Name",
            "Updated description",
            1
        );
        var content = new StringContent(
            JsonConvert.SerializeObject(new { idProject = projectId, name = request.Name, description = request.Description, status = request.Status }), 
            System.Text.Encoding.UTF8, 
            "application/json");

        var response = await CreateClientWithUserId(1).PutAsync($"projects/{projectId}", content);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var getResponse = await CreateClientWithUserId(1).GetAsync($"projects/{projectId}");
        var stringResult = await getResponse.Content.ReadAsStringAsync();
        var project = JsonConvert.DeserializeObject<ProjectModel>(stringResult);
        project!.Name.Should().Be("Updated Project Name");
    }

    [Fact]
    public async Task Update_ReturnsForbidden_WhenUserIsNotOwner()
    {
        const int projectId = 1;
        var request = new UpdateProjectRequest(
            "Unauthorized Update",
            "Should fail",
            1
        );
        var content = new StringContent(
            JsonConvert.SerializeObject(new { idProject = projectId, name = request.Name, description = request.Description, status = request.Status }), 
            System.Text.Encoding.UTF8, 
            "application/json");

        var response = await CreateClientWithUserId(2).PutAsync($"projects/{projectId}", content);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_DeletesProject_WhenUserIsOwner()
    {
        var createRequest = new AddProjectRequest(
            $"Project to Delete {Guid.NewGuid()}",
            "Will be deleted"
        );
        var createContent = new StringContent(
            JsonConvert.SerializeObject(createRequest), 
            System.Text.Encoding.UTF8, 
            "application/json");
        var createResponse = await CreateClientWithUserId(1).PostAsync("projects", createContent);
        var createResult = await createResponse.Content.ReadAsStringAsync();
        var createdProject = JsonConvert.DeserializeObject<ProjectModel>(createResult);

        var response = await CreateClientWithUserId(1).DeleteAsync($"projects/{createdProject!.IdProject}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var getResponse = await CreateClientWithUserId(1).GetAsync($"projects/{createdProject.IdProject}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ReturnsForbidden_WhenUserIsNotOwner()
    {
        const int projectId = 1;

        var response = await CreateClientWithUserId(2).DeleteAsync($"projects/{projectId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Add_ReturnsBadRequest_WhenNameIsEmpty()
    {
        var request = new AddProjectRequest(
            "",
            "Test"
        );
        var content = new StringContent(
            JsonConvert.SerializeObject(request), 
            System.Text.Encoding.UTF8, 
            "application/json");

        var response = await CreateClientWithUserId(1).PostAsync("projects", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenProjectDoesNotExist()
    {
        const int nonExistentProjectId = 99999;

        var response = await CreateClientWithUserId(1).GetAsync($"projects/{nonExistentProjectId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
