using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Core.ORM
{
    internal class EntityTableWrapper
    {
        private static Object _lock = new Object();
        private static Dictionary<Type, EntityMapTable> ValuePairs =
            new Dictionary<Type, EntityMapTable>();
        public static void Add(Type type)
        {
            if (type == null) throw new ArgumentException("类型不能为空");
            if (ValuePairs.ContainsKey(type)) return;
            lock (_lock)
            {
                if (ValuePairs.ContainsKey(type)) return;
                Type t = type;
                EntityMapTable value = new EntityMapTable(type);
                ValuePairs.Add(t, value);
            }
        }
        public static List<EntityField> GetAllColumns(Type type)
        {
            if (type == null) throw new ArgumentException("类型不能为空");
            Add(type);
            if (!ValuePairs.ContainsKey(type) || ValuePairs[type].FieldList == null) throw new ArgumentException("此类型有异常");
            return ValuePairs[type].FieldList.Values.ToList();
        }
        public static List<String> GetDBFieldNames(Type type)
        {
            var list = GetAllColumns(type);
            return list.Select(i => i.DBFieldName).ToList();
        }
        public static List<String> GetPropertyNames(Type type)
        {
            var list = GetAllColumns(type);
            return list.Select(i => i.PropertyName).ToList();
        }
        public static EntityField GetIncrementField(Type type)
        {
            if (type == null) throw new ArgumentException("类型不能为空");
            Add(type);
            if (!ValuePairs.ContainsKey(type)) throw new ArgumentException("此类型有异常");
            return ValuePairs[type].IncrementField;
        }
        public static Boolean CheckIncrementField(Type type, String propertyName)
        {
            var val = GetIncrementField(type);
            if (val == null) return false;
            return String.Equals(val.PropertyName, propertyName);
        }
        public static IEnumerable<EntityField> GetPKFields(Type type)
        {
            Add(type);
            return ValuePairs[type].PKFieldList;
        }
        public static String GetTableName(Type type)
        {
            if (type == null) throw new ArgumentException("请传入对应的表类型");
            Add(type);
            return ValuePairs[type].TableName;
        }
        public static EntityField GetColumnInfo(Type type, String propertyName)
        {
            if (String.IsNullOrEmpty(propertyName)) throw new ArgumentException("Table对应的属性值不能为空");
            Add(type);
            var table = ValuePairs[type];
            if (table.FieldList != null && table.FieldList.Count > 0)
            {
                if (table.FieldList.ContainsKey(propertyName))
                {
                    return table.FieldList[propertyName];
                }
            }
            return null;
        }
        public static String GetDBFieldName(Type type, String name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentException("Table对应的属性值不能为空");
            var value = GetColumnInfo(type, name);
            if (value == null) return string.Empty;
            return value.DBFieldName;
        }
    }
}
