using System;

namespace MsProjects.Domain.Services.Exceptions
{
    public sealed class InvitationExpiredException : DomainException
    {
        public InvitationExpiredException(DateTime expirationDate)
            : base($"Esta invitación expiró el {expirationDate:dd/MM/yyyy HH:mm}. Por favor, solicita una nueva invitación.")
        {
        }
    }
}
