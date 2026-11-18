using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Core.ORM
{
    /// <summary>
    /// 准备精简ORM,只处理单表的增删改查操作,所以这个意义不大
    /// 多表的查询 还是使用SQL处理, 这样维护 及 索引优化都比较好
    /// </summary>
    internal class TableRelevanceMapper
    {
        public TableRelevanceMapper(Type t, DBTableOperatorEnum tableOperator)
        {
            TableName = t;
            TableOperatorEnum = tableOperator;
        }
        /// <summary>
        /// 关联表结构关系 如Inner Join Right Join
        /// </summary>
        public DBTableOperatorEnum TableOperatorEnum { get; set; }
        public Type TableName { get; set; }
    }
}
