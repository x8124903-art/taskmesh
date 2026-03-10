namespace MsProjects.Application.Models;

public sealed record UpdateProjectRequest(string Name, string Description, int Status);
