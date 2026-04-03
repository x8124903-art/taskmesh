using MsProjects.Application.Models;

namespace MsProjects.Application.UseCases.ProjectInvitation;

public interface ICreateProjectInvitationUseCase
{
    Task<ProjectInvitationModel> ExecuteAsync(InviteMemberRequest request, int userId, string? userName = null, CancellationToken cancellationToken = default);
}
