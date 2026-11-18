using System;
using System.Data.Common;
using System.Text;

namespace Framework.Common.Core.ORM
{
    public class CKBaseRepository<T> : BaseRepository<T> where T : class, new()
    {
        public CKBaseRepository(CKExecuteBatch<T> executeBatch) : base(executeBatch)
        {
        }

        /// <summary>
        /// CK 不建议使用 SetProperty(item => item.Age = 13 ).Where(item => item.Name = "xxx").Update(t);
        /// 因为这种先去CK库查询数据，然后新增
        /// 新增也不建议使用 Where().Insert()，目前也不实现，只实现 Insert(T t) Insert(List<T> t)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        protected override IBatch EndExecute()
        {
            try
            {
                CKBatch batch = new CKBatch();
                TableRelevanceMapper table = m_sqlHelper.Table;
                switch (table.TableOperatorEnum)
                {
                    case DBTableOperatorEnum.Insert:
                        return InsertPreExecute();
                    case DBTableOperatorEnum.Update:
                        return UpdatePreExecute();
                    case DBTableOperatorEnum.Delete:
                        return DeletePreExecute();
                    default:
                        throw new NotSupportedException();
                }
            }
            finally
            {
                m_sqlHelper.Clear(typeof(T));
            }
        }
        /// <summary>
        /// Query没有额外的从数据库查询的操作，所以没有必要执行异步
        /// </summary>
        /// <returns></returns>
        protected override IBatch EndQuery()
        {
            try
            {
                IBatch batch = new CKBatch();
                StringBuilder sb = new StringBuilder();
                StringBuilder querySql = ConvertToQuery();
                String tableName = EntityTableWrapper.GetTableName(m_sqlHelper.Table.TableName);
                StringBuilder whereSql = ConvertToWhere();
                StringBuilder orderBySql = ConvertToOrderBy();
                String top = ConvertTop();
                sb.Append($"SELECT  ").Append(top).Append(querySql).AppendLine();
                sb.Append(" FROM ").Append(tableName).Append(" FINAL ").AppendLine();
                sb.Append($" WHERE {tableName}.sign = 1");
                if (whereSql.Length > 0)
                {
                    sb.Append(" AND ( ").Append(whereSql).Append(" ) ").AppendLine();
                }
                if (orderBySql.Length > 0)
                {
                    sb.Append(" ").Append(DBFiledOperatorEnum.OrderBy.GetOperator()).Append(" ").Append(orderBySql).AppendLine();
                }
                batch.Sql = sb.ToString();
                batch.DynamicParameters = m_sqlHelper.Parameters;
                return batch;
            }
            finally
            {
                m_sqlHelper.Clear(typeof(T));
            }
        }
        private String ConvertTop()
        {
            var top = m_sqlHelper.Top;
            if (top == 0) return String.Empty;
            return $" {DBFiledOperatorEnum.Top.GetOperator()} {top} ";
        }
        /// <summary>
        /// 原本以下的方法都应该在 CKBatch 里，但是现在直接不用
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private StringBuilder ConvertToQuery()
        {
            StringBuilder sb = new StringBuilder(256);
            ParameterWrapper queryReduce = m_sqlHelper.Query;
            if (queryReduce == null || queryReduce.Columns.Count == 0)
            {
                sb.Append(" * ");
                return sb;
            }
            foreach (var column in queryReduce.Columns)
            {
                if (sb.Length != 0)
                {
                    sb.Append(",");
                }
                if (column.TableName == null)
                {
                    if (String.IsNullOrWhiteSpace(column.ColumnName) && String.IsNullOrWhiteSpace(column.ASName)) continue;
                    if (!String.IsNullOrWhiteSpace(column.ColumnName))
                    {
                        sb.Append(column.ColumnName);
                        if (!String.IsNullOrWhiteSpace(column.ASName))
                        {
                            sb.Append(" AS ");
                        }
                    }
                    sb.Append($"{column.ASName} ");
                    continue;
                }
                String tablename = EntityTableWrapper.GetTableName(column.TableName);
                String columnname = EntityTableWrapper.GetDBFieldName(column.TableName, column.ColumnName);
                sb.Append($"{tablename}.{columnname} AS {column.ASName}");
            }
            return sb;
        }
        private StringBuilder ConvertToWhere()
        {
            StringBuilder sb = new StringBuilder();
            var where = m_sqlHelper.Where;
            Boolean isNullOrNot = false;
            foreach (var column in where.Columns)
            {
                String str = ConditionConvert(column, isNullOrNot);
                if (String.IsNullOrEmpty(str))
                {
                    isNullOrNot = true;
                }
                else
                {
                    isNullOrNot = false;
                    sb.Append($" {str} ");
                }
            }
            return sb;
        }
        private StringBuilder ConvertToOrderBy()
        {
            StringBuilder sb = new StringBuilder();
            var where = m_sqlHelper.Orderby;
            if (where == null || where.Columns.Count == 0) return sb;
            DBFiledOperatorEnum operatorEnum = DBFiledOperatorEnum.None;
            foreach (var column in where.Columns)
            {
                if (sb.Length > 0) sb.Append(" , ");
                String tablename = EntityTableWrapper.GetTableName(column.TableName);
                String columnname = EntityTableWrapper.GetDBFieldName(column.TableName, column.ColumnName);
                sb.Append($"{tablename}.{columnname}");
                if (column.DBOperatorEnum != operatorEnum && column.DBOperatorEnum != DBFiledOperatorEnum.None) operatorEnum = column.DBOperatorEnum;
                if (operatorEnum == DBFiledOperatorEnum.OrderByDesc) sb.Append($" {DBFiledOperatorEnum.OrderByDesc.GetOperator()} ");
            }
            return sb;
        }
        /// <summary>
        /// 新增数据 不允许 Where(i=> i.id == 1).Insert();
        /// 只允许Insert(T t) Insert(List<T> list)
        /// 即此处不做任何处理，也无需对数据进行填充
        /// </summary>
        /// <param name="tuple"></param>
        /// <returns></returns>
        private IBatch InsertPreExecute()
        {
            CKBatch batch = new CKBatch();
            var list = m_sqlHelper.ListParams as List<T>;
            batch.Value = list;
            return batch;
        }
        /// <summary>
        /// CK 不允许操作 SerProperty(() => new T01(){ Age = "xx"}).Where(item => item.id == 1).Update();
        /// 如果是如此的建议先查询，在去执行异步新增到kafka
        /// </summary>
        /// <returns></returns>
        /// <exception cref="CustomException"></exception>
        private IBatch UpdatePreExecute()
        {
            CKBatch batch = new CKBatch();
            var updateColumns = m_sqlHelper.OperatorColumns;
            var list = m_sqlHelper.ListParams as List<T>;
            var where = m_sqlHelper.Where;
            if ((where == null || where.Columns.Count == 0) && list == null) throw new ArgumentException("Update 操作 请添加限定范围执行参数");
            if (list == null)
            {
                var setColumns = m_sqlHelper.UpdateHeader;
                if (setColumns == null || setColumns.Count() == 0) return batch;
                //注意此EndQuery 会释放所有的m_sqlHelper
                list = QueryList();
                if (list == null || list.Count == 0) return batch;
                Dictionary<String, Object> dic = new Dictionary<String, Object>();
                for (int i = 0; i < setColumns.Count();)
                {
                    var x1 = setColumns.Columns[i];
                    var x2 = setColumns.Columns[i + 1];
                    if (x1.TableName != null)
                    {
                        dic.Add(x1.ColumnName, setColumns.Parameters.Get<Object>($"@{x2.ColumnName}"));
                    }
                    else
                    {
                        dic.Add(x2.ColumnName, setColumns.Parameters.Get<Object>($"@{x1.ColumnName}"));
                    }
                    i += 2;
                }
                foreach (var t in list)
                {
                    foreach (var x in dic)
                    {
                        SetValue(t, x.Key, x.Value);
                    }
                }
                m_sqlHelper.AddListParams(list);
                batch.Value = list;
            }
            else
            {
                if (updateColumns == null || updateColumns.Columns.Count == 0)
                {
                    batch.Value = list;
                    return batch;
                }
                List<T> queryAll = new List<T>();
                List<String> listColumn = updateColumns.Columns.Select(x => x.ColumnName).ToList();

                if (listColumn == null || !listColumn.Any()) throw new InvalidOperationException("You must select a field");
                foreach (var t in list)
                {
                    T queryFromDB = QueryByPK(t)?.FirstOrDefault();

                    if (queryFromDB == null)
                    {
                        throw new InvalidOperationException("sourceList is null or empty");
                    }
                    queryAll.Add(queryFromDB);
                    SetValue(t, queryFromDB, listColumn);
                }
                m_sqlHelper.AddListParams(queryAll);
                batch.Value = queryAll;
            }
            return batch;
        }
        /// <summary>
        /// DeleteByPK(T t) 和  Where().Delete() 在CK中 都先查询，在删除，即使 t 是完整的数据，Delete(T t) 直接删除
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private IBatch DeletePreExecute()
        {
            CKBatch batch = new CKBatch();
            var list = m_sqlHelper.ListParams as List<T>;
            var where = m_sqlHelper.Where;
            batch.Value = list;
            if ((where == null || where.Columns.Count == 0) && list == null) throw new ArgumentException("Delete 操作 请添加限定范围执行参数");
            if (list == null)
            {
                list = QueryList();
                if (list == null || list.Count == 0) return batch;
                m_sqlHelper.AddListParams(list);
                batch.Value = list;
            }
            return batch;
        }
        private void SetValue(T tIn, T tResult, List<String> names)
        {
            foreach (var name in names)
            {
                var property = typeof(T).GetProperty(name);
                property.SetValue(tResult, property.GetValue(tIn));
            }
        }
        private void SetValue(T tResult, String name, Object val)
        {
            var property = typeof(T).GetProperty(name);
            property.SetValue(tResult, val);
        }
        private String ConditionConvert(ColumnRelevanceMapper column, bool isNull = false)
        {
            StringBuilder sb = new StringBuilder();
            //如果 null == item.id 中写法 null,那么返回"", == item.id 返回item.id is null
            //如果是 item.id == null 中写法 那么 == null 则返回 is null
            if (column.ColumnName == null && column.DBOperatorEnum == DBFiledOperatorEnum.None)
            {
                return "";
            }
            if (isNull || column.ColumnName == null)
            {
                column.DBOperatorEnum = DBEnumConvert(column.DBOperatorEnum);
            }
            if (column.DBOperatorEnum == DBFiledOperatorEnum.EqualNotNull ||
                column.DBOperatorEnum == DBFiledOperatorEnum.EqualNull)
            {
                String str = "";
                if (!String.IsNullOrEmpty(column.ColumnName))
                {
                    Type t = column.TableName;
                    String tableName = EntityTableWrapper.GetTableName(t);
                    String columnName = EntityTableWrapper.GetDBFieldName(t, column.ColumnName);
                    str = $"{tableName}.{columnName}";
                }
                return $"{str} {column.DBOperatorEnum.GetOperator()}";
            }
            if (column.DBOperatorEnum != DBFiledOperatorEnum.None)
            {
                sb.Append(column.DBOperatorEnum.GetOperator());
            }
            //如果是 item.id = 1 中的  = 1,那么只 保存 = @ColumnName
            if (column.TableName == null)
            {
                sb.Append($"@{column.ColumnName}");
            }
            else
            {
                Type t = column.TableName;
                String tableName = EntityTableWrapper.GetTableName(t);
                String columnName = EntityTableWrapper.GetDBFieldName(t, column.ColumnName);
                sb.Append($" {tableName}.{columnName} ");
            }
            return sb.ToString();
        }
        private DBFiledOperatorEnum DBEnumConvert(DBFiledOperatorEnum dbFiledOperatorEnum)
        {
            if (dbFiledOperatorEnum == DBFiledOperatorEnum.Equal)
            {
                return DBFiledOperatorEnum.EqualNull;
            }
            if (dbFiledOperatorEnum == DBFiledOperatorEnum.NotEqual)
            {
                return DBFiledOperatorEnum.EqualNotNull;
            }
            return DBFiledOperatorEnum.None;
        }
    }
}
