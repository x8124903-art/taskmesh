using WebApp.Models.Notifications;
using WebApp.Services;
using WebApp.Tests.Helpers;

namespace WebApp.Tests.Services;

public sealed class NotificationApiServiceShould
{
    private static NotificationApiService CreateService(HttpMessageHandler handler)
    {
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        return new NotificationApiService(client);
    }

    [Fact]
    public async Task GetNotificationsAsync_ReturnsNotifications()
    {
        var notifications = new List<NotificationModel>
        {
            new(1, 1, "TaskAssigned", "N1", "Msg1", "Task", 5, 1, false, DateTime.UtcNow),
            new(2, 1, "TaskAssigned", "N2", "Msg2", "Task", 6, 1, false, DateTime.UtcNow)
        };
        var response = new PagedNotificationsResponse(notifications, 2, 1, 10);
        var service = CreateService(MockHttpMessageHandler.WithJsonResponse(response));

        var result = await service.GetNotificationsAsync();

        result.Should().NotBeNull();
        result!.Notifications.Should().HaveCount(2);
        result.Notifications.ElementAt(0).Title.Should().Be("N1");
    }

    [Fact]
    public async Task GetNotificationsAsync_SendsGetToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new PagedNotificationsResponse(new List<NotificationModel>(), 0, 1, 10));
        var service = CreateService(handler);

        await service.GetNotificationsAsync();

        handler.Requests.Should().ContainSingle();
        handler.Requests[0].Method.Should().Be(HttpMethod.Get);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Contain("/api/v1/notifications");
    }

    [Fact]
    public async Task GetNotificationsAsync_WithIsRead_IncludesIsReadInQuery()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new PagedNotificationsResponse(new List<NotificationModel>(), 0, 1, 10));
        var service = CreateService(handler);

        await service.GetNotificationsAsync(isRead: true);

        handler.Requests[0].RequestUri!.PathAndQuery.Should().Contain("isRead=true");
    }

    [Fact]
    public async Task GetNotificationsAsync_WithType_IncludesTypeInQuery()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new PagedNotificationsResponse(new List<NotificationModel>(), 0, 1, 10));
        var service = CreateService(handler);

        await service.GetNotificationsAsync(type: "TaskAssigned");

        handler.Requests[0].RequestUri!.PathAndQuery.Should().Contain("type=TaskAssigned");
    }

    [Fact]
    public async Task GetNotificationsAsync_WithPagination_IncludesPaginationInQuery()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new PagedNotificationsResponse(new List<NotificationModel>(), 0, 2, 20));
        var service = CreateService(handler);

        await service.GetNotificationsAsync(page: 2, pageSize: 20);

        handler.Requests[0].RequestUri!.PathAndQuery.Should().Contain("pageNumber=2");
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Contain("pageSize=20");
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsCount()
    {
        var response = new UnreadCountResponse(5);
        var service = CreateService(MockHttpMessageHandler.WithJsonResponse(response));

        var result = await service.GetUnreadCountAsync();

        result.Should().NotBeNull();
        result!.UnreadCount.Should().Be(5);
    }

    [Fact]
    public async Task GetUnreadCountAsync_SendsGetToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithJsonResponse(new UnreadCountResponse(0));
        var service = CreateService(handler);

        await service.GetUnreadCountAsync();

        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/notifications/unread-count");
    }

    [Fact]
    public async Task MarkAsReadAsync_SendsPatchToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK);
        var service = CreateService(handler);

        await service.MarkAsReadAsync(42);

        handler.Requests[0].Method.Should().Be(HttpMethod.Patch);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/notifications/42/read");
    }

    [Fact]
    public async Task MarkAsReadAsync_ThrowsException_WhenFails()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.NotFound));

        var act = async () => await service.MarkAsReadAsync(999);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task MarkAllAsReadAsync_SendsPatchToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.OK);
        var service = CreateService(handler);

        await service.MarkAllAsReadAsync();

        handler.Requests[0].Method.Should().Be(HttpMethod.Patch);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/notifications/mark-all-read");
    }

    [Fact]
    public async Task MarkAllAsReadAsync_ThrowsException_WhenFails()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.InternalServerError));

        var act = async () => await service.MarkAllAsReadAsync();

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToCorrectEndpoint()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.NoContent);
        var service = CreateService(handler);

        await service.DeleteAsync(55);

        handler.Requests[0].Method.Should().Be(HttpMethod.Delete);
        handler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/api/v1/notifications/55");
    }

    [Fact]
    public async Task DeleteAsync_ThrowsException_WhenFails()
    {
        var service = CreateService(MockHttpMessageHandler.WithStatusCode(HttpStatusCode.NotFound));

        var act = async () => await service.DeleteAsync(999);

        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
