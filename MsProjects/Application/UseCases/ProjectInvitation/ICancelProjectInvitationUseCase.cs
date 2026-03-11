namespace MsProjects.Application.UseCases.ProjectInvitation;

public interface ICancelProjectInvitationUseCase
{
    Task ExecuteAsync(string token, int userId, CancellationToken cancellationToken = default);
}
