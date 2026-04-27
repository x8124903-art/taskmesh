using MassTransit;
using MsNotifications.Application.Models;
using MsNotifications.Application.UseCases.Notifications;
using MsNotifications.Domain.Services;
using MsProjects.Domain.Events;

namespace MsNotifications.Infrastructure.Consumers;

public sealed class MemberInvitedEventConsumer(
    INotificationService notificationService,
    ICreateNotificationUseCase createNotification,
    ILogger<MemberInvitedEventConsumer> logger) : IConsumer<MemberInvitedEvent>
{
    public async Task Consume(ConsumeContext<MemberInvitedEvent> context)
    {
        var evt = context.Message;

        logger.LogInformation("Processing MemberInvitedEvent {EventId} for Project {ProjectId}", evt.EventId, evt.ProjectId);

        if (await notificationService.IsEventProcessedAsync(evt.EventId))
        {
            logger.LogInformation("Event {EventId} already processed, skipping", evt.EventId);
            return;
        }

        if (!evt.InvitedUserId.HasValue)
        {
            logger.LogInformation("No InvitedUserId for email '{Email}', skipping notification for MemberInvitedEvent {EventId}", evt.InvitedEmail, evt.EventId);
            await notificationService.MarkEventAsProcessedAsync(evt.EventId, nameof(MemberInvitedEvent));
            return;
        }

        var request = new CreateNotificationRequest(
            UserId: evt.InvitedUserId.Value,
            Type: "MemberInvited",
            Title: "Invitación recibida",
            Message: $"Has sido invitado al proyecto '{evt.ProjectName}' como {evt.RoleName}",
            RelatedEntityType: "Project",
            RelatedEntityId: evt.ProjectId,
            RelatedProjectId: evt.ProjectId
        );

        await createNotification.ExecuteAsync(request);
        await notificationService.MarkEventAsProcessedAsync(evt.EventId, nameof(MemberInvitedEvent));
        logger.LogInformation("Created invitation notification for invitee {UserId} from MemberInvitedEvent {EventId}", evt.InvitedUserId.Value, evt.EventId);
    }
}
