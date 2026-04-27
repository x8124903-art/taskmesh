namespace MsNotifications.Infrastructure.Options;

public sealed class DatabaseOptions
{
    public string Type { get; set; } = string.Empty;
    public string DefaultConnection { get; set; } = string.Empty;
}
