using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Core.ORM
{
    internal class SQLHelper
    {
        public SQLHelper()
        {
            Parameters = new DynamicParameters();
            Query = new ParameterWrapper(Parameters);
            Table = null;
            Where = new ParameterWrapper(Parameters);
            Orderby = new ParameterWrapper(Parameters);
            UpdateHeader = new ParameterWrapper(Parameters);
            OperatorColumns = new ParameterWrapper(Parameters);
            ListParams = null;
        }
        /// <summary>
        /// 这个 和 ListParams 的区别是 ListParams 目前全部是 数组数据, 批量操作的时候 使用 Parameters 会有异常
        /// 注意 这个数据  和 Id in @list 的数组没关系, 是 多次执行 sql 的数组,而不是 单一 SQL中参数数组
        /// </summary>
        public DynamicParameters Parameters { get; private set; }
        /// <summary>
        /// 这个是批量修改 或者 批量 新增 的参数, 主要是为了避免 dapper 的 DynamicParameters 在批量操作的时候,会出现参数重复的异常
        /// </summary>
        public Object ListParams { get; private set; }
        public Int32 Top { get; private set; }
        /// <summary>
        /// 待查询的字段
        /// </summary>
        public ParameterWrapper Query { get; private set; }
        /// <summary>
        /// 此字段 与 Query 区别是, Query 是查询, Update 是 哪些字段需要修改,即 Update Table set name = "xx" where id =1; 中 name = "xxx" 的 解析
        /// </summary>
        public ParameterWrapper UpdateHeader { get; private set; }
        /// <summary>
        /// 待操作的字段, 与 上面的区别 在Sqlsever等库中 无区别,但是 CK会从数据库中查询数据,然后根据此字段解析的字段,赋值对应的数据
        /// </summary>
        public ParameterWrapper OperatorColumns { get; private set; }
        /// <summary>
        /// 这里现在只处理单表的关联查询,所以这个意义不大,多表的查询 还是使用SQL处理, 这样维护 及 索引优化都比较好
        /// </summary>
        public TableRelevanceMapper Table { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public ParameterWrapper Where { get; private set; }
        public ParameterWrapper Orderby { get; private set; }
        public SQLHelper AddTop(Int32 num)
        {
            Top = num;
            return this;
        }
        /// <summary>
        /// 添加查询 select a.name as name 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public SQLHelper AddQuery(Expression expression)
        {
            ConvertExpression(expression, Query);
            return this;
        }
        public SQLHelper AddQuery(Type type)
        {
            foreach (var item in EntityTableWrapper.GetAllColumns(type))
            {
                Query.Add(new ColumnRelevanceMapper() { ColumnName = item.DBFieldName, TableName = type, ASName = item.PropertyName });
            }
            return this;
        }
        /// <summary>
        /// 这里应该是去除主键
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public SQLHelper AddUpdate(Type type)
        {
            Boolean isFirst = true;
            foreach (var item in EntityTableWrapper.GetAllColumns(type))
            {
                var column = new ColumnRelevanceMapper() { ColumnName = item.DBFieldName, TableName = type };
                if (!isFirst)
                {
                    column.DBOperatorEnum = DBFiledOperatorEnum.And;
                }
                UpdateHeader.Add(column);
                UpdateHeader.Add(new ColumnRelevanceMapper() { ColumnName = item.DBFieldName, ASName = item.PropertyName, TableName = type, DBOperatorEnum = DBFiledOperatorEnum.Equal });
            }
            return this;
        }
        public SQLHelper AddPKWhere(Type type)
        {
            Boolean isFirst = true;
            foreach (var item in EntityTableWrapper.GetPKFields(type))
            {
                var column = new ColumnRelevanceMapper() { ColumnName = item.DBFieldName, ASName = item.PropertyName, TableName = type };
                if (!isFirst)
                {
                    column.DBOperatorEnum = DBFiledOperatorEnum.And;
                }
                isFirst = false;
                Where.Add(column);
                Where.Add(new ColumnRelevanceMapper() { ColumnName = item.DBFieldName, ASName = item.PropertyName, DBOperatorEnum = DBFiledOperatorEnum.Equal });
            }
            return this;
        }
        public SQLHelper AddHeader(Expression expression)
        {
            ConvertExpression(expression, UpdateHeader);
            return this;
        }
        public SQLHelper AddOperatorColumns(Expression expression)
        {
            ConvertExpression(expression, OperatorColumns);
            return this;
        }
        public SQLHelper AddDynamicParams(Object obj)
        {
            Parameters.AddDynamicParams(obj);
            return this;
        }
        public SQLHelper AddListParams(Object obj)
        {
            ListParams = obj;
            return this;
        }
        /// <summary>
        /// a as a inner join as b on a.id = b.id
        /// InnerJoin((a, b)=> a.id == b.id)
        /// </summary>
        /// <param name="tablename"></param>
        /// <returns></returns>
        public SQLHelper AddTable(Type t, DBTableOperatorEnum tableOperator = DBTableOperatorEnum.None, Expression expression = null)
        {
            TableRelevanceMapper tableRelevanceMapper = null;
            ParameterWrapper reduce = null;
            if (expression != null)
            {
                reduce = new ParameterWrapper(Parameters);
                ConvertExpression(expression, reduce);
                tableRelevanceMapper = new TableRelevanceMapper(t, tableOperator);
            }
            tableRelevanceMapper = new TableRelevanceMapper(t, tableOperator);
            Table = tableRelevanceMapper;
            return this;
        }
        public SQLHelper AddWhere(Expression expression)
        {
            if (Where.Count() != 0)
            {
                Where.AddOperator(DBFiledOperatorEnum.And);
            }
            ConvertExpression(expression, Where);
            return this;
        }
        public SQLHelper AddOrder(Expression expression, DBFiledOperatorEnum sqlOperator)
        {
            Orderby.AddOperator(sqlOperator);
            ConvertExpression(expression, Orderby);
            return this;
        }
        public void Clear(Type type)
        {
            Top = 0;
            Parameters = new DynamicParameters();
            ListParams = null;
            Query = new ParameterWrapper(Parameters);
            Table = null;
            Where = new ParameterWrapper(Parameters);
            Orderby = new ParameterWrapper(Parameters);
            UpdateHeader = new ParameterWrapper(Parameters);
            OperatorColumns = new ParameterWrapper(Parameters);
            AddTable(type);
        }
        private void ConvertExpression(Expression expression, ParameterWrapper reduce)
        {
            if (expression == null) return;
            expression.ConvertExpression(reduce);
        }
    }
}
