using MassTransit;
using Microsoft.Extensions.Logging;
using MsNotifications.Application.Models;
using MsNotifications.Application.UseCases.Notifications;
using MsNotifications.Infrastructure.Consumers;
using MsProjects.Domain.Events;

namespace MsNotifications.Tests.Infrastructure.Consumers;

public sealed class MemberInvitedEventConsumerShould
{
    private readonly Mock<INotificationService> _serviceMock;
    private readonly Mock<ICreateNotificationUseCase> _createNotificationMock;
    private readonly Mock<ILogger<MemberInvitedEventConsumer>> _loggerMock;
    private readonly MemberInvitedEventConsumer _consumer;

    public MemberInvitedEventConsumerShould()
    {
        _serviceMock = new Mock<INotificationService>();
        _createNotificationMock = new Mock<ICreateNotificationUseCase>();
        _loggerMock = new Mock<ILogger<MemberInvitedEventConsumer>>();
        _consumer = new MemberInvitedEventConsumer(_serviceMock.Object, _createNotificationMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_CreatesNotificationForInviteeAndMarksProcessed()
    {
        // Arrange
        var evt = new MemberInvitedEvent(
            EventId: "event-123",
            OccurredAt: DateTime.UtcNow,
            ProjectId: 1,
            ProjectName: "Test Project",
            InvitedEmail: "test@example.com",
            RoleName: "Member",
            InvitedByUserId: 5,
            InvitedUserId: 42
        );

        _serviceMock.Setup(x => x.IsEventProcessedAsync(evt.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _createNotificationMock.Setup(x => x.ExecuteAsync(It.IsAny<CreateNotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        var contextMock = new Mock<ConsumeContext<MemberInvitedEvent>>();
        contextMock.Setup(x => x.Message).Returns(evt);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _createNotificationMock.Verify(x => x.ExecuteAsync(
            It.Is<CreateNotificationRequest>(r =>
                r.UserId == 42 &&
                r.Type == "MemberInvited" &&
                r.RelatedEntityType == "Project" &&
                r.RelatedEntityId == 1),
            It.IsAny<CancellationToken>()), Times.Once);
        _serviceMock.Verify(x => x.MarkEventAsProcessedAsync(evt.EventId, nameof(MemberInvitedEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_SkipsNotification_WhenInvitedUserIdIsNull()
    {
        // Arrange
        var evt = new MemberInvitedEvent(
            EventId: "event-456",
            OccurredAt: DateTime.UtcNow,
            ProjectId: 1,
            ProjectName: "Test Project",
            InvitedEmail: "newuser@example.com",
            RoleName: "Member",
            InvitedByUserId: 5,
            InvitedUserId: null
        );

        _serviceMock.Setup(x => x.IsEventProcessedAsync(evt.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var contextMock = new Mock<ConsumeContext<MemberInvitedEvent>>();
        contextMock.Setup(x => x.Message).Returns(evt);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _createNotificationMock.Verify(x => x.ExecuteAsync(It.IsAny<CreateNotificationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _serviceMock.Verify(x => x.MarkEventAsProcessedAsync(evt.EventId, nameof(MemberInvitedEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_SkipsAlreadyProcessedEvent()
    {
        // Arrange
        var evt = new MemberInvitedEvent("event-123", DateTime.UtcNow, 1, "Test", "test@example.com", "Member", 5);
        _serviceMock.Setup(x => x.IsEventProcessedAsync(evt.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var contextMock = new Mock<ConsumeContext<MemberInvitedEvent>>();
        contextMock.Setup(x => x.Message).Returns(evt);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _createNotificationMock.Verify(x => x.ExecuteAsync(It.IsAny<CreateNotificationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
