using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Framework.Common.Core.ORM
{
    /// <summary>
    /// 最终解析的批次信息
    /// 包括sql, 动态参数化入参 以及 update等 数组形式的 入参
    /// </summary>
    public interface IBatch
    {
        String Sql { get; set; }
        DynamicParameters DynamicParameters { get; set; }
        Object Value { get; set; }
    }
}
