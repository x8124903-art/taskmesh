using MsTasks.Domain;

using System.Diagnostics.CodeAnalysis;

namespace MsTasks.Application.Models;

[ExcludeFromCodeCoverage]
public sealed record ChangeStatusRequest(TaskStatus Status);
