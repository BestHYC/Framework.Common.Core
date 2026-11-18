using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Framework.Common.Core.ORM
{
    internal static class SqlOperatorExtention
    {
        public static String Top = "TOP";
        public static String Select = " SELECT ";
        /// <summary>
        /// 左括号
        /// </summary>
        public static String LeftBrace = " ( ";
        /// <summary>0
        /// 右括号
        /// </summary>
        public static String RightBrace = " ) ";
        /// <summary>
        /// 等于
        /// </summary>
        public static String Equal = " = ";
        /// <summary>
        /// 不等于
        /// </summary>
        public static String NotEqual = " != ";
        /// <summary>
        /// 为空
        /// </summary>
        public static String EqualNull = " IS NULL ";
        /// <summary>
        /// 不为空
        /// </summary>
        public static String EqualNotNull = " IS NOT NULL ";
        /// <summary>
        /// 与
        /// </summary>
        public static String And = " AND ";
        /// <summary>
        /// 或
        /// </summary>
        public static String Or = " OR ";
        /// <summary>
        /// in
        /// </summary>
        public static String In = " IN ";
        /// <summary>
        /// 
        /// </summary>
        public static String Having = " HAVING ";
        public static String GreaterThan = " >= ";
        public static String Greater = " > ";
        public static String LessThan = " <= ";
        public static String Less = " < ";
        public static String InnerJoin = " INNER JOIN ";
        public static String LeftJoin = " LEFT JOIN ";
        public static String RightJoin = " RIGHT JOIN ";
        public static String On = " ON ";
        public static String OrderBy = " ORDER BY ";
        public static String OrderByDesc = " DESC ";
        public static String Where = " WHERE ";
        public static String Insert = " INSERT INTO ";
        public static String Update = " UPDATE ";
        public static String Delete = " DELETE FROM ";
        public static String Like = " LIKE ";
        public static String Count = " COUNT(1) ";
        public static String GetOperator(this DBFiledOperatorEnum sql)
        {
            switch (sql)
            {
                case DBFiledOperatorEnum.And:
                    return And;
                case DBFiledOperatorEnum.LIKE:
                    return Like;
                case DBFiledOperatorEnum.Equal:
                    return Equal;
                case DBFiledOperatorEnum.EqualNotNull:
                    return EqualNotNull;
                case DBFiledOperatorEnum.EqualNull:
                    return EqualNull;
                case DBFiledOperatorEnum.Having:
                    return Having;
                case DBFiledOperatorEnum.In:
                    return In;
                case DBFiledOperatorEnum.LeftBrace:
                    return LeftBrace;
                case DBFiledOperatorEnum.NotEqual:
                    return NotEqual;
                case DBFiledOperatorEnum.On:
                    return On;
                case DBFiledOperatorEnum.Or:
                    return Or;
                case DBFiledOperatorEnum.OrderBy:
                    return OrderBy;
                case DBFiledOperatorEnum.OrderByDesc:
                    return OrderByDesc;
                case DBFiledOperatorEnum.RightBrace:
                    return RightBrace;
                case DBFiledOperatorEnum.Select:
                    return Select;
                case DBFiledOperatorEnum.Where:
                    return Where;
                case DBFiledOperatorEnum.LessThan:
                    return Less;
                case DBFiledOperatorEnum.LessThanOrEqual:
                    return LessThan;
                case DBFiledOperatorEnum.GreaterThan:
                    return Greater;
                case DBFiledOperatorEnum.GreaterThanOrEqual:
                    return GreaterThan;
                case DBFiledOperatorEnum.Top:
                    return Top;
                case DBFiledOperatorEnum.Count:
                    return Count;
                default:
                    return "";
            }

        }
        public static String GetOperator(this DBTableOperatorEnum table)
        {
            switch (table)
            {
                case DBTableOperatorEnum.InnerJoin:
                    return InnerJoin;
                case DBTableOperatorEnum.LeftJoin:
                    return LeftJoin;
                case DBTableOperatorEnum.RightJoin:
                    return RightJoin;
                case DBTableOperatorEnum.Insert:
                    return Insert;
                case DBTableOperatorEnum.Update:
                    return Update;
                case DBTableOperatorEnum.Delete:
                    return Delete;
                default: return "";
            }
        }
        /// <summary>
        /// 生成 7 位随机别名，字符来自大写和小写字母。
        /// </summary>
        private const String Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        public static String RandromName(this Object obj)
        {
            const Int32 length = 7;
            Span<Char> buffer = stackalloc Char[length];
            for (Int32 i = 0; i < length; i++)
            {
                buffer[i] = Letters[Random.Shared.Next(Letters.Length)];
            }

            return new String(buffer);
        }
    }
}
