using Microsoft.Extensions.Logging;
using WebApp.Models.Projects;
using WebApp.Services;
using WebApp.Tests.Helpers;

namespace WebApp.Tests.Services;

public sealed class ProjectInvitationApiServiceShould
{
    private readonly Mock<ILogger<ProjectInvitationApiService>> _logger = new();

    private ProjectInvitationApiService CreateService(HttpMessageHandler handler)
    {
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        return new ProjectInvitationApiService(client, _logger.Object);
    }

    [Fact]
    public async Task GetMyInvitationsAsync_ReturnsInvitations()
    {
        var invitations = new List<ProjectInvitation>
        {
            new() { Token = "a", Status = "Pending" },
            new() { Token = "b", Status = "Accepted" }
        };
        var service = CreateService(MockHttpMessageHandler.WithJsonResponse(invitations));

        var result = await service.GetMyInvitationsAsync("test@test.com");

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMyInvitationsAsync_WithStatusFilter_IncludesStatusInUrl()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new List<ProjectInvitation>());
        var service = CreateService(handler);

        await service.GetMyInvitationsAsync("a@b.com", "Pending");

        handler.Requests[0].RequestUri!.PathAndQuery.Should().Contain("status=Pending");
    }

    [Fact]
    public async Task GetMyInvitationsAsync_WithoutStatus_DoesNotIncludeStatusParam()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new List<ProjectInvitation>());
        var service = CreateService(handler);

        await service.GetMyInvitationsAsync("a@b.com");

        handler.Requests[0].RequestUri!.PathAndQuery.Should().NotContain("status=");
    }

    [Fact]
    public async Task GetMyInvitationsAsync_ReturnsEmptyList_WhenFails()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.InternalServerError));

        var result = await service.GetMyInvitationsAsync("a@b.com");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AcceptInvitationAsync_ReturnsTrue_WhenSuccess()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK));

        var result = await service.AcceptInvitationAsync("token-abc");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task AcceptInvitationAsync_ReturnsFalse_WhenFails()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.BadRequest));

        var result = await service.AcceptInvitationAsync("token-abc");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AcceptInvitationAsync_SendsPostToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK);
        var service = CreateService(handler);

        await service.AcceptInvitationAsync("tok");

        handler.Requests[0].Method.Should().Be(HttpMethod.Post);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/invitations/accept");
    }

    [Fact]
    public async Task RejectInvitationAsync_ReturnsTrue_WhenSuccess()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK));

        var result = await service.RejectInvitationAsync("token-abc");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task RejectInvitationAsync_ReturnsFalse_WhenFails()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.BadRequest));

        var result = await service.RejectInvitationAsync("token-abc");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task RejectInvitationAsync_SendsPostToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK);
        var service = CreateService(handler);

        await service.RejectInvitationAsync("tok");

        handler.Requests[0].Method.Should().Be(HttpMethod.Post);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/invitations/reject");
    }

    [Fact]
    public async Task DeleteInvitationAsync_ReturnsTrue_WhenSuccess()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.NoContent));

        var result = await service.DeleteInvitationAsync("token-abc");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteInvitationAsync_ReturnsFalse_WhenFails()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.NotFound));

        var result = await service.DeleteInvitationAsync("token-abc");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteInvitationAsync_SendsDeleteToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.NoContent);
        var service = CreateService(handler);

        await service.DeleteInvitationAsync("some-token");

        handler.Requests[0].Method.Should().Be(HttpMethod.Delete);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/invitations/some-token");
    }

    [Fact]
    public async Task AcceptInvitationAsync_ReturnsFalse_WhenExceptionOccurs()
    {
        var handler = new MockHttpMessageHandler((_, _) => throw new HttpRequestException("Network error"));
        var service = CreateService(handler);

        var result = await service.AcceptInvitationAsync("tok");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task RejectInvitationAsync_ReturnsFalse_WhenExceptionOccurs()
    {
        var handler = new MockHttpMessageHandler((_, _) => throw new HttpRequestException("Network error"));
        var service = CreateService(handler);

        var result = await service.RejectInvitationAsync("tok");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteInvitationAsync_ReturnsFalse_WhenExceptionOccurs()
    {
        var handler = new MockHttpMessageHandler((_, _) => throw new HttpRequestException("Network error"));
        var service = CreateService(handler);

        var result = await service.DeleteInvitationAsync("tok");

        result.Should().BeFalse();
    }
}
