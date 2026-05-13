using System.Diagnostics.CodeAnalysis;

namespace MsNotifications.Infrastructure.Options;

[ExcludeFromCodeCoverage]
public sealed class DatabaseOptions
{
    public string Type { get; set; } = string.Empty;
    public string DefaultConnection { get; set; } = string.Empty;
}
