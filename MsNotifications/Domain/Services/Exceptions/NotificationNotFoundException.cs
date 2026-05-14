using System.Diagnostics.CodeAnalysis;

namespace MsNotifications.Domain.Services.Exceptions;

[ExcludeFromCodeCoverage]
public sealed class NotificationNotFoundException : DomainException
{
    public NotificationNotFoundException(int idNotification) 
        : base($"Notification with ID {idNotification} not found")
    {
    }
}
