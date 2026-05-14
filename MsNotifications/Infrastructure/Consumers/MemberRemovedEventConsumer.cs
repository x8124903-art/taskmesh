using MassTransit;
using MsNotifications.Application.Models;
using MsNotifications.Application.UseCases.Notifications;
using MsNotifications.Domain.Services;
using MsProjects.Domain.Events;

namespace MsNotifications.Infrastructure.Consumers;

public sealed class MemberRemovedEventConsumer(
    INotificationService notificationService,
    ICreateNotificationUseCase createNotification,
    ILogger<MemberRemovedEventConsumer> logger) : IConsumer<MemberRemovedEvent>
{
    public async Task Consume(ConsumeContext<MemberRemovedEvent> context)
    {
        var evt = context.Message;

        logger.LogInformation("Processing MemberRemovedEvent {EventId} for Project {ProjectId}", evt.EventId, evt.ProjectId);

        if (await notificationService.IsEventProcessedAsync(evt.EventId))
        {
            logger.LogInformation("Event {EventId} already processed, skipping", evt.EventId);
            return;
        }

        if (evt.RemovedUserId != evt.RemovedByUserId)
        {
            var request = new CreateNotificationRequest(
                UserId: evt.RemovedUserId,
                Type: "MemberRemoved",
                Title: "Eliminado de proyecto",
                Message: $"Has sido eliminado del proyecto '{evt.ProjectName}'",
                RelatedEntityType: "Project",
                RelatedEntityId: evt.ProjectId,
                RelatedProjectId: evt.ProjectId
            );

            await createNotification.ExecuteAsync(request);
            logger.LogInformation("Created notification for removed user {UserId}", evt.RemovedUserId);
        }

        await notificationService.MarkEventAsProcessedAsync(evt.EventId, nameof(MemberRemovedEvent));
        logger.LogInformation("Processed MemberRemovedEvent {EventId}", evt.EventId);
    }
}
