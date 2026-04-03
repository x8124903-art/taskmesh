using MsProjects.Application.Models;

namespace MsProjects.Application.UseCases.ProjectInvitation;

public interface IGetMyInvitationsUseCase
{
    Task<IEnumerable<ProjectInvitationModel>> ExecuteAsync(string email, string? status = null, CancellationToken cancellationToken = default);
}
