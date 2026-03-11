using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;

namespace MsProjects.Application.UseCases.ProjectInvitation;

public sealed class CancelProjectInvitationUseCase : ICancelProjectInvitationUseCase
{
    private readonly IProjectInvitationService _invitationService;
    private readonly IProjectAuthorizationService _authService;

    public CancelProjectInvitationUseCase(IProjectInvitationService invitationService, IProjectAuthorizationService authService)
    {
        _invitationService = invitationService;
        _authService = authService;
    }

    public async Task ExecuteAsync(string token, int userId, CancellationToken cancellationToken = default)
    {
        var invitation = await _invitationService.GetInvitationByTokenAsync(token, cancellationToken);
        if (invitation == null)
            throw new KeyNotFoundException("Invitation not found.");

        if (!await _authService.HasProjectRoleAsync(userId, invitation.ProjectId, cancellationToken,
            ProjectRoles.Owner, ProjectRoles.Admin))
            throw new UnauthorizedAccessException("User does not have permission to cancel this invitation.");

        await _invitationService.DeleteInvitationAsync(token, cancellationToken);
    }
}
