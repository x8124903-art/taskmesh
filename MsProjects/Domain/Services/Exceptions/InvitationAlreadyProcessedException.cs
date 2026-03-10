namespace MsProjects.Domain.Services.Exceptions
{
    public sealed class InvitationAlreadyProcessedException : DomainException
    {
        public InvitationAlreadyProcessedException(string status)
            : base($"Esta invitación ya ha sido procesada con el estado '{status}'.")
        {
        }
    }
}
