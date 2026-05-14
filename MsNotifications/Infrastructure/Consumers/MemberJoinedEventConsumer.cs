using MassTransit;
using MsNotifications.Application.Models;
using MsNotifications.Application.UseCases.Notifications;
using MsNotifications.Domain.Services;
using MsProjects.Domain.Events;

namespace MsNotifications.Infrastructure.Consumers;

public sealed class MemberJoinedEventConsumer(
    INotificationService notificationService,
    ICreateNotificationUseCase createNotification,
    ILogger<MemberJoinedEventConsumer> logger) : IConsumer<MemberJoinedEvent>
{
    public async Task Consume(ConsumeContext<MemberJoinedEvent> context)
    {
        var evt = context.Message;

        logger.LogInformation("Processing MemberJoinedEvent {EventId} for Project {ProjectId}", evt.EventId, evt.ProjectId);

        if (await notificationService.IsEventProcessedAsync(evt.EventId))
        {
            logger.LogInformation("Event {EventId} already processed, skipping", evt.EventId);
            return;
        }

        var request = new CreateNotificationRequest(
            UserId: evt.UserId,
            Type: "MemberJoined",
            Title: "Te has unido a un proyecto",
            Message: $"Te has unido al proyecto '{evt.ProjectName}' como {evt.RoleName}",
            RelatedEntityType: "Project",
            RelatedEntityId: evt.ProjectId,
            RelatedProjectId: evt.ProjectId
        );

        await createNotification.ExecuteAsync(request);
        await notificationService.MarkEventAsProcessedAsync(evt.EventId, nameof(MemberJoinedEvent));
        logger.LogInformation("Created notification for user {UserId} from MemberJoinedEvent {EventId}", evt.UserId, evt.EventId);
    }
}
