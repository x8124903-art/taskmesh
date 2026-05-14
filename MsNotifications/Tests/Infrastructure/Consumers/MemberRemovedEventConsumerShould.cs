using MassTransit;
using Microsoft.Extensions.Logging;
using MsNotifications.Application.UseCases.Notifications;
using MsNotifications.Infrastructure.Consumers;
using MsProjects.Domain.Events;

namespace MsNotifications.Tests.Infrastructure.Consumers;

public sealed class MemberRemovedEventConsumerShould
{
    private readonly Mock<INotificationService> _serviceMock;
    private readonly Mock<ICreateNotificationUseCase> _createNotificationMock;
    private readonly Mock<ILogger<MemberRemovedEventConsumer>> _loggerMock;
    private readonly MemberRemovedEventConsumer _consumer;

    public MemberRemovedEventConsumerShould()
    {
        _serviceMock = new Mock<INotificationService>();
        _createNotificationMock = new Mock<ICreateNotificationUseCase>();
        _loggerMock = new Mock<ILogger<MemberRemovedEventConsumer>>();
        _consumer = new MemberRemovedEventConsumer(_serviceMock.Object, _createNotificationMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_NotifiesRemovedUser_WhenNotSelfRemoval()
    {
        // Arrange
        var evt = new MemberRemovedEvent(
            EventId: "event-123",
            OccurredAt: DateTime.UtcNow,
            ProjectId: 1,
            ProjectName: "Test Project",
            RemovedUserId: 2,
            RemovedByUserId: 1
        );

        _serviceMock.Setup(x => x.IsEventProcessedAsync(evt.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var contextMock = new Mock<ConsumeContext<MemberRemovedEvent>>();
        contextMock.Setup(x => x.Message).Returns(evt);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _createNotificationMock.Verify(x => x.ExecuteAsync(
            It.Is<CreateNotificationRequest>(r => r.UserId == 2 && r.Type == "MemberRemoved"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_SkipsNotification_WhenSelfRemoval()
    {
        // Arrange
        var evt = new MemberRemovedEvent("event-123", DateTime.UtcNow, 1, "Test", 1, 1);
        _serviceMock.Setup(x => x.IsEventProcessedAsync(evt.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var contextMock = new Mock<ConsumeContext<MemberRemovedEvent>>();
        contextMock.Setup(x => x.Message).Returns(evt);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _createNotificationMock.Verify(x => x.ExecuteAsync(It.IsAny<CreateNotificationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
