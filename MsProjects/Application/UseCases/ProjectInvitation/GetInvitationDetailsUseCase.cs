using MsProjects.Application.Models;
using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;

namespace MsProjects.Application.UseCases.ProjectInvitation;

public sealed class GetInvitationDetailsUseCase : IGetInvitationDetailsUseCase
{
    private readonly IProjectInvitationService _invitationService;
    private readonly IProjectAuthorizationService _authService;

    public GetInvitationDetailsUseCase(IProjectInvitationService invitationService, IProjectAuthorizationService authService)
    {
        _invitationService = invitationService;
        _authService = authService;
    }

    public async Task<ProjectInvitationModel> ExecuteAsync(int invitationId, int userId, CancellationToken cancellationToken = default)
    {
        var invitation = await _invitationService.GetInvitationByIdAsync(invitationId, cancellationToken);
        if (invitation == null)
            throw new KeyNotFoundException($"Invitation {invitationId} not found.");

        if (!await _authService.HasProjectRoleAsync(userId, invitation.ProjectId, cancellationToken,
            ProjectRoles.Owner, ProjectRoles.Admin))
            throw new UnauthorizedAccessException("User does not have permission to view this invitation.");

        return invitation;
    }
}
