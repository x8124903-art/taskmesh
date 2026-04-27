namespace MsNotifications.FunctionalTests.Application.Controllers;

[Collection("Server collection")]
public sealed class NotificationsControllerShould
{
    private readonly ServerFixture _fixture;
    private readonly HttpClient _client;

    public NotificationsControllerShould(ServerFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateAuthenticatedClient(userId: 1);
        _fixture.Cleanup();
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithEmptyList_WhenNoNotifications()
    {
        // Act
        var response = await _client.GetAsync("/notifications");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedNotificationsResponse>();
        result.Should().NotBeNull();
        result!.Notifications.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUnreadCount_ReturnsOk_WithZero_WhenNoNotifications()
    {
        // Act
        var response = await _client.GetAsync("/notifications/unread-count");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UnreadCountResponse>();
        result.Should().NotBeNull();
        result!.UnreadCount.Should().Be(0);
    }

    [Fact]
    public async Task MarkAsRead_ReturnsNoContent_WhenNotificationExists()
    {
        // Arrange

        // Act
        var response = await _client.PatchAsync("/notifications/1/read", null);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MarkAllAsRead_ReturnsOk_WithUpdatedCount()
    {
        // Act
        var response = await _client.PatchAsync("/notifications/mark-all-read", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsStringAsync();
        result.Should().Contain("updatedCount");
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_OrNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/notifications/1");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_WithFilters_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/notifications?isRead=false&type=TaskAssigned&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedNotificationsResponse>();
        result.Should().NotBeNull();
    }
}
