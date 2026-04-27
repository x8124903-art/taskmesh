using System.Text.Json.Serialization;

namespace MsTasks.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskStatus
{
    Todo = 1,
    InProgress = 2,
    Review = 3,
    Testing = 4,
    Done = 5,
    Blocked = 6
}
