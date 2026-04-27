namespace MsNotifications.Domain.Services.Exceptions;

public sealed class NotificationNotFoundException : DomainException
{
    public NotificationNotFoundException(int idNotification) 
        : base($"Notification with ID {idNotification} not found")
    {
    }
}
