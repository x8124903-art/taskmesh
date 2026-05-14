using System.Diagnostics.CodeAnalysis;

namespace MsTasks.Infrastructure.Options;

[ExcludeFromCodeCoverage]
public sealed class DatabaseOptions
{
    public string Type { get; set; } = string.Empty;
    public string DefaultConnection { get; set; } = string.Empty;
}
