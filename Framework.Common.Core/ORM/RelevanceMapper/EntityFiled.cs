using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Core.ORM
{
    internal class EntityField
    {
        /// <summary>
        /// 属性名称,用于反射获取属性值
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// 对应的数据库字段名称
        /// </summary>
        public string DBFieldName { get; set; }
        /// <summary>
        /// 是否自增,自增的话不对其进行插入操作,也不对其进行修改操作
        /// </summary>
        public bool IsIncrease { get; set; }
        /// <summary>
        /// 是否主键, 主键的话,在修改的时候不对其进行修改操作,但是 插入会判断是否是自增,如果不是自增,则 判断是否有值,如果没有值,则抛出异常,如果有值,则插入
        /// </summary>
        public bool IsPK { get; set; }
    }
}
