using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Framework.Common.Core.ORM
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class, new()
    {
        private readonly IExecuteBatch<T> _executeBatch;
        private protected SQLHelper m_sqlHelper = new SQLHelper();

        public BaseRepository(IExecuteBatch<T> executeBatch)
        {
            _executeBatch = executeBatch;
        }
        /// <summary>
        /// 执行查询操作
        /// </summary>
        /// <returns></returns>
        protected abstract IBatch EndQuery();
        /// <summary>
        /// 执行增删改操作
        /// </summary>
        /// <returns></returns>
        protected abstract IBatch EndExecute();
        public IBaseRepository<T> Where(Expression<Func<T, bool>> where)
        {
            m_sqlHelper.AddWhere(where);
            return this;
        }
        public IBaseRepository<T> OrderBy(Expression<Func<T, object>> property)
        {
            m_sqlHelper.AddOrder(property, DBFiledOperatorEnum.OrderBy);
            return this;
        }
        public IBaseRepository<T> OrderByDesc(Expression<Func<T, object>> property)
        {
            m_sqlHelper.AddOrder(property, DBFiledOperatorEnum.OrderByDesc);
            return this;
        }
        public IBaseRepository<T> Select(Expression<Func<T, object>> select)
        {
            m_sqlHelper.AddQuery(select);
            return this;
        }
        public IBaseRepository<T> Top(int num)
        {
            m_sqlHelper.AddTop(num);
            return this;
        }

        public IEnumerable<T> Query()
        {
            return QueryList();
        }
        public Task<IEnumerable<T>> QueryAsync()
        {
            return _executeBatch.QueryAsync(EndQuery());
        }
        public T QueryFirst()
        {
            m_sqlHelper.AddTop(1);
            return _executeBatch.Query(EndQuery()).FirstOrDefault();
        }
        public async Task<T> QueryFirstAsync()
        {
            m_sqlHelper.AddTop(1);
            return (await _executeBatch.QueryAsync(EndQuery())).FirstOrDefault();
        }
        public List<T> QueryList()
        {
            return _executeBatch.QueryList(EndQuery());
        }
        public async Task<List<T>> QueryListAsync()
        {
            return await _executeBatch.QueryListAsync(EndQuery());
        }
        public IEnumerable<T> Query(string sql, DynamicParameters parameters = null)
        {
            return _executeBatch.Query(sql, parameters);
        }

        public async Task<IEnumerable<T>> QueryAsync(string sql, DynamicParameters parameters = null)
        {
            return await _executeBatch.QueryAsync(sql, parameters);
        }

        public IEnumerable<K> Query<K>(string sql, DynamicParameters parameters = null)
        {
            return _executeBatch.Query<K>(sql, parameters);
        }

        public async Task<IEnumerable<K>> QueryAsync<K>(string sql, DynamicParameters parameters = null)
        {
            return await _executeBatch.QueryAsync<K>(sql, parameters);
        }
        public List<T> QueryList(string sql, DynamicParameters parameters = null)
        {
            return _executeBatch.QueryList<T>(sql, parameters);
        }
        public async Task<List<T>> QueryListAsync(string sql, DynamicParameters parameters = null)
        {
            return await _executeBatch.QueryListAsync(sql, parameters);
        }

        public List<K> QueryList<K>(string sql, DynamicParameters parameters = null)
        {
            return _executeBatch.QueryList<K>(sql, parameters);
        }

        public List<T> QueryByPage(string sql, PageInfo pageInfo, object parameters)
        {
            AddParameters(parameters);
            return _executeBatch.QueryList(BuildPageSql(sql, pageInfo), m_sqlHelper.Parameters) ?? new List<T>();
        }
        public (List<T> list, int totalCount) QueryByPageWithTotal(string sql, PageInfo pageInfo, object parameters)
        {
            AddParameters(parameters);
            List<T> list = _executeBatch.QueryList(BuildPageSql(sql, pageInfo), m_sqlHelper.Parameters) ?? new List<T>();
            int totalCount = _executeBatch.Query<int>(BuildCountSql(sql), m_sqlHelper.Parameters).FirstOrDefault();
            return (list, totalCount);
        }
        public async Task<List<T>> QueryByPageAsync(string sql, PageInfo pageInfo, object parameters)
        {
            AddParameters(parameters);
            return await _executeBatch.QueryListAsync(BuildPageSql(sql, pageInfo), m_sqlHelper.Parameters) ?? new List<T>();
        }
        public async Task<(List<T> list, int totalCount)> QueryByPageWithTotalAsync(string sql, PageInfo pageInfo, object parameters)
        {
            AddParameters(parameters);
            List<T> list = await _executeBatch.QueryListAsync(BuildPageSql(sql, pageInfo), m_sqlHelper.Parameters) ?? new List<T>();
            int totalCount = (await _executeBatch.QueryAsync<int>(BuildCountSql(sql), m_sqlHelper.Parameters)).FirstOrDefault();
            return (list, totalCount);
        }

        public IEnumerable<T> QueryByPK(object t)
        {
            AddPrimaryKey(t);
            return _executeBatch.Query(EndQuery());
        }
        public async Task<IEnumerable<T>> QueryByPKAsync(object t)
        {
            AddPrimaryKey(t);
            return await _executeBatch.QueryAsync(EndQuery());
        }

        public int Update()
        {
            m_sqlHelper.AddTable(typeof(T), DBTableOperatorEnum.Update);
            EnsureUpdateColumns();
            return _executeBatch.Execute(EndExecute());
        }
        public async Task<int> UpdateAsync()
        {
            m_sqlHelper.AddTable(typeof(T), DBTableOperatorEnum.Update);
            EnsureUpdateColumns();
            return await _executeBatch.ExecuteAsync(EndExecute());
        }
        public int Update(T t)
        {
            m_sqlHelper.AddTable(typeof(T), DBTableOperatorEnum.Update);
            AddPrimaryKey(t);
            EnsureUpdateColumns();
            return _executeBatch.Execute(EndExecute());
        }
        public async Task<int> UpdateAsync(T t)
        {
            m_sqlHelper.AddTable(typeof(T), DBTableOperatorEnum.Update);
            AddPrimaryKey(t);
            EnsureUpdateColumns();
            return await _executeBatch.ExecuteAsync(EndExecute());
        }
        public int UpdateList(List<T> list)
        {
            if (list == null || list.Count == 0)
            {
                return 0;
            }
            m_sqlHelper.AddTable(typeof(T), DBTableOperatorEnum.Update);
            m_sqlHelper.AddListParams(list);
            EnsureUpdateColumns();
            _executeBatch.BulkUpdate(EndExecute());
            return list.Count;
        }
        public async Task<int> UpdateListAsync(List<T> list)
        {
            if (list == null || list.Count == 0)
            {
                return 0;
            }
            m_sqlHelper.AddTable(typeof(T), DBTableOperatorEnum.Update);
            m_sqlHelper.AddListParams(list);
            EnsureUpdateColumns();
            await _executeBatch.BulkUpdateAsync(EndExecute());
            return list.Count;
        }
        public IBaseRepository<T> UpdateColumnsWithValue(Expression<Func<T>> property)
        {
            m_sqlHelper.AddHeader(property);
            return this;
        }
        public IBaseRepository<T> UpdateColumns(Expression<Func<T, object>> property)
        {
            m_sqlHelper.AddOperatorColumns(property);
            return this;
        }

        public int Delete()
        {
            m_sqlHelper.AddTable(typeof(T), DBTableOperatorEnum.Delete);
            return _executeBatch.Execute(EndExecute());
        }
        public async Task<int> DeleteAsync()
        {
            m_sqlHelper.AddTable(typeof(T), DBTableOperatorEnum.Delete);
            await _executeBatch.BulkDeleteAsync(EndExecute());
            return 1;
        }
        public int Delete(T t)
        {
            m_sqlHelper.AddTable(typeof(T), DBTableOperatorEnum.Delete);
            AddPrimaryKey(t);
            _executeBatch.BulkDelete(EndExecute());
            return 1;
        }
        public async Task<int> DeleteAsync(T t)
        {
            m_sqlHelper.AddTable(typeof(T), DBTableOperatorEnum.Delete);
            AddPrimaryKey(t);
            return await _executeBatch.ExecuteAsync(EndExecute());
        }
        public int Delete(List<T> t)
        {
            if (t == null || t.Count == 0)
            {
                return 0;
            }
            m_sqlHelper.AddTable(typeof(T), DBTableOperatorEnum.Delete);
            m_sqlHelper.AddListParams(t);
            _executeBatch.BulkDelete(EndExecute());
            return t.Count;
        }
        public async Task<int> DeleteAsync(List<T> t)
        {
            if (t == null || t.Count == 0)
            {
                return 0;
            }
            m_sqlHelper.AddTable(typeof(T), DBTableOperatorEnum.Delete);
            m_sqlHelper.AddListParams(t);
            await _executeBatch.BulkDeleteAsync(EndExecute());
            return t.Count;
        }

        public int Insert(T insert)
        {
            if (insert == null)
            {
                throw new ArgumentNullException(nameof(insert));
            }
            m_sqlHelper.AddTable(typeof(T), DBTableOperatorEnum.Insert);
            m_sqlHelper.AddListParams(new List<T> { insert });
            return _executeBatch.Execute(EndExecute());
        }
        public async Task<int> InsertAsync(T insert)
        {
            if (insert == null)
            {
                throw new ArgumentNullException(nameof(insert));
            }
            m_sqlHelper.AddTable(typeof(T), DBTableOperatorEnum.Insert);
            m_sqlHelper.AddListParams(new List<T> { insert });
            return await _executeBatch.ExecuteAsync(EndExecute());
        }
        public int InsertList(List<T> list)
        {
            if (list == null || list.Count == 0)
            {
                return 0;
            }
            m_sqlHelper.AddTable(typeof(T), DBTableOperatorEnum.Insert);
            m_sqlHelper.AddListParams(list);
            _executeBatch.BulkInsert(EndExecute());
            return list.Count;
        }
        public async Task<int> InsertListAsync(List<T> list)
        {
            if (list == null || list.Count == 0)
            {
                return 0;
            }
            m_sqlHelper.AddTable(typeof(T), DBTableOperatorEnum.Insert);
            m_sqlHelper.AddListParams(list);
            await _executeBatch.BulkInsertAsync(EndExecute());
            return list.Count;
        }

        private void AddParameters(object parameters)
        {
            if (parameters != null)
            {
                m_sqlHelper.AddDynamicParams(parameters);
            }
        }
        private void AddPrimaryKey(object entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            if (m_sqlHelper.Where.Count() != 0)
            {
                m_sqlHelper.Where.AddOperator(DBFiledOperatorEnum.And);
            }
            m_sqlHelper.AddPKWhere(typeof(T));
            m_sqlHelper.AddDynamicParams(entity);
        }
        private void EnsureUpdateColumns()
        {
            if (m_sqlHelper.UpdateHeader.Count() == 0 && m_sqlHelper.OperatorColumns.Count() == 0)
            {
                m_sqlHelper.AddUpdate(typeof(T));
            }
        }
        /// <summary>
        /// 调用方传入的 sql 不带分页。ClickHouse 使用 LIMIT offset, count。
        /// </summary>
        private static string BuildPageSql(string sql, PageInfo pageInfo)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException("sql不能为空", nameof(sql));
            }
            if (pageInfo == null)
            {
                throw new ArgumentNullException(nameof(pageInfo));
            }
            sql = sql.Trim().TrimEnd(';');
            int pageIndex = pageInfo.PageIndex < 1 ? 1 : pageInfo.PageIndex;
            int pageSize = pageInfo.PageSize < 1 ? 10 : pageInfo.PageSize;
            int offset = (pageIndex - 1) * pageSize;
            if (!string.IsNullOrWhiteSpace(pageInfo.OrderBy))
            {
                sql += " ORDER BY " + pageInfo.OrderBy;
            }
            return $"{sql} LIMIT {offset}, {pageSize}";
        }
        private static string BuildCountSql(string sql)
        {
            sql = sql.Trim().TrimEnd(';');
            return $"SELECT COUNT(1) FROM ({sql}) AS _page";
        }
    }
}
