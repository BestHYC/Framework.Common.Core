using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Framework.Common.Core.ORM
{
    public interface IExecuteBatch<T> where T : class, new()
    {
        Int32 Execute(String sql, DynamicParameters parameters = null, Boolean transaction = false);
        Task<Int32> ExecuteAsync(String sql, DynamicParameters parameters = null, Boolean transaction = false);
        Int32 Execute(IBatch batch, Boolean transaction = false);

        /// <summary>
        /// 此方法是为了使用外部得connection,且不会主动释放
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="batch"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Int32 Execute(IDbConnection connection, IBatch batch, IDbTransaction transaction);
        Task<Int32> ExecuteAsync(IDbConnection connection, IBatch batch, IDbTransaction transaction);
        Task<Int32> ExecuteAsync(IBatch batch, Boolean transaction = false);
        IEnumerable<T> Query(IBatch batch);
        Task<IEnumerable<T>> QueryAsync(IBatch batch);
        IEnumerable<K> Query<K>(IBatch batch);
        Task<IEnumerable<K>> QueryAsync<K>(IBatch batch);
        IEnumerable<T> Query(String sql, DynamicParameters parameters = null);
        Task<IEnumerable<T>> QueryAsync(String sql, DynamicParameters parameters = null);
        IEnumerable<K> Query<K>(String sql, DynamicParameters parameters = null);
        Task<IEnumerable<K>> QueryAsync<K>(String sql, DynamicParameters parameters = null);
        List<T> QueryList(IBatch batch);
        Task<List<T>> QueryListAsync(IBatch batch);
        List<T> QueryList(String sql, DynamicParameters parameters = null);
        Task<List<T>> QueryListAsync(String sql, DynamicParameters parameters = null);
        List<K> QueryList<K>(IBatch batch);
        Task<List<K>> QueryListAsync<K>(IBatch batch);
        List<K> QueryList<K>(String sql, DynamicParameters parameters = null);
        Task<List<K>> QueryListAsync<K>(String sql, DynamicParameters parameters = null);
        void BulkInsert(List<T> sourceList, int batchSize = 100000);
        Task BulkInsertAsync(List<T> sourceList, int batchSize = 100000);
        void BulkDelete(List<T> sourceList, int batchSize = 100000);
        Task BulkDeleteAsync(List<T> sourceList, int batchSize = 100000);
        void BulkUpdate(List<T> sourceList, int batchSize = 100000);
        Task BulkUpdateAsync(List<T> sourceList, int batchSize = 100000);
        void BulkInsert(IBatch batch, int batchSize = 100000);
        Task BulkInsertAsync(IBatch batch, int batchSize = 100000);
        void BulkDelete(IBatch batch, int batchSize = 100000);
        Task BulkDeleteAsync(IBatch batch, int batchSize = 100000);
        void BulkUpdate(IBatch batch, int batchSize = 100000);
        Task BulkUpdateAsync(IBatch batch, int batchSize = 100000);
        IEnumerable<T> ExecuteReader(IBatch batch, int batchSize = 100000);
        Task<IEnumerable<T>> ExecuteReaderAsync(IBatch batch, int batchSize = 100000);
    }
}
