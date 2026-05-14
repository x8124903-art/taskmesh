using System.Diagnostics.CodeAnalysis;

namespace MsNotifications.Domain.Services.Exceptions;

[ExcludeFromCodeCoverage]
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
