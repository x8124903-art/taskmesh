using MsProjects.Application.Models;
using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;

namespace MsProjects.Application.UseCases.ProjectInvitation;

public sealed class GetProjectPendingInvitationsUseCase : IGetProjectPendingInvitationsUseCase
{
    private readonly IProjectInvitationService _invitationService;
    private readonly IProjectAuthorizationService _authService;

    public GetProjectPendingInvitationsUseCase(IProjectInvitationService invitationService, IProjectAuthorizationService authService)
    {
        _invitationService = invitationService;
        _authService = authService;
    }

    public async Task<IEnumerable<ProjectInvitationModel>> ExecuteAsync(int projectId, int userId, CancellationToken cancellationToken = default)
    {
        if (!await _authService.HasProjectRoleAsync(userId, projectId, cancellationToken,
            ProjectRoles.Owner, ProjectRoles.Admin))
            throw new UnauthorizedAccessException("User does not have permission to view invitations for this project.");

        return await _invitationService.GetPendingInvitationsAsync(projectId, cancellationToken);
    }
}
