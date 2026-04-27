using MassTransit;
using Microsoft.Extensions.Logging;
using MsNotifications.Application.UseCases.Notifications;
using MsNotifications.Infrastructure.Consumers;
using MsTasks.Domain.Events;

namespace MsNotifications.Tests.Infrastructure.Consumers;

public sealed class TaskCommentAddedEventConsumerShould
{
    private readonly Mock<INotificationService> _serviceMock;
    private readonly Mock<ICreateNotificationUseCase> _createNotificationMock;
    private readonly Mock<ILogger<TaskCommentAddedEventConsumer>> _loggerMock;
    private readonly TaskCommentAddedEventConsumer _consumer;

    public TaskCommentAddedEventConsumerShould()
    {
        _serviceMock = new Mock<INotificationService>();
        _createNotificationMock = new Mock<ICreateNotificationUseCase>();
        _loggerMock = new Mock<ILogger<TaskCommentAddedEventConsumer>>();
        _consumer = new TaskCommentAddedEventConsumer(_serviceMock.Object, _createNotificationMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_NotifiesCreatorAndAssigned_WhenBothDifferentFromAuthor()
    {
        // Arrange
        var evt = new TaskCommentAddedEvent(
            EventId: "event-123",
            OccurredAt: DateTime.UtcNow,
            TaskId: 1,
            ProjectId: 1,
            CommentId: 1,
            AuthorUserId: 1,
            TaskTitle: "Test Task",
            TaskCreatedByUserId: 2,
            TaskAssignedToUserId: 3
        );

        _serviceMock.Setup(x => x.IsEventProcessedAsync(evt.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var contextMock = new Mock<ConsumeContext<TaskCommentAddedEvent>>();
        contextMock.Setup(x => x.Message).Returns(evt);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _createNotificationMock.Verify(x => x.ExecuteAsync(
            It.Is<CreateNotificationRequest>(r => r.UserId == 2 && r.Type == "TaskCommentAdded"),
            It.IsAny<CancellationToken>()), Times.Once);
        _createNotificationMock.Verify(x => x.ExecuteAsync(
            It.Is<CreateNotificationRequest>(r => r.UserId == 3 && r.Type == "TaskCommentAdded"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_SkipsSelfNotification_WhenCreatorIsAuthor()
    {
        // Arrange
        var evt = new TaskCommentAddedEvent("event-123", DateTime.UtcNow, 1, 1, 1, 1, "Test", 1, 2);
        _serviceMock.Setup(x => x.IsEventProcessedAsync(evt.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var contextMock = new Mock<ConsumeContext<TaskCommentAddedEvent>>();
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
    public async Task Consume_DoesNotDuplicate_WhenCreatorIsAlsoAssigned()
    {
        // Arrange
        var evt = new TaskCommentAddedEvent("event-456", DateTime.UtcNow, 1, 1, 1, 1, "Test", 2, 2);
        _serviceMock.Setup(x => x.IsEventProcessedAsync(evt.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var contextMock = new Mock<ConsumeContext<TaskCommentAddedEvent>>();
        contextMock.Setup(x => x.Message).Returns(evt);

        // Act
        await _consumer.Consume(contextMock.Object);

        _createNotificationMock.Verify(x => x.ExecuteAsync(
            It.Is<CreateNotificationRequest>(r => r.UserId == 2 && r.Type == "TaskCommentAdded"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
