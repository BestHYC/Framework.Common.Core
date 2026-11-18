using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ClickHouse.Client.ADO;
using ClickHouse.Client.Copy;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Framework.Common.Core.ORM
{
    /// <summary>
    /// CK执行的方式
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CKExecuteBatch<T> : IExecuteBatch<T> where T : class, new()
    {
        private readonly CKDBContext _context;
        public CKExecuteBatch(CKDBContext dbContext)
        {
            _context = dbContext;
        }
        public void BulkInsert(List<T> sourceList, int batchSize = 100000)
        {
            var task = BulkInsertAsync(sourceList);
            task.Wait();
        }
        public async Task BulkInsertAsync(List<T> sourceList, int batchSize = 100000)
        {
            try
            {
                if (sourceList == null || sourceList.Count == 0) return;
                ClickHouseDataRowCollection localTable = DataColumnsToRowCollection(sourceList); 
                if (localTable == null)
                {
                    throw new ArgumentException("sourceList is null or empty");
                }
                ClickHouseConnection connection = GetConnection();
                if (connection == null)
                {
                    WriteSql(BuildBulkSql(localTable, false));
                    return;
                }
                using (connection)
                using (ClickHouseBulkCopy bulkCopyInterface = new ClickHouseBulkCopy(connection)
                {
                    DestinationTableName = EntityTableWrapper.GetTableName(typeof(T)),
                    MaxDegreeOfParallelism = _context.MaxDegreeBulkInsertionOfParallelism,
                    BatchSize = batchSize,
                    ColumnNames = localTable.Columns.ToArray()
                })
                {
                    await bulkCopyInterface.WriteToServerAsync(rows: localTable.Rows, token: CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Exception e = ex;
                _context.Logger.LogError(e, "ClickHouse 批量插入失败 BulkInsertClickHouseAsync");
                throw;
            }
        }
        public void BulkDelete(List<T> sourceList, int batchSize = 100000)
        {
            try
            {
                var task = BulkDeleteAsync(sourceList);
                task.Wait();
            }
            catch (Exception ex)
            {
                Exception e = ex;
                _context.Logger.LogError(e, "ClickHouse 批量插入失败 BulkInsertClickHouseAsync");
                throw ex;
            }
        }
        public async Task BulkDeleteAsync(List<T> sourceList, int batchSize = 100000)
        {
            try
            {
                if (sourceList == null || sourceList.Count == 0) return;
                ClickHouseDataRowCollection localTable = DataColumnsToRowCollection(sourceList, true);
                if (localTable == null)
                {
                    throw new ArgumentException("sourceList is null or empty");
                }
                ClickHouseConnection connection = GetConnection();
                if (connection == null)
                {
                    WriteSql(BuildBulkSql(localTable, true));
                    return;
                }
                using (connection)
                using (ClickHouseBulkCopy bulkCopyInterface = new ClickHouseBulkCopy(connection)
                {
                    DestinationTableName = EntityTableWrapper.GetTableName(typeof(T)),
                    MaxDegreeOfParallelism = _context.MaxDegreeBulkInsertionOfParallelism,
                    BatchSize = batchSize,
                    ColumnNames = localTable.Columns.ToArray()
                })
                {
                    await bulkCopyInterface.WriteToServerAsync(rows: localTable.Rows, token: CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Exception e = ex;
                _context.Logger.LogError(e, "ClickHouse 批量插入失败 BulkInsertClickHouseAsync");
                throw ex;
            }
        }
        public void BulkUpdate(List<T> sourceList, int batchSize = 100000)
        {
            BulkInsert(sourceList, batchSize);
        }
        /// <summary>
        /// CK只有Insert，所以直接操作，注意，操作前得 将所有数据填满，否则会覆盖
        /// </summary>
        /// <param name="sourceList"></param>
        /// <param name="update"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        public async Task BulkUpdateAsync(List<T> sourceList, int batchSize = 100000)
        {
            await BulkInsertAsync(sourceList, batchSize);
        }

        /// <summary>
        /// 由于CK无transaction，所以直接忽略
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public int Execute(string sql, DynamicParameters parameters = null, bool transaction = false)
        {
            ClickHouseConnection connection = GetConnection();
            if (connection == null)
            {
                WriteSql(sql);
                return 0;
            }
            using (connection)
            {
                return connection.Execute(sql, parameters, commandTimeout: 3600);
            }
        }

        public int Execute(IBatch batch, bool transaction = false)
        {
            ClickHouseConnection connection = GetConnection();
            if (connection == null)
            {
                WriteSql(batch?.Sql);
                return 0;
            }
            using (connection)
            {
                return connection.Execute(batch.Sql, batch.DynamicParameters, commandTimeout: 3600);
            }
        }
        public int Execute(IDbConnection connection, IBatch batch, IDbTransaction transaction)
        {
            return connection.Execute(batch.Sql, batch.DynamicParameters, transaction, commandTimeout: 3600);
        }

        public async Task<int> ExecuteAsync(string sql, DynamicParameters parameters = null, bool transaction = false)
        {
            ClickHouseConnection connection = GetConnection();
            if (connection == null)
            {
                WriteSql(sql);
                return 0;
            }
            using (connection)
            {
                return await connection.ExecuteAsync(sql, parameters, commandTimeout: 3600);
            }
        }

        public async Task<int> ExecuteAsync(IDbConnection connection, IBatch batch, IDbTransaction transaction)
        {
            return await connection.ExecuteAsync(batch.Sql, batch.DynamicParameters, transaction, commandTimeout: 3600);
        }

        public async Task<int> ExecuteAsync(IBatch batch, bool transaction = false)
        {
            ClickHouseConnection connection = GetConnection();
            if (connection == null)
            {
                WriteSql(batch?.Sql);
                return 0;
            }
            using (connection)
            {
                return await connection.ExecuteAsync(batch.Sql, batch.DynamicParameters, commandTimeout: 3600);
            }
        }
        /// <summary>
        /// ADO.Net 中按行读取，CK 可能没有，
        /// </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IEnumerable<T> ExecuteReader(IBatch batch)
        {
            ClickHouseConnection connection = GetConnection();
            if (connection == null)
            {
                WriteSql(batch?.Sql);
                return new List<T>();
            }
            List<T> entitybuffer = new List<T>();
            using (connection)
            {
                var reader = connection.ExecuteReader(batch.Sql, batch.DynamicParameters, commandTimeout: 3600);
                var entityParser = reader.GetRowParser<T>();
                while (reader.Read())
                {
                    var entity = entityParser.Invoke(reader);
                    if (entity == null) break;
                    entitybuffer.Add(entity);
                }
            }
            return entitybuffer;
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync(IBatch batch)
        {
            ClickHouseConnection connection = GetConnection();
            if (connection == null)
            {
                WriteSql(batch?.Sql);
                return new List<T>();
            }
            List<T> entitybuffer = new List<T>();
            using (connection)
            {
                var reader = await connection.ExecuteReaderAsync(batch.Sql, batch.DynamicParameters, commandTimeout: 3600);
                var entityParser = reader.GetRowParser<T>();
                while (reader.Read())
                {
                    var entity = entityParser.Invoke(reader);
                    if (entity == null) break;
                    entitybuffer.Add(entity);
                }
            }
            return entitybuffer;
        }

        public ClickHouseConnection GetConnection()
        {
            var conn = _context.GetConnection();
            if (conn == null) return null;
            return conn as ClickHouseConnection;
        }

        private static void WriteSql(string? sql)
        {
            Console.WriteLine(sql ?? string.Empty);
        }

        private static string BuildBulkSql(ClickHouseDataRowCollection table, bool isDelete)
        {
            string verb = isDelete ? "DELETE" : "INSERT";
            string columns = table?.Columns == null ? string.Empty : string.Join(", ", table.Columns);
            return $"{verb} {EntityTableWrapper.GetTableName(typeof(T))} ({columns})";
        }

        public IEnumerable<T> Query(IBatch batch)
        {
            ClickHouseConnection connection = GetConnection();
            if (connection == null)
            {
                WriteSql(batch?.Sql);
                return Enumerable.Empty<T>();
            }
            using (connection)
            {
                return connection.Query<T>(batch.Sql, batch.DynamicParameters, commandTimeout: 3600);
            }
        }

        public IEnumerable<K> Query<K>(IBatch batch)
        {
            ClickHouseConnection connection = GetConnection();
            if (connection == null)
            {
                WriteSql(batch?.Sql);
                return Enumerable.Empty<K>();
            }
            using (connection)
            {
                return connection.Query<K>(batch.Sql, batch.DynamicParameters, commandTimeout: 3600);
            }
        }

        public IEnumerable<K> Query<K>(string sql, DynamicParameters parameters = null)
        {
            ClickHouseConnection connection = GetConnection();
            if (connection == null)
            {
                WriteSql(sql);
                return Enumerable.Empty<K>();
            }
            using (connection)
            {
                return connection.Query<K>(sql, parameters, commandTimeout: 3600);
            }
        }
        public IEnumerable<T> Query(string sql, DynamicParameters parameters = null)
        {
            ClickHouseConnection connection = GetConnection();
            if (connection == null)
            {
                WriteSql(sql);
                return Enumerable.Empty<T>();
            }
            using (connection)
            {
                return connection.Query<T>(sql, parameters, commandTimeout: 3600);
            }
        }
        public async Task<IEnumerable<T>> QueryAsync(IBatch batch)
        {
            ClickHouseConnection connection = GetConnection();
            if (connection == null)
            {
                WriteSql(batch?.Sql);
                return Enumerable.Empty<T>();
            }
            using (connection)
            {
                return await connection.QueryAsync<T>(batch.Sql, batch.DynamicParameters, commandTimeout: 3600);
            }
        }

        public async Task<IEnumerable<K>> QueryAsync<K>(IBatch batch)
        {
            ClickHouseConnection connection = GetConnection();
            if (connection == null)
            {
                WriteSql(batch?.Sql);
                return Enumerable.Empty<K>();
            }
            using (connection)
            {
                return await connection.QueryAsync<K>(batch.Sql, batch.DynamicParameters, commandTimeout: 3600);
            }
        }

        public async Task<IEnumerable<K>> QueryAsync<K>(string sql, DynamicParameters parameters = null)
        {
            ClickHouseConnection connection = GetConnection();
            if (connection == null)
            {
                WriteSql(sql);
                return Enumerable.Empty<K>();
            }
            using (connection)
            {
                return await connection.QueryAsync<K>(sql, parameters, commandTimeout: 3600);
            }
        }

        public async Task<IEnumerable<T>> QueryAsync(string sql, DynamicParameters parameters = null)
        {
            ClickHouseConnection connection = GetConnection();
            if (connection == null)
            {
                WriteSql(sql);
                return Enumerable.Empty<T>();
            }
            using (connection)
            {
                return await connection.QueryAsync<T>(sql, parameters, commandTimeout: 3600);
            }
        }
        public List<T> QueryList(IBatch batch)
        {
            var result = Query(batch);
            if (result == null) return null;
            return result.ToList();
        }

        public List<K> QueryList<K>(IBatch batch)
        {
            var result = Query<K>(batch);
            if (result == null) return null;
            return result.ToList();
        }

        public async Task<List<T>> QueryListAsync(IBatch batch)
        {
            var result = await QueryAsync(batch);
            if (result == null) return null;
            return result.ToList();
        }

        public async Task<List<K>> QueryListAsync<K>(IBatch batch)
        {
            var result = await QueryAsync<K>(batch);
            if (result == null) return null;
            return result.ToList();
        }
        private ClickHouseDataRowCollection DataColumnsToRowCollection(IList<T> list, bool isDelete = false)
        {
            if (list == null)
            {
                return null;
            }
            ClickHouseDataRowCollection result = new ClickHouseDataRowCollection
            {
                Rows = new List<object[]>(),
                Columns = new List<string>()
            };
            Type typeFromHandle = typeof(T);

            var listPK = EntityTableWrapper.GetPKFields(typeof(T)).Select(i => i.DBFieldName);
            List<String> none = new List<String>() { "version", "sign", "_insert_time" };
            List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
            List<String> rowNames = new List<string>();
            List<String> columns = new List<String>();
            if (columns == null || !columns.Any())
            {
                PropertyInfo[] array = Array.FindAll(typeFromHandle.GetProperties(), (PropertyInfo p) => p.CanRead);
                Array.ForEach(array, delegate (PropertyInfo prop)
                {
                    String column = EntityTableWrapper.GetDBFieldName(typeof(T), prop.Name);
                    if (none.Contains(column)) return;
                    propertyInfos.Add(prop);
                    rowNames.Add(column);
                    result.Columns.Add(column);
                });
            }
            result.Columns.Add("version");
            result.Columns.Add("sign");
            result.Columns.Add("_insert_time");
            DateTimeOffset utcNow = DateTimeOffset.UtcNow;
            long num = utcNow.ToUnixTimeMilliseconds();
            int deleteFlag = isDelete ? 0 : 1;
            DateTimeOffset dateTimeOffset = utcNow;
            foreach (T item in list)
            {
                object[] array2 = new object[result.Columns.Count];

                for (int i = 0; i < propertyInfos.Count; i++)
                {
                    PropertyInfo propertyInfo = propertyInfos[i];
                    var propertyValue = propertyInfo.GetValue(item) ?? DBNull.Value;
                    if (listPK.Contains(propertyInfo.Name) && (
                        propertyValue == DBNull.Value ||
                        propertyValue.GetHashCode() == DefaultTypeValue(propertyInfo.PropertyType).GetHashCode()))
                    {
                        throw new ArgumentException("Primary key cannot be null");
                    }
                    array2[i] = propertyValue;
                }
                array2[result.Columns.Count - 3] = num;
                array2[result.Columns.Count - 2] = deleteFlag;
                array2[result.Columns.Count - 1] = dateTimeOffset;
                result.Rows.Add(array2);
            }
            return result;
        }
        private Object DefaultTypeValue(Type targetType)
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : DBNull.Value;
        }

        public List<T> QueryList(string sql, DynamicParameters parameters = null)
        {
            var result = Query(sql, parameters);
            if (result == null) return null;
            return result.ToList();
        }

        public async Task<List<T>> QueryListAsync(string sql, DynamicParameters parameters = null)
        {
            var result = await QueryAsync(sql, parameters);
            if (result == null) return null;
            return result.ToList();
        }

        public List<K> QueryList<K>(string sql, DynamicParameters parameters = null)
        {
            var result = Query<K>(sql, parameters);
            if (result == null) return null;
            return result.ToList();
        }

        public async Task<List<K>> QueryListAsync<K>(string sql, DynamicParameters parameters = null)
        {
            var result = await QueryAsync<K>(sql, parameters);
            if (result == null) return null;
            return result.ToList();
        }

        public void BulkInsert(IBatch batch, int batchSize = 100000)
        {
            if (batch == null) throw new ArgumentNullException();
            var result = batch.Value as List<T>;
            BulkInsert(result, batchSize);
        }

        public async Task BulkInsertAsync(IBatch batch, int batchSize = 100000)
        {
            if (batch == null) throw new ArgumentNullException();
            var result = batch.Value as List<T>;
            await BulkInsertAsync(result, batchSize);
        }

        public void BulkDelete(IBatch batch, int batchSize = 100000)
        {
            if (batch == null) throw new ArgumentNullException();
            var result = batch.Value as List<T>;
            BulkDelete(result, batchSize);
        }

        public async Task BulkDeleteAsync(IBatch batch, int batchSize = 100000)
        {
            if (batch == null) throw new ArgumentNullException();
            var result = batch.Value as List<T>;
            await BulkDeleteAsync(result, batchSize);
        }

        public void BulkUpdate(IBatch batch, int batchSize = 100000)
        {
            if (batch == null) throw new ArgumentNullException();
            var result = batch.Value as List<T>;
            BulkUpdate(result, batchSize);
        }

        public async Task BulkUpdateAsync(IBatch batch, int batchSize = 100000)
        {
            if (batch == null) throw new ArgumentNullException();
            var result = batch.Value as List<T>;
            await BulkUpdateAsync(result, batchSize);
        }

        public IEnumerable<T> ExecuteReader(IBatch batch, int batchSize = 100000)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> ExecuteReaderAsync(IBatch batch, int batchSize = 100000)
        {
            throw new NotImplementedException();
        }
    }
    public class ClickHouseDataRowCollection
    {
        public List<object[]> Rows { get; set; }

        public List<string> Columns { get; set; }
    }
}
