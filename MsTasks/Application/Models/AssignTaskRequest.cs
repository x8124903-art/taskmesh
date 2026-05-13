using System.Diagnostics.CodeAnalysis;

namespace MsTasks.Application.Models;

[ExcludeFromCodeCoverage]
public sealed record AssignTaskRequest(int? AssignedToUserId);
