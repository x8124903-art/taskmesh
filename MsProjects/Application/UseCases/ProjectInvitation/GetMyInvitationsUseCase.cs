using MsProjects.Application.Models;
using MsProjects.Domain.Services;

namespace MsProjects.Application.UseCases.ProjectInvitation;

public sealed class GetMyInvitationsUseCase : IGetMyInvitationsUseCase
{
    private readonly IProjectInvitationService _invitationService;

    public GetMyInvitationsUseCase(IProjectInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    public async Task<IEnumerable<ProjectInvitationModel>> ExecuteAsync(string email, string? status = null, CancellationToken cancellationToken = default)
    {
        var invitations = await _invitationService.GetInvitationsByEmailAsync(email, cancellationToken);

        if (!string.IsNullOrWhiteSpace(status))
            invitations = invitations.Where(i => string.Equals(i.Status, status, StringComparison.OrdinalIgnoreCase));

        return invitations;
    }
}
