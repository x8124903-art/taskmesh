namespace MsProjects.Domain.Services.Exceptions
{
    public sealed class InvitationNotFoundException : DomainException
    {
        public InvitationNotFoundException(string token)
            : base($"No se encontró ninguna invitación con el token '{token}'.")
        {
        }
        
        public InvitationNotFoundException(int invitationId)
            : base($"No se encontró ninguna invitación con el ID {invitationId}.")
        {
        }
    }
}
