using MsProjects.Application.Models;
using MsProjects.Domain.Services;

namespace MsProjects.Application.UseCases.ProjectInvitation;

public sealed class GetInvitationByTokenUseCase : IGetInvitationByTokenUseCase
{
    private readonly IProjectInvitationService _invitationService;

    public GetInvitationByTokenUseCase(IProjectInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    public async Task<ProjectInvitationModel> ExecuteAsync(string token, CancellationToken cancellationToken = default)
    {
        var invitation = await _invitationService.GetInvitationByTokenAsync(token, cancellationToken);
        if (invitation == null)
            throw new KeyNotFoundException("Invitation not found.");

        return invitation;
    }
}
