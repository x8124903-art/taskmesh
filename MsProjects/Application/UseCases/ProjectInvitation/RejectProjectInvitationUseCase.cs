using MsProjects.Domain.Services;

namespace MsProjects.Application.UseCases.ProjectInvitation;

public sealed class RejectProjectInvitationUseCase : IRejectProjectInvitationUseCase
{
    private readonly IProjectInvitationService _invitationService;

    public RejectProjectInvitationUseCase(IProjectInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    public async Task ExecuteAsync(string token, CancellationToken cancellationToken = default)
    {
        await _invitationService.RejectInvitationAsync(token, cancellationToken);
    }
}
