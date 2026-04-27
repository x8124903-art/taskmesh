using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Moq;
using WebApp.Services;
using WebApp.Models.Notifications;
using WebApp.Components.Common;

namespace WebApp.Tests.Components.Common;

public sealed class NotificationDropdownShould : TestContext
{
    private readonly Mock<INotificationApiService> _notificationServiceMock;
    private readonly Mock<ISnackbar> _snackbarMock;

    public NotificationDropdownShould()
    {
        _notificationServiceMock = new Mock<INotificationApiService>();
        _snackbarMock = new Mock<ISnackbar>();
    }

    private void SetupServices(List<NotificationModel>? notifications = null)
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var unread = notifications?.Where(n => !n.IsRead).ToList() ?? new List<NotificationModel>();
        var read = notifications?.Where(n => n.IsRead).ToList() ?? new List<NotificationModel>();

        _notificationServiceMock.Setup(x => x.GetNotificationsAsync(
            false, 
            It.IsAny<string?>(), 
            1, 
            100,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedNotificationsResponse(unread, unread.Count, 1, 100));

        _notificationServiceMock.Setup(x => x.GetNotificationsAsync(
            true, 
            It.IsAny<string?>(), 
            1, 
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedNotificationsResponse(read, read.Count, 1, 10));

        Services.AddSingleton<INotificationApiService>(_notificationServiceMock.Object);
        Services.AddSingleton(_snackbarMock.Object);
    }

    private string GetPopoverMarkup()
    {
        var provider = RenderComponent<MudPopoverProvider>();
        return provider.Markup;
    }

    [Fact]
    public void RenderNotificationsList_WhenOpen()
    {
        var notifications = new List<NotificationModel>
        {
            new(1, 1, "TaskAssigned", "Nueva tarea", "Se te ha asignado la tarea 'Test'", 
                "Task", 101, 201, false, DateTime.UtcNow)
        };

        SetupServices(notifications);

        var provider = RenderComponent<MudPopoverProvider>();
        var cut = RenderComponent<NotificationDropdown>(parameters => parameters
            .Add(p => p.IsOpen, true));

        cut.WaitForState(() => provider.Markup.Contains("Nueva tarea"), TimeSpan.FromSeconds(2));

        var popoverMarkup = provider.Markup;
        popoverMarkup.Should().Contain("Nueva tarea");
        popoverMarkup.Should().Contain("Se te ha asignado la tarea");
    }

    [Fact]
    public void ShowEmptyState_WhenNoNotifications()
    {
        SetupServices(new List<NotificationModel>());

        var provider = RenderComponent<MudPopoverProvider>();
        var cut = RenderComponent<NotificationDropdown>(parameters => parameters
            .Add(p => p.IsOpen, true));

        cut.WaitForState(() => provider.Markup.Contains("No hay notificaciones"), TimeSpan.FromSeconds(2));

        provider.Markup.Should().Contain("No hay notificaciones");
    }

    [Fact]
    public void MarkAsRead_OnNotificationClick()
    {
        var notifications = new List<NotificationModel>
        {
            new(1, 1, "TaskAssigned", "Nueva tarea", "Test message", 
                "Task", 101, 201, false, DateTime.UtcNow)
        };

        SetupServices(notifications);

        _notificationServiceMock.Setup(x => x.MarkAsReadAsync(1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var provider = RenderComponent<MudPopoverProvider>();
        var cut = RenderComponent<NotificationDropdown>(parameters => parameters
            .Add(p => p.IsOpen, true));

        cut.WaitForState(() => provider.Markup.Contains("Nueva tarea"), TimeSpan.FromSeconds(2));

        var listItem = provider.Find(".mud-list-item");
        listItem.Click();

        _notificationServiceMock.Verify(x => x.MarkAsReadAsync(1, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public void NotShowMarkAllButton_WhenOpen()
    {
        var notifications = new List<NotificationModel>
        {
            new(1, 1, "TaskAssigned", "Nueva tarea", "Test", "Task", 101, 201, false, DateTime.UtcNow),
            new(2, 1, "TaskAssigned", "Otra tarea", "Test2", "Task", 102, 201, false, DateTime.UtcNow)
        };

        SetupServices(notifications);

        var provider = RenderComponent<MudPopoverProvider>();
        var cut = RenderComponent<NotificationDropdown>(parameters => parameters
            .Add(p => p.IsOpen, true));

        cut.WaitForState(() => provider.Markup.Contains("Nueva tarea"), TimeSpan.FromSeconds(2));

        var markAllButton = provider.FindAll("button")
            .FirstOrDefault(b => b.TextContent.Contains("Marcar todas"));
        
        markAllButton.Should().BeNull("'Marcar todas' button should not exist");
    }

    [Fact]
    public void DeleteNotification_OnDeleteButtonClick()
    {
        var notifications = new List<NotificationModel>
        {
            new(1, 1, "TaskAssigned", "Nueva tarea", "Test", "Task", 101, 201, false, DateTime.UtcNow)
        };

        SetupServices(notifications);

        _notificationServiceMock.Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var provider = RenderComponent<MudPopoverProvider>();
        var cut = RenderComponent<NotificationDropdown>(parameters => parameters
            .Add(p => p.IsOpen, true));

        cut.WaitForState(() => provider.Markup.Contains("Nueva tarea"), TimeSpan.FromSeconds(2));

        var deleteButton = provider.FindAll("button.mud-icon-button")
            .FirstOrDefault(b => b.ClassList.Any(c => c.Contains("error")) || 
                                  b.GetAttribute("style")?.Contains("margin-left") == true);

        if (deleteButton == null)
        {
            var iconButtons = provider.FindAll(".mud-list-item button.mud-icon-button").ToList();
            deleteButton = iconButtons.LastOrDefault();
        }

        if (deleteButton == null)
        {
            var allIconButtons = provider.FindAll("button.mud-icon-button").ToList();
            deleteButton = allIconButtons.Count > 1 ? allIconButtons.Last() : allIconButtons.FirstOrDefault();
        }

        deleteButton.Should().NotBeNull("delete button should exist in popover");
        deleteButton!.Click();

        _notificationServiceMock.Verify(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()), 
            Times.Once);
    }
}
