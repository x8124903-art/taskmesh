using MassTransit;
using Microsoft.Extensions.Logging;
using MsNotifications.Application.UseCases.Notifications;
using MsNotifications.Infrastructure.Consumers;
using MsTasks.Domain.Events;

namespace MsNotifications.Tests.Infrastructure.Consumers;

public sealed class TaskAssignedEventConsumerShould
{
    private readonly Mock<INotificationService> _serviceMock;
    private readonly Mock<ICreateNotificationUseCase> _createNotificationMock;
    private readonly Mock<ILogger<TaskAssignedEventConsumer>> _loggerMock;
    private readonly TaskAssignedEventConsumer _consumer;

    public TaskAssignedEventConsumerShould()
    {
        _serviceMock = new Mock<INotificationService>();
        _createNotificationMock = new Mock<ICreateNotificationUseCase>();
        _loggerMock = new Mock<ILogger<TaskAssignedEventConsumer>>();
        _consumer = new TaskAssignedEventConsumer(_serviceMock.Object, _createNotificationMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_CreatesNotification_WhenEventNotProcessed()
    {
        // Arrange
        var evt = new TaskAssignedEvent(
            EventId: "event-123",
            OccurredAt: DateTime.UtcNow,
            TaskId: 1,
            ProjectId: 1,
            AssignedToUserId: 2,
            AssignedByUserId: 1,
            TaskTitle: "Test Task"
        );

        _serviceMock.Setup(x => x.IsEventProcessedAsync(evt.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _createNotificationMock.Setup(x => x.ExecuteAsync(It.IsAny<CreateNotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        var contextMock = new Mock<ConsumeContext<TaskAssignedEvent>>();
        contextMock.Setup(x => x.Message).Returns(evt);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _createNotificationMock.Verify(x => x.ExecuteAsync(
            It.Is<CreateNotificationRequest>(r => r.UserId == 2 && r.Type == "TaskAssigned"),
            It.IsAny<CancellationToken>()), Times.Once);
        _serviceMock.Verify(x => x.MarkEventAsProcessedAsync(evt.EventId, nameof(TaskAssignedEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_SkipsProcessing_WhenEventAlreadyProcessed()
    {
        // Arrange
        var evt = new TaskAssignedEvent("event-123", DateTime.UtcNow, 1, 1, 2, 1, "Test");
        _serviceMock.Setup(x => x.IsEventProcessedAsync(evt.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var contextMock = new Mock<ConsumeContext<TaskAssignedEvent>>();
        contextMock.Setup(x => x.Message).Returns(evt);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _createNotificationMock.Verify(x => x.ExecuteAsync(It.IsAny<CreateNotificationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Consume_SkipsSelfNotification_WhenAssignedToEqualsAssignedBy()
    {
        // Arrange
        var evt = new TaskAssignedEvent("event-123", DateTime.UtcNow, 1, 1, 1, 1, "Test");
        _serviceMock.Setup(x => x.IsEventProcessedAsync(evt.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var contextMock = new Mock<ConsumeContext<TaskAssignedEvent>>();
        contextMock.Setup(x => x.Message).Returns(evt);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _createNotificationMock.Verify(x => x.ExecuteAsync(It.IsAny<CreateNotificationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _serviceMock.Verify(x => x.MarkEventAsProcessedAsync(evt.EventId, nameof(TaskAssignedEvent), It.IsAny<CancellationToken>()), Times.Once);
    }
}
