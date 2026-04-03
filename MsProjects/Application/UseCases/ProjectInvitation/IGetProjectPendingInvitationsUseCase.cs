using MsProjects.Application.Models;

namespace MsProjects.Application.UseCases.ProjectInvitation;

public interface IGetProjectPendingInvitationsUseCase
{
    Task<IEnumerable<ProjectInvitationModel>> ExecuteAsync(int projectId, int userId, CancellationToken cancellationToken = default);
}
