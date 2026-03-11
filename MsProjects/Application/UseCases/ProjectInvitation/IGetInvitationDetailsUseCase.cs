using MsProjects.Application.Models;

namespace MsProjects.Application.UseCases.ProjectInvitation;

public interface IGetInvitationDetailsUseCase
{
    Task<ProjectInvitationModel> ExecuteAsync(int invitationId, int userId, CancellationToken cancellationToken = default);
}
