using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClickHouse.Client.ADO;
using Microsoft.Extensions.Logging;

namespace Framework.Common.Core.ORM
{
    public class CKDBContext: IDbContext
    {

        private readonly string _connUrl;
        public Boolean ViewType { get; set; }

        private readonly ILogger _logger;

        private readonly string _logAppName;

        public readonly int MaxDegreeBulkInsertionOfParallelism;

        public CKDBContext(int maxDegreeBulkInsertionOfParallelism,
            ILogger<CKDBContext> logger,
            string connectionString,
            string logAppName)
        {
            _connUrl = connectionString;
            MaxDegreeBulkInsertionOfParallelism = maxDegreeBulkInsertionOfParallelism;
            _logger = logger;
            _logAppName = logAppName;
        }

        public string ConnectionString => _connUrl;

        public ILogger Logger => _logger;

        public IDbConnection GetConnection()
        {
            if (ViewType) return null;
            return new ClickHouseConnection(ConnectionString);
        }
    }
}
