using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Core.ORM
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TabelNameAttribute : Attribute
    {
        public String TableName { get; }
        public TabelNameAttribute(String tableName)
        {
            TableName = tableName;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnNameAttribute : Attribute
    {
        public String ColumnName { get; }
        public String ColumnType { get; set; }
        public ColumnNameAttribute(String columnName, String columnType)
        {
            ColumnName = columnName;
            ColumnType = columnType;
        }
    }
    /// <summary>
    /// 主键特性
    /// </summary>
    public class PKAttribute : Attribute
    {
    }
    /// <summary>
    /// 自增特性
    /// </summary>
    public class IncrementAttribute : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class SelectIndexAttribute : Attribute
    {
        public String[] IndexNames { get; }
        public SelectIndexAttribute(params String[] indexName)
        {
            IndexNames = indexName.ToArray();
        }
    }
    /// <summary>
    /// 实体中此字段不做sql处理
    /// 常见中为
    /// public int Flag{get;set;}
    /// [Ignore]
    /// public String FlagString{
    /// get
    ///    {
    ///     if(Flag==0)
    ///        return"确认";
    ///     else return "取消";
    ///    }
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class IgnoreAttribute : Attribute
    {
    }
    public class ColumnTypeString
    {
        public const String Bool = "Bool";
        public const String UInt8 = "byte";
        public const String Int8 = "sbyte";
        public const String Int16 = "Int16";
        public const String Int32 = "Int32";
        public const String Int64 = "Int64";
        public const String Decimal = "Decimal";
        public const String String = "String";
        public const String Double = "Double";
        public const String Boolean = "Boolean";
        public const String Float16 = "Float16";
        public const String Float32 = "Float32";
        public const String Float64 = "Float64";
        public const String Date = "DateTime";
        public const String DateTime = "DateTime";
        public const String DateTime64 = "DateTime64";
        public const String Decimal_18_2 = "Decimal(18, 2)";
    }
}
