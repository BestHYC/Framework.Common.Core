using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Core.ORM
{
    internal enum DBTableOperatorEnum
    {
        /// <summary>
        /// 无连接
        /// </summary>
        None,
        /// <summary>
        /// 左连接
        /// </summary>
        LeftJoin,
        /// <summary>
        /// 内连接
        /// </summary>
        InnerJoin,
        /// <summary>
        /// 右连接
        /// </summary>
        RightJoin,
        /// <summary>
        /// 插入
        /// </summary>
        Insert,
        /// <summary>
        /// 修改
        /// </summary>
        Update,
        /// <summary>
        /// 删除
        /// </summary>
        Delete,
    }
}
