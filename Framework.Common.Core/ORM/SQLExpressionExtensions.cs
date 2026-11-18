using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
namespace Framework.Common.Core.ORM
{
    internal static class SQLExpressionExtensions
    {
        public static void ConvertExpression(this Expression expression,        
            in ParameterWrapper reduce)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Lambda:
                    ConvertExpression(((LambdaExpression)expression).Body, in reduce);
                    break;
                case ExpressionType.AndAlso:
                    ConvertExpression(((BinaryExpression)expression).Left, in reduce);
                    reduce.AddOperator(DBFiledOperatorEnum.And);
                    ConvertExpression(((BinaryExpression)expression).Right, in reduce);
                    break;
                case ExpressionType.OrElse:
                    ConvertExpression(((BinaryExpression)expression).Left, in reduce);
                    reduce.AddOperator(DBFiledOperatorEnum.Or);
                    ConvertExpression(((BinaryExpression)expression).Right, in reduce);
                    break;
                case ExpressionType.Equal:
                    ConvertExpression(((BinaryExpression)expression).Left, in reduce);
                    reduce.AddOperator(DBFiledOperatorEnum.Equal);
                    ConvertExpression(((BinaryExpression)expression).Right, in reduce);
                    break;
                case ExpressionType.NotEqual:
                    ConvertExpression(((BinaryExpression)expression).Left, in reduce);
                    reduce.AddOperator(DBFiledOperatorEnum.NotEqual);
                    ConvertExpression(((BinaryExpression)expression).Right, in reduce);
                    break;
                case ExpressionType.LessThanOrEqual:
                    ConvertExpression(((BinaryExpression)expression).Left, in reduce);
                    reduce.AddOperator(DBFiledOperatorEnum.LessThanOrEqual);
                    ConvertExpression(((BinaryExpression)expression).Right, in reduce);
                    break;
                case ExpressionType.LessThan:
                    ConvertExpression(((BinaryExpression)expression).Left, in reduce);
                    reduce.AddOperator(DBFiledOperatorEnum.LessThan);

                    ConvertExpression(((BinaryExpression)expression).Right, in reduce);
                    break;
                case ExpressionType.GreaterThan:
                    ConvertExpression(((BinaryExpression)expression).Left, in reduce);
                    reduce.AddOperator(DBFiledOperatorEnum.GreaterThan);
                    ConvertExpression(((BinaryExpression)expression).Right, in reduce);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    ConvertExpression(((BinaryExpression)expression).Left, in reduce);
                    reduce.AddOperator(DBFiledOperatorEnum.GreaterThanOrEqual);
                    ConvertExpression(((BinaryExpression)expression).Right, in reduce);
                    break;
                case ExpressionType.Constant:
                    ConstantExpression((ConstantExpression)expression, in reduce);
                    break;
                case ExpressionType.MemberAccess:
                    MemberExpression((MemberExpression)expression, in reduce);
                    break;
                case ExpressionType.New:
                    NewExpression((NewExpression)expression, in reduce);
                    break;
                case ExpressionType.Convert:
                    ConvertExpression(((UnaryExpression)expression).Operand, in reduce);
                    break;
                case ExpressionType.Call:
                    //待实现List.contains 和 String.equals 
                    //ConvertExpression(((UnaryExpression)expression).Operand, in reduce);
                    MethodCallExpression call = (MethodCallExpression)expression;
                    VisitMethodCall(call, in reduce);
                    break;
                case ExpressionType.MemberInit:
                    MemberInitExpression((MemberInitExpression)expression, in reduce);
                    break;
                case ExpressionType.Not:
                    UnaryExpression((UnaryExpression)expression, in reduce);
                    break;
                default:
                    throw new ArgumentException("请传递正确sql参数");
            }
        }
        private static void UnaryExpression(UnaryExpression node, in ParameterWrapper reduce)
        {
            var memberExpression = node.Operand as MemberExpression;
            var member = memberExpression.Member as MemberInfo;
            MemberExpression(node.Operand as MemberExpression,
                in reduce);
            reduce.AddOperator(DBFiledOperatorEnum.Equal);
            if (node.NodeType == ExpressionType.Not)
            {
                reduce.Add(member.Name, 0);
            }
            if (node.NodeType == ExpressionType.IsTrue)
            {
                reduce.Add(member.Name, 1);
            }
        }
        private static void VisitMethodCall(MethodCallExpression node, in ParameterWrapper reduce)
        {

            // x.ItemId.Contains("x");
            if (node.Method.Name == "Contains" && node.Method.DeclaringType == typeof(String))
            {
                var value = (MemberExpression)node.Object;
                MemberExpression(value, in reduce);
                reduce.AddOperator(DBFiledOperatorEnum.LIKE);
                // 处理 IEnumerable<T>.Contains(value) 方法调用
                var list = (ConstantExpression)node.Arguments[0]; // 列表表达式
                var result1 = Expression.Lambda(list).Compile().DynamicInvoke();
                reduce.Add(list.RandromName(), $"%{result1}%");
                return;
            }
            if (node.Method.Name == "Contains" && node.Method.DeclaringType.IsAssignableTo(typeof(IEnumerable)))
            {
                // 处理 IEnumerable<T>.Contains(value) 方法调用
                var list = (MemberExpression)node.Arguments[0]; // 列表表达式
                MemberExpression(list, in reduce);
                reduce.AddOperator(DBFiledOperatorEnum.In);
                var value = node.Object as MemberExpression;
                var result1 = Expression.Lambda(value).Compile().DynamicInvoke();
                reduce.Add(value.Member.Name, ConvertToArray(result1));
                return;
            }
            if (node.Method.Name == nameof(String.Equals) && node.Method.DeclaringType == typeof(String))
            {
                var list = node.Arguments[0];
                if (list is ConstantExpression)
                {
                    ConstantExpression((ConstantExpression)list, reduce);
                }
                else
                {
                    MemberExpression((MemberExpression)list, in reduce);
                }
                reduce.AddOperator(DBFiledOperatorEnum.Equal);
                list = node.Arguments[1];
                if (list is ConstantExpression)
                {
                    ConstantExpression((ConstantExpression)list, reduce);
                }
                else
                {
                    MemberExpression((MemberExpression)list, in reduce);
                }
                return;
            }
        }
        private static Object ConvertToArray(Object list)
        {
            var name = list.GetType().GenericTypeArguments[0].Name;
            switch (name)
            {
                case ColumnTypeString.Int8:
                    return (list as List<sbyte>).ToArray();
                case ColumnTypeString.Int16:
                    return (list as List<Int16>).ToArray();
                case ColumnTypeString.Int32:
                    return (list as List<Int32>).ToArray();
                case ColumnTypeString.Int64:
                    return (list as List<Int64>).ToArray();
                case ColumnTypeString.UInt8:
                    return (list as List<byte>).ToArray();
                case ColumnTypeString.Bool:
                    return (list as List<bool>).ToArray();
                case ColumnTypeString.DateTime:
                    return (list as List<DateTime>).ToArray();
                case ColumnTypeString.Float64:
                    return (list as List<double>).ToArray();
                case ColumnTypeString.Decimal:
                    return (list as List<decimal>).ToArray();
                case ColumnTypeString.String:
                    return (list as List<String>).ToArray();
                case ColumnTypeString.Float16:
                    return (list as List<Single>).ToArray();
                case ColumnTypeString.Float32:
                    return (list as List<Single>).ToArray();
            }
            return null;
        }
        private static void ConstantExpression(ConstantExpression constant, in ParameterWrapper reduce)
        {
            String name = constant.RandromName();
            reduce.Add(name, constant.Value);
        }
        private static void NewExpression(NewExpression expression, in ParameterWrapper reduce)
        {
            bool isConstant = false;
            int len = expression.Arguments.Count;
            for (int i = 0; i < len; i++)
            {
                Expression item = expression.Arguments[i];
                MemberExpression memberExp;
                if (item is ConstantExpression)
                {
                    isConstant = true;
                    ConstantExpression((ConstantExpression)item, reduce);
                    continue;
                }
                MemberInfo asName = expression.Members[i];
                memberExp = (MemberExpression)item;
                var member = memberExp.Member;
                Type type = member.ReflectedType;
                ColumnRelevanceMapper column = new ColumnRelevanceMapper();
                column.TableName = type;
                column.ColumnName = member.Name;
                column.ASName = asName.Name;
                reduce.Add(column);
            }
            if (isConstant)
            {
                foreach (var item in expression.Members)
                {
                    ColumnRelevanceMapper column = new ColumnRelevanceMapper();
                    column.ColumnName = item.Name;
                    column.TableName = item.ReflectedType;
                    column.ASName = item.Name;
                    reduce.Add(column);
                }
            }

        }
        private static void MemberInitExpression(MemberInitExpression expression, in ParameterWrapper reduce)
        {
            foreach (var binding in expression.Bindings)
            {
                if (binding is MemberAssignment assignment)
                {
                    var memberName = assignment.Member.Name;
                    var memberValue = Expression.Lambda(assignment.Expression).Compile().DynamicInvoke();
                    ColumnRelevanceMapper column = new ColumnRelevanceMapper();
                    column.ColumnName = memberName;
                    column.ASName = memberName;
                    column.TableName = assignment.Member.DeclaringType;
                    reduce.Add(column);
                    // 假设 reduce.Add 方法可以接受列名和值
                    reduce.Add(memberName, memberValue);
                }
            }
        }
        /// <summary>
        /// AdItem_allModel allModel = new AdItem_allModel()
        ///{
        ///    AdGroupId = 2350229
        ///};
        ///    Expression<Func<AdItem_allModel, bool>> expression = x => x.AdGroupId == allModel.AdGroupId;
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="reduce"></param>
        /// <exception cref="ArgumentException"></exception>
        private static void MemberExpression(MemberExpression expression,
                in ParameterWrapper reduce)
        {
            var member = expression.Member as MemberInfo;
            Type type = member.ReflectedType;
            //如 int32 i =0;
            //a.id=i 那么会解析出 @i result = 1
            if (
                expression.Expression.NodeType == ExpressionType.MemberAccess ||
                expression.Expression.NodeType == ExpressionType.Constant
                )
            {
                var result1 = Expression.Lambda(expression).Compile().DynamicInvoke();
                reduce.Add(member.Name, result1);
                return;
            }
            switch (member.MemberType)
            {
                case MemberTypes.Property:
                    ColumnRelevanceMapper column = new ColumnRelevanceMapper();
                    column.ColumnName = member.Name;
                    column.ASName = member.Name;
                    column.TableName = type;
                    reduce.Add(column);
                    break;
                case MemberTypes.Field:
                    throw new ArgumentException("请传递正确sql参数,注:只允许属性注入,不允许字段注入");
            }
        }
    }
}
