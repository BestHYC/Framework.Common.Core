using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Core.ORM
{
    internal enum DBFiledOperatorEnum
    {
        /// <summary>
     /// 无,初始化使用
     /// </summary>
        None,
        /// <summary>
        /// 左小括号 (
        /// </summary>
        LeftBrace,
        /// <summary>
        /// 右小括号 )
        /// </summary>
        RightBrace,
        /// <summary>
        /// 相等于 like 
        /// </summary>
        LIKE,
        /// <summary>
        /// 相等于 = 
        /// </summary>
        Equal,
        /// <summary>
        /// 不等于 !=
        /// </summary>
        NotEqual,
        /// <summary>
        /// 为空 is null
        /// </summary>
        EqualNull,
        /// <summary>
        /// 不为空 is not null
        /// </summary>
        EqualNotNull,
        /// <summary>
        /// 以及 and 
        /// </summary>
        And,
        /// <summary>
        /// 或 or
        /// </summary>
        Or,
        /// <summary>
        /// 选择限定性条件 in
        /// </summary>
        In,
        /// <summary>
        /// On 条件
        /// </summary>
        On,
        /// <summary>
        /// where 条件
        /// </summary>
        Where,
        /// <summary>
        /// 排序 order by 
        /// </summary>
        OrderBy,
        /// <summary>
        /// having 排序
        /// </summary>
        Having,
        /// <summary>
        /// 选择
        /// </summary>
        Select,
        /// <summary>
        /// 大于等于
        /// </summary>
        GreaterThan,
        /// <summary>
        /// 大于
        /// </summary>
        GreaterThanOrEqual,
        /// <summary>
        /// 小于等于
        /// </summary>
        LessThanOrEqual,
        /// <summary>
        /// 小于
        /// </summary>
        LessThan,
        /// <summary>
        /// top 选择
        /// </summary>
        Top,
        /// <summary>
        /// top 选择
        /// </summary>
        OrderByDesc,
        /// <summary>
        /// 
        /// </summary>
        Count,
    }
}
