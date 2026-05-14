using MassTransit;
using Microsoft.Extensions.Logging;
using MsNotifications.Application.Models;
using MsNotifications.Application.UseCases.Notifications;
using MsNotifications.Infrastructure.Consumers;
using MsProjects.Domain.Events;

namespace MsNotifications.Tests.Infrastructure.Consumers;

public sealed class MemberJoinedEventConsumerShould
{
    [Fact]
    public async Task Consume_CreatesNotificationAndMarksProcessed()
    {
        // Arrange
        var serviceMock = new Mock<INotificationService>();
        var createNotificationMock = new Mock<ICreateNotificationUseCase>();
        var loggerMock = new Mock<ILogger<MemberJoinedEventConsumer>>();
        var consumer = new MemberJoinedEventConsumer(serviceMock.Object, createNotificationMock.Object, loggerMock.Object);

        var evt = new MemberJoinedEvent(
            EventId: "event-123",
            OccurredAt: DateTime.UtcNow,
            ProjectId: 1,
            ProjectName: "Test Project",
            UserId: 2,
            RoleName: "Member"
        );

        serviceMock.Setup(x => x.IsEventProcessedAsync(evt.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var contextMock = new Mock<ConsumeContext<MemberJoinedEvent>>();
        contextMock.Setup(x => x.Message).Returns(evt);

        // Act
        await consumer.Consume(contextMock.Object);

        // Assert
        createNotificationMock.Verify(x => x.ExecuteAsync(
            It.Is<CreateNotificationRequest>(r =>
                r.UserId == 2 &&
                r.Type == "MemberJoined" &&
                r.RelatedEntityType == "Project"),
            It.IsAny<CancellationToken>()), Times.Once);
        serviceMock.Verify(x => x.MarkEventAsProcessedAsync(evt.EventId, nameof(MemberJoinedEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_SkipsAlreadyProcessedEvent()
    {
        // Arrange
        var serviceMock = new Mock<INotificationService>();
        var createNotificationMock = new Mock<ICreateNotificationUseCase>();
        var loggerMock = new Mock<ILogger<MemberJoinedEventConsumer>>();
        var consumer = new MemberJoinedEventConsumer(serviceMock.Object, createNotificationMock.Object, loggerMock.Object);

        var evt = new MemberJoinedEvent("event-123", DateTime.UtcNow, 1, "Test", 2, "Member");

        serviceMock.Setup(x => x.IsEventProcessedAsync(evt.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var contextMock = new Mock<ConsumeContext<MemberJoinedEvent>>();
        contextMock.Setup(x => x.Message).Returns(evt);

        // Act
        await consumer.Consume(contextMock.Object);

        // Assert
        createNotificationMock.Verify(x => x.ExecuteAsync(
            It.IsAny<CreateNotificationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
