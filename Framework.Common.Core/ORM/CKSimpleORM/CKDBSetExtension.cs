using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Core.ORM
{
    public static class CKDBSetExtension
    {
        public static CKBaseRepository<T> Where<T>(this CKDBSet<T> set, Expression<Func<T, bool>> where) where T : class, new()
        {
            CKBaseRepository<T> repository = ServiceHelper.GetService<CKBaseRepository<T>>();
            repository.Where(where);
            return repository;
        }
        public static CKBaseRepository<T> Convert<T>(this CKDBSet<T> set) where T : class, new()
        {
            CKBaseRepository<T> repository = ServiceHelper.GetService<CKBaseRepository<T>>();
            return repository;
        }
        public static K ConvertService<T, K>(this CKDBSet<T> set) where T : class, new() where K : class
        {
            CKBaseRepository<T> repository = ServiceHelper.GetService<CKBaseRepository<T>>();
            return repository as K;
        }
        public static CKBaseRepository<T> Select<T>(this CKDBSet<T> set, Expression<Func<T, Object>> select) where T : class, new()
        {
            CKBaseRepository<T> repository = ServiceHelper.GetService<CKBaseRepository<T>>();
            repository.Select(select);
            return repository;
        }
        public static CKBaseRepository<T> AddTop<T>(this CKDBSet<T> set, int num) where T : class, new()
        {
            CKBaseRepository<T> repository = ServiceHelper.GetService<CKBaseRepository<T>>();
            repository.Top(num);
            return repository;
        }
        public static List<T> Query<T>(this CKDBSet<T> context, string sql, DynamicParameters parameters) where T : class, new()
        {
            CKBaseRepository<T> repository = ServiceHelper.GetService<CKBaseRepository<T>>();
            return repository.QueryList(sql, parameters);
        }
        public static List<K> Query<T, K>(this CKDBSet<T> context, string sql, DynamicParameters parameters) where T : class, new()
        {
            CKBaseRepository<T> repository = ServiceHelper.GetService<CKBaseRepository<T>>();
            var result = repository.Query<K>(sql, parameters);
            if (result == null) return null;
            return result.ToList();
        }
        public static async Task<List<T>> QueryByPKAsync<T>(this CKDBSet<T> context, T t) where T : class, new()
        {
            CKBaseRepository<T> repository = ServiceHelper.GetService<CKBaseRepository<T>>();
            var result = await repository.QueryByPKAsync(t);
            if (result == null) return null;
            return result.ToList();
        }
        public static async Task<List<T>> QueryAsync<T>(this CKDBSet<T> context, string sql, DynamicParameters parameters) where T : class, new()
        {
            CKBaseRepository<T> repository = ServiceHelper.GetService<CKBaseRepository<T>>();
            return await repository.QueryListAsync(sql, parameters);
        }
        public static CKBaseRepository<T> Insert<T>(this CKDBSet<T> set, T list) where T : class, new()
        {
            CKBaseRepository<T> repository = ServiceHelper.GetService<CKBaseRepository<T>>();
            repository.Insert(list);
            return repository;
        }
        public static CKBaseRepository<T> InsertList<T>(this CKDBSet<T> set, List<T> list) where T : class, new()
        {
            CKBaseRepository<T> repository = ServiceHelper.GetService<CKBaseRepository<T>>();
            repository.InsertList(list);
            return repository;
        }
        public static CKBaseRepository<T> Update<T>(this CKDBSet<T> set, T list) where T : class, new()
        {
            CKBaseRepository<T> repository = ServiceHelper.GetService<CKBaseRepository<T>>();
            repository.Update(list);
            return repository;
        }
        public static CKBaseRepository<T> UpdateList<T>(this CKDBSet<T> set, List<T> list) where T : class, new()
        {
            CKBaseRepository<T> repository = ServiceHelper.GetService<CKBaseRepository<T>>();
            repository.UpdateList(list);
            return repository;
        }
        public static List<T> Query<T>(this CKBaseRepository<T> context, string sql, DynamicParameters parameters) where T : class, new()
        {
            var context1 = ServiceHelper.GetService<CKDBContext>();
            CKExecuteBatch<T> batch = new CKExecuteBatch<T>(context1);
            return batch.QueryList(sql, parameters);
        }
        public static List<T> QueryByPage<T>(this CKBaseRepository<T> context, string sql, PageInfo pageInfo, DynamicParameters parameters) where T : class, new()
        {
            CKBaseRepository<T> repository = ServiceHelper.GetService<CKBaseRepository<T>>();
            return repository.QueryByPage(sql, pageInfo, parameters);
        }
        public static async Task<List<T>> QueryByPageAsync<T>(this CKBaseRepository<T> context, string sql, PageInfo pageInfo, DynamicParameters parameters) where T : class, new()
        {
            CKBaseRepository<T> repository = ServiceHelper.GetService<CKBaseRepository<T>>();
            return await repository.QueryByPageAsync(sql, pageInfo, parameters);
        }
        public static void AddList<T>(this DynamicParameters dynamicParameters, String name, IEnumerable<T> list)
        {
            dynamicParameters.Add(name, list.ToArray());
        }
    }
}
