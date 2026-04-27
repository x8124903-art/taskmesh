using MassTransit;
using MsNotifications.Application.Models;
using MsNotifications.Application.UseCases.Notifications;
using MsNotifications.Domain.Services;
using MsTasks.Domain.Events;

namespace MsNotifications.Infrastructure.Consumers;

public sealed class TaskCommentAddedEventConsumer(
    INotificationService notificationService,
    ICreateNotificationUseCase createNotification,
    ILogger<TaskCommentAddedEventConsumer> logger) : IConsumer<TaskCommentAddedEvent>
{
    public async Task Consume(ConsumeContext<TaskCommentAddedEvent> context)
    {
        var evt = context.Message;

        logger.LogInformation("Processing TaskCommentAddedEvent {EventId} for Task {TaskId}", evt.EventId, evt.TaskId);

        if (await notificationService.IsEventProcessedAsync(evt.EventId))
        {
            logger.LogInformation("Event {EventId} already processed, skipping", evt.EventId);
            return;
        }

        if (evt.TaskCreatedByUserId != evt.AuthorUserId)
        {
            var request = new CreateNotificationRequest(
                UserId: evt.TaskCreatedByUserId,
                Type: "TaskCommentAdded",
                Title: "Nuevo comentario en tarea",
                Message: $"Hay un nuevo comentario en la tarea '{evt.TaskTitle}'",
                RelatedEntityType: "Task",
                RelatedEntityId: evt.TaskId,
                RelatedProjectId: evt.ProjectId
            );

            await createNotification.ExecuteAsync(request);
            logger.LogInformation("Created notification for task creator {UserId}", evt.TaskCreatedByUserId);
        }

        if (evt.TaskAssignedToUserId.HasValue && evt.TaskAssignedToUserId != evt.AuthorUserId && evt.TaskAssignedToUserId != evt.TaskCreatedByUserId)
        {
            var request = new CreateNotificationRequest(
                UserId: evt.TaskAssignedToUserId.Value,
                Type: "TaskCommentAdded",
                Title: "Nuevo comentario en tarea",
                Message: $"Hay un nuevo comentario en la tarea '{evt.TaskTitle}'",
                RelatedEntityType: "Task",
                RelatedEntityId: evt.TaskId,
                RelatedProjectId: evt.ProjectId
            );

            await createNotification.ExecuteAsync(request);
            logger.LogInformation("Created notification for assigned user {UserId}", evt.TaskAssignedToUserId.Value);
        }

        await notificationService.MarkEventAsProcessedAsync(evt.EventId, nameof(TaskCommentAddedEvent));
        logger.LogInformation("Processed TaskCommentAddedEvent {EventId}", evt.EventId);
    }
}
