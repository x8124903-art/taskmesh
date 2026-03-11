namespace MsProjects.Application.UseCases.ProjectInvitation;

public interface IRejectProjectInvitationUseCase
{
    Task ExecuteAsync(string token, CancellationToken cancellationToken = default);
}
