using System.Diagnostics.CodeAnalysis;
using MsProjects.Application.Models;
using MsProjects.Domain.Services;
using MsProjects.Infrastructure.Persistence;

namespace MsProjects.Infrastructure.Repositories
{
    [ExcludeFromCodeCoverage]
    public sealed class ProjectSqlRepository : IProjectRepository
    {
        private readonly MsProjects.Infrastructure.Data.IDapperContext _context;

        public ProjectSqlRepository(MsProjects.Infrastructure.Data.IDapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProjectModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            using var connection = _context.CreateConnection();
            var entities = await _context.QueryAsync<ProjectEntity>(
                connection,
                QueriesMySql.GetAll,
                null,
                cancellationToken);
            return entities.Select(ToModel);
        }

        public async Task<ProjectModel?> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            using var connection = _context.CreateConnection();
            var entity = await _context.QueryFirstOrDefaultAsync<ProjectEntity>(
                connection,
                QueriesMySql.GetById,
                new { IdProject = id },
                cancellationToken);
            return entity is null ? null : ToModel(entity);
        }

        public async Task<ProjectModel> AddAsync(AddProjectRequest request, int ownerId, CancellationToken cancellationToken = default)
        {
            using var connection = _context.CreateConnection();
            var now = DateTime.UtcNow;
            var id = await _context.ExecuteScalarAsync<int>(
                connection,
                QueriesMySql.Insert,
                new { request.Name, request.Description, Status = ProjectStatuses.Active, CreatedBy = ownerId, CreatedAt = now },
                cancellationToken);
            
            return (await GetAsync(id, cancellationToken))!;
        }

        public async Task UpdateAsync(int id, UpdateProjectRequest request, CancellationToken cancellationToken = default)
        {
            using var connection = _context.CreateConnection();
            await _context.ExecuteAsync(
                connection,
                QueriesMySql.Update,
                new { IdProject = id, request.Name, request.Description, request.Status },
                cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            using var connection = _context.CreateConnection();
            await _context.ExecuteAsync(
                connection,
                QueriesMySql.Delete,
                new { IdProject = id },
                cancellationToken);
        }

        private static ProjectModel ToModel(ProjectEntity e) =>
            new(e.IdProject, e.Name, e.Description, e.Status, e.StatusName, e.CreatedBy, e.CreatedByName, e.IsDeleted, e.CreatedAt);
    }
}
