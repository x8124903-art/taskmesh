namespace MsProjects.Application.UseCases.ProjectInvitation;

public interface IAcceptProjectInvitationUseCase
{
    Task ExecuteAsync(string token, int userId, string? userName = null, string? userEmail = null, CancellationToken cancellationToken = default);
}
