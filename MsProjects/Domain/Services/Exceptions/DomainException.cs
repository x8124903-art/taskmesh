using System;

namespace MsProjects.Domain.Services.Exceptions
{
    /// <summary>
    /// Base exception class for all domain-specific exceptions.
    /// </summary>
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message)
        {
        }

        protected DomainException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
