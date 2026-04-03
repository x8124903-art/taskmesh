using MsProjects.Application.Models;
using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;

namespace MsProjects.Application.UseCases.ProjectInvitation;

public sealed class CreateProjectInvitationUseCase : ICreateProjectInvitationUseCase
{
    private readonly IProjectInvitationService _invitationService;
    private readonly IProjectAuthorizationService _authService;

    public CreateProjectInvitationUseCase(IProjectInvitationService invitationService, IProjectAuthorizationService authService)
    {
        _invitationService = invitationService;
        _authService = authService;
    }

    public async Task<ProjectInvitationModel> ExecuteAsync(InviteMemberRequest request, int userId, string? userName = null, CancellationToken cancellationToken = default)
    {
        if (!await _authService.HasProjectRoleAsync(userId, request.ProjectId, cancellationToken,
            ProjectRoles.Owner, ProjectRoles.Admin))
            throw new UnauthorizedAccessException("User does not have permission to invite members to this project.");

        return await _invitationService.CreateInvitationAsync(request, userId, cancellationToken, userName);
    }
}
