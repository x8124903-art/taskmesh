using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace MsProjects.IntegrationTests.Application.Controllers;

[Collection(nameof(ProjectsControllerCollection))]
public sealed class ProjectMembersControllerShould(ProjectsControllerFixture fixture)
{
    private HttpClient CreateClient(int userId = 1)
    {
        var client = fixture.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());
        return client;
    }

    [Fact]
    public async Task GetRole_ReturnsOk_WithRole_WhenUserIsMember()
    {
        const int PROJECT_ID = 1;
        const int USER_ID = 1;
        var client = CreateClient(USER_ID);

        var response = await client.GetAsync($"projects/{PROJECT_ID}/members/{USER_ID}/role");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stringResult = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(stringResult);
        result.GetProperty("role").GetString().Should().Be("Owner");
    }

    [Fact]
    public async Task GetRole_ReturnsNotFound_WhenUserIsNotMember()
    {
        const int PROJECT_ID = 1;
        const int NON_MEMBER_USER_ID = 999;
        var client = CreateClient(1);

        var response = await client.GetAsync($"projects/{PROJECT_ID}/members/{NON_MEMBER_USER_ID}/role");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
