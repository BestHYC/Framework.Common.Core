using System.Data;
using Microsoft.Extensions.Logging;

namespace Framework.Common.Core.ORM
{
    public interface IDbContext
    {
        string ConnectionString { get; }

        ILogger Logger { get; }
        IDbConnection GetConnection();
    }
}
