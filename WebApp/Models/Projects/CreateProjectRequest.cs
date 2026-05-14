using System.Diagnostics.CodeAnalysis;

namespace WebApp.Models.Projects;

[ExcludeFromCodeCoverage]
public sealed record CreateProjectRequest(string Name, string? Description);
