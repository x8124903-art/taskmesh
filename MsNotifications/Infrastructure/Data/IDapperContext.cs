using System.Data;

namespace MsNotifications.Infrastructure.Data;

public interface IDapperContext : IDisposable
{
    IDbConnection Connection { get; }
}
