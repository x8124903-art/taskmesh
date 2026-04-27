using MsTasks.Domain;

namespace MsTasks.Application.Models;

public sealed record ChangeStatusRequest(TaskStatus Status);
