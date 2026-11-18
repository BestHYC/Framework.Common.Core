using Dapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Core
{
    public interface IBaseRepository<T> where T : class, new()
    {
        IBaseRepository<T> Where(Expression<Func<T, bool>> where);
        IBaseRepository<T> OrderBy(Expression<Func<T, Object>> property);
        IBaseRepository<T> OrderByDesc(Expression<Func<T, Object>> property);
        IBaseRepository<T> Select(Expression<Func<T, object>> select);
        IBaseRepository<T> Top(Int32 num);
        IEnumerable<T> Query();
        Task<IEnumerable<T>> QueryAsync();
        T QueryFirst();
        Task<T> QueryFirstAsync();
        List<T> QueryList();
        Task<List<T>> QueryListAsync();
        IEnumerable<T> Query(String sql, DynamicParameters parameters = null);
        Task<IEnumerable<T>> QueryAsync(String sql, DynamicParameters parameters = null);
        IEnumerable<K> Query<K>(String sql, DynamicParameters parameters = null);
        Task<IEnumerable<K>> QueryAsync<K>(String sql, DynamicParameters parameters = null);
        List<T> QueryList(String sql, DynamicParameters parameters = null);
        Task<List<T>> QueryListAsync(String sql, DynamicParameters parameters = null);
        List<T> QueryByPage(string sql, PageInfo pageInfo, Object parameters);
        (List<T> list, Int32 totalCount) QueryByPageWithTotal(string sql, PageInfo pageInfo, Object parameters);
        Task<List<T>> QueryByPageAsync(string sql, PageInfo pageInfo, Object parameters);
        Task<(List<T> list, Int32 totalCount)> QueryByPageWithTotalAsync(string sql, PageInfo pageInfo, Object parameters);
        IEnumerable<T> QueryByPK(Object t);
        Task<IEnumerable<T>> QueryByPKAsync(Object t);
        Int32 Update();
        Task<Int32> UpdateAsync();
        Int32 Update(T t);
        Task<Int32> UpdateAsync(T t);
        Int32 UpdateList(List<T> list);
        Task<Int32> UpdateListAsync(List<T> list);
        /// <summary>
        /// 修改字段值,Expression<Func<Test01>> property = ()=> new Test01() { Name = "123", Age = 123 }; 
        /// 则修改Name和Age字段
        /// 注意一定与确定的值使用, 全局使用 也必须是 where (i => i.Id > 0).UpdateColumnsWithValue(() => new Test01() { Name = "123", Age = 123 }); 这样才会生效
        /// 如果是外部的传参, 那么 也必须是 主键 可以查询的数据
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        IBaseRepository<T> UpdateColumnsWithValue(Expression<Func<T>> property);
        /// <summary>
        /// 只允许操作 item => new { item.Name } 表示 对Name字段进行了修改, 
        /// UpdateColumns(item => new { item.Name });
        /// 注意一定与确定的值使用, 这里 必须是 存在的 T, 而不是 where 查出来的数据
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        IBaseRepository<T> UpdateColumns(Expression<Func<T, Object>> property);
        /// <summary>
        /// 以下 一定是 主键对应的数据, 否则 会报错, 
        /// 注意 如果 只有 主键 如Id =1 那么可以 where(i => i.Id == 1).Delete(); 也可以 Delete(new Test01() { Id = 1 });
        /// </summary>
        /// <returns></returns>
        Int32 Delete();
        Task<Int32> DeleteAsync();
        Int32 Delete(T t);
        Task<Int32> DeleteAsync(T t);
        Int32 Delete(List<T> t);
        Task<Int32> DeleteAsync(List<T> t);
        Int32 Insert(T insert);
        Task<Int32> InsertAsync(T insert);
        Int32 InsertList(List<T> list);
        Task<Int32> InsertListAsync(List<T> list);
    }
}
