using System.Diagnostics.CodeAnalysis;

namespace WebApp.Models.Notifications;

[ExcludeFromCodeCoverage]
public sealed record UnreadCountResponse(int UnreadCount);
