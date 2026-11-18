using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Core.ORM
{
    /// <summary>
    /// 实体表包装类,用于存储实体表的相关信息
    /// </summary>
    internal class EntityMapTable
    {/// <summary>
     /// 表名
     /// </summary>
        public String TableName { get; private set; }
        /// <summary>
        /// 自增字段，一般只有1个，
        /// </summary>
        public EntityField IncrementField { get; private set; }
        /// <summary>
        /// 主键索引字段列表
        /// </summary>
        public IEnumerable<EntityField> PKFieldList { get; private set; }
        /// <summary>
        /// key 是属性字段名，Value是对应的信息
        /// </summary>
        public Dictionary<String, EntityField> FieldList { get; private set; }
        private Type m_Type;
        public EntityMapTable(Type type)
        {
            m_Type = type;
            Init();
        }
        private void Init()
        {
            TableName = m_Type.GetCustomAttributes<TabelNameAttribute>().FirstOrDefault()?.TableName;
            List<EntityField> pkList = new List<EntityField>();
            Dictionary<string, EntityField> columns = new Dictionary<string, EntityField>();

            foreach (PropertyInfo pi in m_Type.GetProperties())
            {
                if (pi.GetCustomAttribute(typeof(IgnoreAttribute)) != null) continue;
                String name = pi.Name;
                ColumnNameAttribute attribute = pi.GetCustomAttribute<ColumnNameAttribute>();
                if (attribute != null)
                {
                    name = attribute.ColumnName;
                }
                EntityField propertyValue = new EntityField()
                {
                    PropertyName = pi.Name,
                    DBFieldName = name
                };
                columns.Add(pi.Name, propertyValue);
                PKAttribute pk = pi.GetCustomAttribute<PKAttribute>();
                if (pk != null)
                {
                    pkList.Add(propertyValue);
                    propertyValue.IsPK = true;
                }

                IncrementAttribute increment = pi.GetCustomAttribute<IncrementAttribute>();

                //自增主键只允许一个自增
                if (increment != null)
                {
                    IncrementField = propertyValue;
                    propertyValue.IsIncrease = true;
                }
            }
            FieldList = columns;
            PKFieldList = pkList;
        }
    }
}
