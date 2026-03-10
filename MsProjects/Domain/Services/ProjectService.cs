using MsProjects.Application.Models;
using MsProjects.Domain.Services.Authorization;
using MsProjects.Infrastructure.Repositories;

namespace MsProjects.Domain.Services
{
    public sealed class ProjectService : IProjectService
    {
        private readonly IProjectRepository _repository;
        private readonly IProjectMemberRepository _memberRepository;
        private readonly IProjectAuthorizationService _authService;

        public ProjectService(
            IProjectRepository repository,
            IProjectMemberRepository memberRepository,
            IProjectAuthorizationService authService)
        {
            _repository = repository;
            _memberRepository = memberRepository;
            _authService = authService;
        }

        public Task<IEnumerable<ProjectModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repository.GetAllAsync(cancellationToken);
        }

        public Task<ProjectModel?> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repository.GetAsync(id, cancellationToken);
        }

        public async Task<ProjectModel> AddAsync(AddProjectRequest request, int ownerId, CancellationToken cancellationToken = default, string? ownerName = null, string? ownerEmail = null)
        {
            var project = await _repository.AddAsync(request, ownerId, cancellationToken);
            
            await _memberRepository.AddMemberAsync(
                project.IdProject, 
                ownerId, 
                ProjectRoles.Owner, 
                cancellationToken,
                ownerName,
                ownerEmail);
            
            await _authService.InvalidateUserProjectsCacheAsync(ownerId, cancellationToken);
            
            return project;
        }

        public Task UpdateAsync(int id, UpdateProjectRequest request, CancellationToken cancellationToken = default)
        {
            return _repository.UpdateAsync(id, request, cancellationToken);
        }

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repository.DeleteAsync(id, cancellationToken);
        }
    }
}