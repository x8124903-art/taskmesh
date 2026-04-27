using MassTransit;
using MsNotifications.Application.Models;
using MsNotifications.Application.UseCases.Notifications;
using MsNotifications.Domain.Services;
using MsTasks.Domain.Events;

namespace MsNotifications.Infrastructure.Consumers;

public sealed class TaskAssignedEventConsumer(
    INotificationService notificationService,
    ICreateNotificationUseCase createNotification,
    ILogger<TaskAssignedEventConsumer> logger) : IConsumer<TaskAssignedEvent>
{
    public async Task Consume(ConsumeContext<TaskAssignedEvent> context)
    {
        var evt = context.Message;

        logger.LogInformation("Processing TaskAssignedEvent {EventId} for Task {TaskId}", evt.EventId, evt.TaskId);

        if (await notificationService.IsEventProcessedAsync(evt.EventId))
        {
            logger.LogInformation("Event {EventId} already processed, skipping", evt.EventId);
            return;
        }

        if (evt.AssignedToUserId == evt.AssignedByUserId)
        {
            logger.LogInformation("Skipping self-notification for user {UserId}", evt.AssignedToUserId);
            await notificationService.MarkEventAsProcessedAsync(evt.EventId, nameof(TaskAssignedEvent));
            return;
        }

        var request = new CreateNotificationRequest(
            UserId: evt.AssignedToUserId,
            Type: "TaskAssigned",
            Title: "Tarea asignada",
            Message: $"Se te ha asignado la tarea '{evt.TaskTitle}'",
            RelatedEntityType: "Task",
            RelatedEntityId: evt.TaskId,
            RelatedProjectId: evt.ProjectId
        );

        await createNotification.ExecuteAsync(request);
        await notificationService.MarkEventAsProcessedAsync(evt.EventId, nameof(TaskAssignedEvent));

        logger.LogInformation("Created notification for user {UserId} from TaskAssignedEvent {EventId}", evt.AssignedToUserId, evt.EventId);
    }
}
