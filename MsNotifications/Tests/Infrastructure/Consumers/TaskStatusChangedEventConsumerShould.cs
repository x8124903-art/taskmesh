using MassTransit;
using Microsoft.Extensions.Logging;
using MsNotifications.Application.UseCases.Notifications;
using MsNotifications.Infrastructure.Consumers;
using MsTasks.Domain.Events;

namespace MsNotifications.Tests.Infrastructure.Consumers;

public sealed class TaskStatusChangedEventConsumerShould
{
    private readonly Mock<INotificationService> _serviceMock;
    private readonly Mock<ICreateNotificationUseCase> _createNotificationMock;
    private readonly Mock<ILogger<TaskStatusChangedEventConsumer>> _loggerMock;
    private readonly TaskStatusChangedEventConsumer _consumer;

    public TaskStatusChangedEventConsumerShould()
    {
        _serviceMock = new Mock<INotificationService>();
        _createNotificationMock = new Mock<ICreateNotificationUseCase>();
        _loggerMock = new Mock<ILogger<TaskStatusChangedEventConsumer>>();
        _consumer = new TaskStatusChangedEventConsumer(_serviceMock.Object, _createNotificationMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_NotifiesCreatorAndAssigned_WhenBothDifferentFromChanger()
    {
        // Arrange
        var evt = new TaskStatusChangedEvent(
            EventId: "event-123",
            OccurredAt: DateTime.UtcNow,
            TaskId: 1,
            ProjectId: 1,
            OldStatus: "Todo",
            NewStatus: "Done",
            ChangedByUserId: 1,
            TaskTitle: "Test Task",
            AssignedToUserId: 3,
            TaskCreatedByUserId: 2
        );

        _serviceMock.Setup(x => x.IsEventProcessedAsync(evt.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var contextMock = new Mock<ConsumeContext<TaskStatusChangedEvent>>();
        contextMock.Setup(x => x.Message).Returns(evt);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _createNotificationMock.Verify(x => x.ExecuteAsync(
            It.Is<CreateNotificationRequest>(r => r.UserId == 2),
            It.IsAny<CancellationToken>()), Times.Once);
        _createNotificationMock.Verify(x => x.ExecuteAsync(
            It.Is<CreateNotificationRequest>(r => r.UserId == 3),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_SkipsCreatorNotification_WhenCreatorIsChanger()
    {
        // Arrange
        var evt = new TaskStatusChangedEvent("event-123", DateTime.UtcNow, 1, 1, "Todo", "Done", 1, "Test", 2, 1);
        _serviceMock.Setup(x => x.IsEventProcessedAsync(evt.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var contextMock = new Mock<ConsumeContext<TaskStatusChangedEvent>>();
        contextMock.Setup(x => x.Message).Returns(evt);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _createNotificationMock.Verify(x => x.ExecuteAsync(
            It.Is<CreateNotificationRequest>(r => r.UserId == 1),
            It.IsAny<CancellationToken>()), Times.Never);
        _createNotificationMock.Verify(x => x.ExecuteAsync(
            It.Is<CreateNotificationRequest>(r => r.UserId == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_SkipsDuplicateNotification_WhenCreatorIsAssigned()
    {
        // Arrange
        var evt = new TaskStatusChangedEvent("event-456", DateTime.UtcNow, 1, 1, "Todo", "Done", 1, "Test", 2, 2);
        _serviceMock.Setup(x => x.IsEventProcessedAsync(evt.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var contextMock = new Mock<ConsumeContext<TaskStatusChangedEvent>>();
        contextMock.Setup(x => x.Message).Returns(evt);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _createNotificationMock.Verify(x => x.ExecuteAsync(
            It.Is<CreateNotificationRequest>(r => r.UserId == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
