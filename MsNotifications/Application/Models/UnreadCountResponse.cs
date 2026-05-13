using System.Diagnostics.CodeAnalysis;

namespace MsNotifications.Application.Models;

[ExcludeFromCodeCoverage]
public sealed record UnreadCountResponse(
    int UnreadCount
);
