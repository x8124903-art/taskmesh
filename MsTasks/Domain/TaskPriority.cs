using System.Text.Json.Serialization;

namespace MsTasks.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskPriority
{
    Low = 1,
    Medium = 2,
    High = 3
}
