namespace MsProjects.Infrastructure.Persistence;

internal sealed record ProjectEntity(
    int IdProject,
    string Name,
    string Description,
    int Status,
    string StatusName,
    int CreatedBy,
    string CreatedByName,
    bool IsDeleted,
    DateTime CreatedAt);
