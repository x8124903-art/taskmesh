namespace MsProjects.Application.UseCases.ProjectMember;

public interface IChangeProjectMemberRoleUseCase
{
    Task ExecuteAsync(int projectId, int targetUserId, string role, int currentUserId, CancellationToken cancellationToken = default);
}
