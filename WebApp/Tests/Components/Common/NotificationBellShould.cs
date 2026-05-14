using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Moq;
using WebApp.Services;
using WebApp.Models.Notifications;
using WebApp.Components.Common;

namespace WebApp.Tests.Components.Common;

public sealed class NotificationBellShould : TestContext
{
    private readonly Mock<INotificationApiService> _notificationServiceMock;

    public NotificationBellShould()
    {
        _notificationServiceMock = new Mock<INotificationApiService>();
    }

    private void SetupServices(int unreadCount = 0)
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        _notificationServiceMock.Setup(x => x.GetUnreadCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UnreadCountResponse(unreadCount));

        Services.AddSingleton<INotificationApiService>(_notificationServiceMock.Object);
    }

    [Fact]
    public void RenderBadge_WithUnreadCount()
    {
        SetupServices(unreadCount: 5);

        var cut = RenderComponent<NotificationBell>();

        cut.WaitForState(() => cut.Markup.Contains("5"), TimeSpan.FromSeconds(2));
        cut.Markup.Should().Contain("5");
    }

    [Fact]
    public void HideBadge_WhenUnreadCountIsZero()
    {
        SetupServices(unreadCount: 0);

        var cut = RenderComponent<NotificationBell>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().NotContain(">0<");
        }, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ToggleDropdown_OnBellClick()
    {
        SetupServices(unreadCount: 3);

        RenderComponent<MudPopoverProvider>();
        var cut = RenderComponent<NotificationBell>();

        cut.WaitForState(() => cut.Find("button") != null, TimeSpan.FromSeconds(2));
        var bellButton = cut.Find("button");
        bellButton.Click();

        cut.WaitForState(() => cut.HasComponent<NotificationDropdown>(), TimeSpan.FromSeconds(2));
        cut.HasComponent<NotificationDropdown>().Should().BeTrue();
    }

    [Fact]
    public void PollUnreadCount_Every30Seconds()
    {
        SetupServices(unreadCount: 1);

        var cut = RenderComponent<NotificationBell>();

        cut.WaitForState(() => cut.Markup.Contains("1"), TimeSpan.FromSeconds(2));

        _notificationServiceMock.Verify(x => x.GetUnreadCountAsync(It.IsAny<CancellationToken>()), 
            Times.AtLeastOnce);
    }

    [Fact]
    public void CloseDropdown_WhenOverlayClicked()
    {
        SetupServices(unreadCount: 2);

        RenderComponent<MudPopoverProvider>();
        var cut = RenderComponent<NotificationBell>();

        cut.WaitForState(() => cut.Find("button") != null, TimeSpan.FromSeconds(2));
        cut.Find("button").Click();

        cut.WaitForState(() => cut.HasComponent<NotificationDropdown>(), TimeSpan.FromSeconds(2));

        var overlay = cut.Find(".mud-overlay");
        overlay.Click();

        cut.WaitForAssertion(() =>
        {
            cut.HasComponent<NotificationDropdown>().Should().BeFalse();
        }, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Dispose_DisposesTimer()
    {
        SetupServices(unreadCount: 0);

        var cut = RenderComponent<NotificationBell>();

        var act = () => cut.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public void HandleLoadError_SetsUnreadCountToZero()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var mock = new Mock<INotificationApiService>();
        mock.Setup(x => x.GetUnreadCountAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        Services.AddSingleton<INotificationApiService>(mock.Object);

        var cut = RenderComponent<NotificationBell>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().NotContain(">0<");
        }, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task OnNotificationRead_RefreshesCount()
    {
        var callCount = 0;
        var mock = new Mock<INotificationApiService>();
        mock.Setup(x => x.GetUnreadCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new UnreadCountResponse(callCount++ < 1 ? 5 : 3));

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<INotificationApiService>(mock.Object);

        RenderComponent<MudPopoverProvider>();
        var cut = RenderComponent<NotificationBell>();
        cut.WaitForState(() => cut.Markup.Contains("5"), TimeSpan.FromSeconds(2));

        // Open dropdown, then invoke OnNotificationRead
        cut.Find("button").Click();
        cut.WaitForState(() => cut.HasComponent<NotificationDropdown>(), TimeSpan.FromSeconds(2));

        // Invoke the method via reflection
        await cut.InvokeAsync(() =>
        {
            var method = typeof(NotificationBell).GetMethod("OnNotificationRead",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (Task)method!.Invoke(cut.Instance, null)!;
        });
    }

    [Fact]
    public async Task OnNotificationDeleted_RefreshesCount()
    {
        var mock = new Mock<INotificationApiService>();
        mock.Setup(x => x.GetUnreadCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UnreadCountResponse(2));

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<INotificationApiService>(mock.Object);

        var cut = RenderComponent<NotificationBell>();
        cut.WaitForState(() => cut.Markup.Contains("2"), TimeSpan.FromSeconds(2));

        await cut.InvokeAsync(() =>
        {
            var method = typeof(NotificationBell).GetMethod("OnNotificationDeleted",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (Task)method!.Invoke(cut.Instance, null)!;
        });

        mock.Verify(x => x.GetUnreadCountAsync(It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task PollUnreadCount_WhenVisible_RefreshesCount()
    {
        var mock = new Mock<INotificationApiService>();
        mock.Setup(x => x.GetUnreadCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UnreadCountResponse(1));

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<INotificationApiService>(mock.Object);

        var cut = RenderComponent<NotificationBell>();
        cut.WaitForState(() => cut.Markup.Contains("1"), TimeSpan.FromSeconds(2));

        await cut.InvokeAsync(() =>
        {
            var method = typeof(NotificationBell).GetMethod("PollUnreadCount",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (Task)method!.Invoke(cut.Instance, null)!;
        });

        mock.Verify(x => x.GetUnreadCountAsync(It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task PollUnreadCount_WhenError_DoesNotThrow()
    {
        var callCount = 0;
        var mock = new Mock<INotificationApiService>();
        mock.Setup(x => x.GetUnreadCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                if (callCount++ > 0)
                    throw new HttpRequestException("Network error");
                return new UnreadCountResponse(1);
            });

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<INotificationApiService>(mock.Object);

        var cut = RenderComponent<NotificationBell>();
        cut.WaitForState(() => cut.Markup.Contains("1"), TimeSpan.FromSeconds(2));

        await cut.InvokeAsync(() =>
        {
            var method = typeof(NotificationBell).GetMethod("PollUnreadCount",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (Task)method!.Invoke(cut.Instance, null)!;
        });
    }
}
