namespace MsProjects.Application.UseCases.ProjectMember;

public interface IRemoveProjectMemberUseCase
{
    Task ExecuteAsync(int projectId, int targetUserId, int currentUserId, CancellationToken cancellationToken = default);
}
