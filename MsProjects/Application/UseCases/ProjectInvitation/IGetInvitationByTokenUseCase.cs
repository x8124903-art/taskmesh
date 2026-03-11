using MsProjects.Application.Models;

namespace MsProjects.Application.UseCases.ProjectInvitation;

public interface IGetInvitationByTokenUseCase
{
    Task<ProjectInvitationModel> ExecuteAsync(string token, CancellationToken cancellationToken = default);
}
