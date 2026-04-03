using MsProjects.Application.Models;

namespace MsProjects.Domain.Services
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectModel>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<ProjectModel?> GetAsync(int id, CancellationToken cancellationToken = default);
        Task<ProjectModel> AddAsync(AddProjectRequest request, int ownerId, CancellationToken cancellationToken = default, string? ownerName = null, string? ownerEmail = null);
        Task UpdateAsync(int id, UpdateProjectRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}