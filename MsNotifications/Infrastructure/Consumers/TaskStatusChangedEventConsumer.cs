using MassTransit;
using MsNotifications.Application.Models;
using MsNotifications.Application.UseCases.Notifications;
using MsNotifications.Domain.Services;
using MsTasks.Domain.Events;

namespace MsNotifications.Infrastructure.Consumers;

public sealed class TaskStatusChangedEventConsumer(
    INotificationService notificationService,
    ICreateNotificationUseCase createNotification,
    ILogger<TaskStatusChangedEventConsumer> logger) : IConsumer<TaskStatusChangedEvent>
{
    public async Task Consume(ConsumeContext<TaskStatusChangedEvent> context)
    {
        var evt = context.Message;

        logger.LogInformation("Processing TaskStatusChangedEvent {EventId} for Task {TaskId}", evt.EventId, evt.TaskId);

        if (await notificationService.IsEventProcessedAsync(evt.EventId))
        {
            logger.LogInformation("Event {EventId} already processed, skipping", evt.EventId);
            return;
        }

        if (evt.TaskCreatedByUserId != evt.ChangedByUserId)
        {
            var request = new CreateNotificationRequest(
                UserId: evt.TaskCreatedByUserId,
                Type: "TaskStatusChanged",
                Title: "Estado de tarea actualizado",
                Message: $"La tarea '{evt.TaskTitle}' cambió de '{evt.OldStatus}' a '{evt.NewStatus}'",
                RelatedEntityType: "Task",
                RelatedEntityId: evt.TaskId,
                RelatedProjectId: evt.ProjectId
            );

            await createNotification.ExecuteAsync(request);
            logger.LogInformation("Created notification for task creator {UserId}", evt.TaskCreatedByUserId);
        }

        if (evt.AssignedToUserId.HasValue && evt.AssignedToUserId != evt.ChangedByUserId && evt.AssignedToUserId != evt.TaskCreatedByUserId)
        {
            var request = new CreateNotificationRequest(
                UserId: evt.AssignedToUserId.Value,
                Type: "TaskStatusChanged",
                Title: "Estado de tarea actualizado",
                Message: $"La tarea '{evt.TaskTitle}' cambió de '{evt.OldStatus}' a '{evt.NewStatus}'",
                RelatedEntityType: "Task",
                RelatedEntityId: evt.TaskId,
                RelatedProjectId: evt.ProjectId
            );

            await createNotification.ExecuteAsync(request);
            logger.LogInformation("Created notification for assigned user {UserId}", evt.AssignedToUserId.Value);
        }

        await notificationService.MarkEventAsProcessedAsync(evt.EventId, nameof(TaskStatusChangedEvent));
        logger.LogInformation("Processed TaskStatusChangedEvent {EventId}", evt.EventId);
    }
}
