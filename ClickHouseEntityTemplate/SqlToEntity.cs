using System;
using System.Collections.Generic;
using System.Text;

namespace ClickHouseEntityTemplate
{
    public static class SqlToEntity
    {
        private static List<String> noneColumn = new List<string>() { "version", "sign", "_insert_time" };

        public static String ConverToEntity(String tableName, IEnumerable<DescriptionTable> list, String nameSpace)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using pacvue.walmartICSams.Common.ORM;");
            sb.Append("namespace ").AppendLine(nameSpace).AppendLine("{");
            sb.Append("[CKTabelName(\"").Append(tableName).AppendLine("\")]");
            sb.Append("public class ").Append(CapitalizeFirstLetter(tableName)).AppendLine("Model").AppendLine("{");

            foreach (var item in list)
            {
                if (noneColumn.Contains(item.Name)) continue;
                String columenType = ConvertToType(item.Type, out String cKColumnName);
                if (!String.IsNullOrEmpty(item.Comment))
                {
                    sb.AppendLine(" /// <summary> ").Append(" /// ").AppendLine(item.Comment).AppendLine(" /// </summary> ");
                }
                sb.Append("[CKColumnName(\"").Append(item.Name).Append("\",").Append(cKColumnName).AppendLine(")]");
                if (item.is_in_primary_key)
                {
                    sb.AppendLine("[PK]");
                }
                sb.Append("public ").Append(columenType).Append(" ").Append(CapitalizeFirstLetter(item.Name)).AppendLine(" { get; set; }");
            }
            sb.AppendLine("}").AppendLine("}");
            return sb.ToString();
        }
        public static String ConvertToRepository(String tableName, String nameSpace)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using pacvue.common.clickHousebase;");
            sb.AppendLine("using pacvue.walmartICSams.Common.ORM;");
            sb.Append("namespace ").AppendLine(nameSpace).AppendLine("{");
            sb.Append("public class ").Append(CapitalizeFirstLetter(tableName)).Append("CKManage : CKBaseRepository<")
                .Append(CapitalizeFirstLetter(tableName)).Append("Model").Append(">").Append(",I").Append(CapitalizeFirstLetter(tableName).Substring(0, tableName.Length - 4)).Append("Repository").AppendLine("{");
            //        public Profile_allCKManage(CKExecuteBatch<Profile_allModel> batch, ClickhouseKfkHelper kfkHelper) : base(batch, kfkHelper)
            //{
            //}
            sb.Append("public ").Append(CapitalizeFirstLetter(tableName)).Append("CKManage(PacvueCKDBContext context , ClickhouseKfkHelper kfkHelper) : base(context, kfkHelper)");
            sb.AppendLine("{").AppendLine("}");
            sb.AppendLine("}").AppendLine("}");
            return sb.ToString();
        }
        public static String ConvertToInterface(String item, String nameSpace)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using pacvue.walmartICSams.Common.ORM;");
            sb.Append("namespace ").AppendLine(nameSpace).AppendLine("{");
            sb.Append("public interface ").Append("I").Append(CapitalizeFirstLetter(item).Substring(0, item.Length - 4)).Append("Repository").Append(":IBaseRepository<").Append(CapitalizeFirstLetter(item)).Append("Model>, IBatchRepository<").Append(CapitalizeFirstLetter(item)).AppendLine("Model>{").AppendLine("}");
            sb.AppendLine("}");
            return sb.ToString();
        }
        public static String ConvertToExtensions(List<String> list, String nameSpace)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using pacvue.walmartICSams.Common.ORM;");
            sb.Append("namespace ").AppendLine(nameSpace).AppendLine("{");
            sb.Append("public static class WakmartCKManagerExtensions ").AppendLine("{");
            sb.Append("public static void AddCKManager(this IServiceCollection services) ").AppendLine("{");
            foreach (var item in list)
            {
                sb.Append("services.AddTransient<").Append("I").Append(CapitalizeFirstLetter(item).Substring(0, item.Length - 4)).Append("Repository").Append(",").Append(CapitalizeFirstLetter(item)).Append("CKManage").AppendLine(">();");
                sb.Append("services.AddTransient<").Append("CKBaseRepository").Append("<").Append(CapitalizeFirstLetter(item)).Append("Model").Append(">,").Append(CapitalizeFirstLetter(item)).Append("CKManage").AppendLine(">();");
            }
            sb.AppendLine("}").AppendLine("}").AppendLine("}");
            return sb.ToString();
        }
        public static String ConvertToPKIndex(Dictionary<String, List<String>> dic, String nameSpace)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            sb.AppendLine("using System.Text;");
            sb.Append("namespace ").AppendLine(nameSpace).AppendLine("{");
            sb.Append("public static class CKTablePKIndexCollection ").AppendLine("{");
            sb.Append("public static List<String> GetPKIndex(String tableName) ").AppendLine("{");
            sb.AppendLine("if (AllPKIndex.ContainsKey(tableName))").AppendLine("{");
            sb.AppendLine("return AllPKIndex[tableName];");
            sb.AppendLine("}");
            sb.AppendLine("return new List<string>();");
            sb.AppendLine("}");
            sb.Append("public readonly static Dictionary<String, List<String>> AllPKIndex = new Dictionary<string, List<string>>()").AppendLine("{");
            bool second = false;
            foreach (var item in dic)
            {
                if (second)
                {
                    sb.AppendLine(",");
                }
                second = true;
                sb.Append("{\"").Append(CapitalizeFirstLetter(item.Key)).Append("\",  new List<string>(){ ");
                sb.Append(String.Join(",", item.Value.Select(i => $"\"{i}\"")));
                sb.Append("} }");
            }
            sb.AppendLine("};").AppendLine("}").AppendLine("}");
            return sb.ToString();
        }

        public static String ConverToModel(String tableName, IEnumerable<DescriptionTable> list, String nameSpace)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using pacvue.walmartICSams.Common.ORM;");
            sb.Append("namespace ").AppendLine(nameSpace).AppendLine("{");
            sb.Append("public class ").Append(CapitalizeFirstLetter(tableName)).AppendLine("Model").AppendLine("{");
            foreach (var item in list)
            {
                String columenType = ConvertToType(item.Type, out String cKColumnName);
                if (!String.IsNullOrEmpty(item.Comment))
                {
                    sb.AppendLine(" /// <summary> ").Append(" /// ").AppendLine(item.Comment).AppendLine(" /// </summary> ");
                }
                sb.Append("public ").Append(columenType).Append(" ").Append(CapitalizeFirstLetter(item.Name)).AppendLine(" { get; set; }");
            }
            sb.AppendLine("}").AppendLine("}");
            return sb.ToString();
        }
        public static String ConvertToContext(IEnumerable<String> list, String nameSpace)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using pacvue.walmartICSams.Common.ORM;");
            sb.Append("namespace ").AppendLine(nameSpace).AppendLine("{");
            sb.Append("public class ").AppendLine("WalmartCKContext : IDisposable").AppendLine("{");
            foreach (var item in list)
            {
                sb.Append("public CKDBSet<").Append(CapitalizeFirstLetter(item)).Append("Model> ").Append(CapitalizeFirstLetter(item).Substring(0, item.Length - 4)).AppendLine(" { get; }");
            }
            sb.AppendLine("public void Dispose(){}");
            sb.AppendLine("}").AppendLine("}");
            return sb.ToString();
        }
        public static IEnumerable<String> QueryContext()
        {
            //Type type = typeof(WalmartCKContext);
            //if (type != null)
            //{
            //    var properties = type.GetProperties();
            //    return properties.Select(x => x.Name);
            //}
            return null;
        }
        public static string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return char.ToUpper(input[0]) + input.Substring(1);
        }

        private static String ConvertToType(String type, out String attribute)
        {
            switch (type)
            {

                case "Int8":
                    attribute = "CKColumnTypeString.Int8";
                    return "bool";
                case "UInt8":
                    attribute = "CKColumnTypeString.UInt8";
                    return "byte";
                case "Int16":
                    attribute = "CKColumnTypeString.Int16";
                    return "short";
                case "Int32":
                    attribute = "CKColumnTypeString.Int32";
                    return "int";
                case "Int64":
                    attribute = "CKColumnTypeString.Int64";
                    return "long";
                case "Float16":
                    attribute = "CKColumnTypeString.Float16";
                    return "float";
                case "Float32":
                    attribute = "CKColumnTypeString.Float32";
                    return "float";
                case "Float64":
                    attribute = "CKColumnTypeString.Float64";
                    return "double";
                case "String":
                    attribute = "CKColumnTypeString.String";
                    return "string";
                case "Decimal(18, 2)":
                    attribute = "CKColumnTypeString.Decimal_18_2";
                    return "decimal";
                case "Double":
                    attribute = "CKColumnTypeString.Double";
                    return "double";
                case "Boolean":
                    attribute = "CKColumnTypeString.Boolean";
                    return "bool";
                case "Date":
                    attribute = "CKColumnTypeString.Date";
                    return "DateTime";
                case "DateTime":
                    attribute = "CKColumnTypeString.DateTime";
                    return "DateTime";
                case "DateTime64(3)":
                    attribute = "CKColumnTypeString.DateTime64";
                    return "DateTime";
                case "Nullable(String)":
                    attribute = "CKColumnTypeString.String";
                    return "string";
                case "Nullable(Bool)":
                    attribute = "CKColumnTypeString.Bool";
                    return "bool?";
                case "Bool":
                    attribute = "CKColumnTypeString.Bool";
                    return "bool";
                case "Nullable(UInt8)":
                    attribute = "CKColumnTypeString.UInt8";
                    return "byte?";
                case "Nullable(Int8)":
                    attribute = "CKColumnTypeString.Int8";
                    return "bool?";
                case "Nullable(Int16)":
                    attribute = "CKColumnTypeString.Int16";
                    return "short?";
                case "Nullable(Int32)":
                    attribute = "CKColumnTypeString.Int32";
                    return "int?";
                case "Nullable(Int64)":
                    attribute = "CKColumnTypeString.Int64";
                    return "long?";
                case "Nullable(Float16)":
                    attribute = "CKColumnTypeString.Float16";
                    return "float?";
                case "Nullable(Float32)":
                    attribute = "CKColumnTypeString.Float32";
                    return "float?";
                case "Nullable(Float64)":
                    attribute = "CKColumnTypeString.Float64";
                    return "double?";
                case "Nullable(Date)":
                    attribute = "CKColumnTypeString.Date";
                    return "DateTime?";
                case "Nullable(DateTime)":
                    attribute = "CKColumnTypeString.DateTime";
                    return "DateTime?";
                case "Nullable(DateTime64(3))":
                    attribute = "CKColumnTypeString.DateTime64";
                    return "DateTime?";
                case "Nullable(Decimal(18, 2))":
                    attribute = "CKColumnTypeString.Decimal_18_2";
                    return "decimal?";
                default:
                    if (type.StartsWith("Nullable(Decimal"))
                    {
                        attribute = "CKColumnTypeString.Decimal";
                        return "decimal?";
                    }
                    if (type.StartsWith("Decimal"))
                    {
                        attribute = "CKColumnTypeString.Decimal";
                        return "decimal";
                    }
                    String result = type.Replace("Map", "Dictionary").Replace("(", "<").Replace(")", ">").Replace("Array", "List");
                    attribute = $"\"{type}\"";
                    return result;
            }
        }

    }
}
