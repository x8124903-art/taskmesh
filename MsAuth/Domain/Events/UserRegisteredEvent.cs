namespace MsAuth.Domain.Events;

public sealed record UserRegisteredEvent(
    string EventId,
    DateTime OccurredAt,
    int UserId,
    string Email,
    string Name
);
