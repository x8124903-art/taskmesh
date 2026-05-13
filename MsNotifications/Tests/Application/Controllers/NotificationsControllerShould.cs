using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MsNotifications.Application.Controllers;
using MsNotifications.Application.Models;
using MsNotifications.Application.UseCases.Notifications;

namespace MsNotifications.Tests.Application.Controllers;

public sealed class NotificationsControllerShould
{
    [Fact]
    public async Task GetAll_Returns200WithPagedResult()
    {
        // Arrange
        const int GIVEN_USER_ID = 10;
        var expected = new PagedNotificationsResponse(
            new List<NotificationResponse>
            {
                new(1, "TaskAssigned", "Title", "Message", "Task", 1, 1, false, DateTime.UtcNow)
            },
            1, 20, 1);

        var useCaseMock = new Mock<IGetNotificationsUseCase>();
        useCaseMock
            .Setup(x => x.ExecuteAsync(GIVEN_USER_ID, null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = CreateController(getNotifications: useCaseMock.Object);
        controller.ControllerContext = CreateControllerContext(GIVEN_USER_ID);

        // Act
        var result = await controller.GetAll(null, null, 1, 20);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetAll_PassesIsReadFilter_ToUseCase()
    {
        // Arrange
        const int GIVEN_USER_ID = 10;
        var useCaseMock = new Mock<IGetNotificationsUseCase>();
        var controller = CreateController(getNotifications: useCaseMock.Object);
        controller.ControllerContext = CreateControllerContext(GIVEN_USER_ID);

        // Act
        await controller.GetAll(isRead: true, null, 1, 20);

        // Assert
        useCaseMock.Verify(x => x.ExecuteAsync(GIVEN_USER_ID, true, null, 1, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_PassesTypeFilter_ToUseCase()
    {
        // Arrange
        const int GIVEN_USER_ID = 10;
        var useCaseMock = new Mock<IGetNotificationsUseCase>();
        var controller = CreateController(getNotifications: useCaseMock.Object);
        controller.ControllerContext = CreateControllerContext(GIVEN_USER_ID);

        // Act
        await controller.GetAll(null, "TaskAssigned", 1, 20);

        // Assert
        useCaseMock.Verify(x => x.ExecuteAsync(GIVEN_USER_ID, null, "TaskAssigned", 1, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUnreadCount_Returns200WithCount()
    {
        // Arrange
        const int GIVEN_USER_ID = 10;
        var expected = new UnreadCountResponse(5);

        var useCaseMock = new Mock<IGetUnreadCountUseCase>();
        useCaseMock
            .Setup(x => x.ExecuteAsync(GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = CreateController(getUnreadCount: useCaseMock.Object);
        controller.ControllerContext = CreateControllerContext(GIVEN_USER_ID);

        // Act
        var result = await controller.GetUnreadCount();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task MarkAsRead_Returns204_WhenSuccessful()
    {
        // Arrange
        const int GIVEN_USER_ID = 10;
        const int GIVEN_NOTIFICATION_ID = 5;

        var useCaseMock = new Mock<IMarkNotificationAsReadUseCase>();
        var controller = CreateController(markAsRead: useCaseMock.Object);
        controller.ControllerContext = CreateControllerContext(GIVEN_USER_ID);

        // Act
        var result = await controller.MarkAsRead(GIVEN_NOTIFICATION_ID);

        // Assert
        var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
        noContentResult.StatusCode.Should().Be(204);
        useCaseMock.Verify(x => x.ExecuteAsync(GIVEN_NOTIFICATION_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAllAsRead_Returns200WithUpdatedCount()
    {
        // Arrange
        const int GIVEN_USER_ID = 10;
        const int UPDATED_COUNT = 3;

        var useCaseMock = new Mock<IMarkAllNotificationsAsReadUseCase>();
        useCaseMock
            .Setup(x => x.ExecuteAsync(GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(UPDATED_COUNT);

        var controller = CreateController(markAllAsRead: useCaseMock.Object);
        controller.ControllerContext = CreateControllerContext(GIVEN_USER_ID);

        // Act
        var result = await controller.MarkAllAsRead();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(new { UpdatedCount = UPDATED_COUNT });
    }

    [Fact]
    public async Task Delete_Returns204_WhenSuccessful()
    {
        // Arrange
        const int GIVEN_USER_ID = 10;
        const int GIVEN_NOTIFICATION_ID = 5;

        var useCaseMock = new Mock<IDeleteNotificationUseCase>();
        var controller = CreateController(deleteNotification: useCaseMock.Object);
        controller.ControllerContext = CreateControllerContext(GIVEN_USER_ID);

        // Act
        var result = await controller.Delete(GIVEN_NOTIFICATION_ID);

        // Assert
        var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
        noContentResult.StatusCode.Should().Be(204);
        useCaseMock.Verify(x => x.ExecuteAsync(GIVEN_NOTIFICATION_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserIdFromHeaders_ThrowsUnauthorized_WhenHeaderMissing()
    {
        // Arrange
        var controller = CreateController();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act & Assert
        var act = async () => await controller.GetAll(null, null, 1, 20);
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("User ID is missing or invalid");
    }

    [Fact]
    public async Task GetUserIdFromHeaders_ThrowsUnauthorized_WhenHeaderInvalid()
    {
        // Arrange
        var controller = CreateController();
        controller.ControllerContext = CreateControllerContext("invalid");

        // Act & Assert
        var act = async () => await controller.GetAll(null, null, 1, 20);
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("User ID is missing or invalid");
    }

    private static NotificationsController CreateController(
        IGetNotificationsUseCase? getNotifications = null,
        IGetUnreadCountUseCase? getUnreadCount = null,
        IMarkNotificationAsReadUseCase? markAsRead = null,
        IMarkAllNotificationsAsReadUseCase? markAllAsRead = null,
        IDeleteNotificationUseCase? deleteNotification = null)
    {
        return new NotificationsController(
            getNotifications ?? Mock.Of<IGetNotificationsUseCase>(),
            getUnreadCount ?? Mock.Of<IGetUnreadCountUseCase>(),
            markAsRead ?? Mock.Of<IMarkNotificationAsReadUseCase>(),
            markAllAsRead ?? Mock.Of<IMarkAllNotificationsAsReadUseCase>(),
            deleteNotification ?? Mock.Of<IDeleteNotificationUseCase>());
    }

    private static ControllerContext CreateControllerContext(int userId)
    {
        return CreateControllerContext(userId.ToString());
    }

    private static ControllerContext CreateControllerContext(string userIdHeader)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-User-Id"] = userIdHeader;

        return new ControllerContext
        {
            HttpContext = httpContext
        };
    }
}
