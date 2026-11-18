using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Core.ORM
{
    internal class ColumnRelevanceMapper
    {
        public DBFiledOperatorEnum DBOperatorEnum { get; set; }
        public String ColumnName { get; set; }
        /// <summary>
        /// 此字段在查询结果中的别名,如果没有,则取数据库默认值
        /// </summary>
        public String ASName { get; set; }
        public Type TableName { get; set; }
    }
}
