namespace MsProjects.Domain.Services.Exceptions
{
    public sealed class DuplicateInvitationException : DomainException
    {
        public DuplicateInvitationException(int projectId, string email)
            : base($"Ya existe una invitación pendiente para el email '{email}' en el proyecto con ID {projectId}.")
        {
        }
    }
}
