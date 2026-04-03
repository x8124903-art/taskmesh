using MsProjects.Domain.Services;

namespace MsProjects.Application.UseCases.ProjectInvitation;

public sealed class AcceptProjectInvitationUseCase : IAcceptProjectInvitationUseCase
{
    private readonly IProjectInvitationService _invitationService;

    public AcceptProjectInvitationUseCase(IProjectInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    public async Task ExecuteAsync(string token, int userId, string? userName = null, string? userEmail = null, CancellationToken cancellationToken = default)
    {
        await _invitationService.AcceptInvitationAsync(token, userId, cancellationToken, userName, userEmail);
    }
}
