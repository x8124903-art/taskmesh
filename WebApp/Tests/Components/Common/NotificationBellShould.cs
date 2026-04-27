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
}
