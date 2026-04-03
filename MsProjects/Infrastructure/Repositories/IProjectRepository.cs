using MsProjects.Application.Models;

namespace MsProjects.Infrastructure.Repositories
{
    public interface IProjectRepository
    {
        Task<IEnumerable<ProjectModel>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<ProjectModel?> GetAsync(int id, CancellationToken cancellationToken = default);
        Task<ProjectModel> AddAsync(AddProjectRequest request, int ownerId, CancellationToken cancellationToken = default);
        Task UpdateAsync(int id, UpdateProjectRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}