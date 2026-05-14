using System.Diagnostics.CodeAnalysis;

namespace MsTasks.Domain.Exceptions;

[ExcludeFromCodeCoverage]
public sealed class ConcurrencyException : DomainException
{
    public ConcurrencyException()
        : base("The task has been modified by another user. Please reload the task and try again.")
    {
    }
}
