using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Framework.Common.Core
{
    /// <summary>
    /// 对象映射：把源对象的公共属性逐个赋给目标对象，适用于 AModel ↔ AEntity 这类扁平转换。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 配对规则：默认按属性名（忽略大小写）配对；名字不一致时用 <see cref="MapNameAttribute"/> 指定对方的属性名。
    /// 源属性上标注表示"写到目标的哪个属性"，目标属性上标注表示"从源的哪个属性读"，两边都行，标一处即可。
    /// 同一属性可标多条：先找 <see cref="MapNameAttribute.Type"/> 等于对面类型的那条，没有则用 Type 为 null 的那条，再没有则用属性名本身。
    /// 例如源为 [MapName(typeof(AEntity), "Nameyyy")] public string Namexx，映射到 AEntity 时写入 Nameyyy。
    /// 配不上的属性直接跳过，目标上保留原值。
    /// </para>
    /// <para>
    /// 类型不同也会尽量互转，规则如下：
    /// </para>
    /// <para>
    /// int 与 long 可以互转，如 int id = 1444 与 long id = 555。
    /// </para>
    /// <para>
    /// int 与 string 可以互转，如 int age = 20 与 string age = "20"。
    /// </para>
    /// <para>
    /// 数值 0、1（含字符串 "0"/"1"）与 bool 可以互转，如 int flag = 1 与 bool flag = true；其它数字会抛异常。
    /// </para>
    /// <para>
    /// long 时间戳与 DateTime 可以互转，如 long time = 1577836800000 与 DateTime time = 2020-01-01。
    /// 绝对值小于 1e11 按秒，否则按毫秒；写出一律毫秒、本地时间。
    /// </para>
    /// <para>
    /// 字符串与枚举可以互转，如 string status = "Active" 与 enum Status { Active }。
    /// </para>
    /// <para>
    /// 两端类型无法转换的属性会被跳过，如 DateTime 与 bool；非法字符串（如 "abc" → int）会抛异常。
    /// </para>
    /// <para>
    /// 属性值按引用/原值直接赋过去，不递归映射嵌套对象，也不复制集合元素，即目标与源会共享同一个子对象实例。
    /// </para>
    /// <para>
    /// 性能：首次遇到某个 源类型 + 目标类型 组合时用表达式树编译出赋值委托并缓存，之后调用等同于手写赋值，反射只发生在首次。
    /// </para>
    /// </remarks>
    public static class Mapper
    {
        private static readonly ConcurrentDictionary<Type, Delegate> CreateCache = new();
        private static readonly ConcurrentDictionary<Type, Delegate> UpdateCache = new();

        private static readonly MethodInfo ChangeValueMethod =
            typeof(Mapper).GetMethod(nameof(ChangeValue), BindingFlags.NonPublic | BindingFlags.Static)!;

        /// <summary>
        /// 新建一个 <typeparamref name="TDestination"/> 并填入源属性。<paramref name="source"/> 为 null 时返回 null。
        /// </summary>
        public static TDestination MapTo<TDestination>(this object? source)
            where TDestination : class, new()
        {
            if (source is null)
            {
                return null!;
            }

            var key = typeof(CacheKey<,>).MakeGenericType(source.GetType(), typeof(TDestination));
            var map = (Func<object, TDestination>)CreateCache.GetOrAdd(key, static t => BuildCreate(t));
            return map(source);
        }

        /// <summary>
        /// 把源属性写入已有的 <paramref name="destination"/> 并返回它。
        /// 配对上的属性一律覆盖（源为 null 也会覆盖成 null），任一参数为 null 则原样返回。
        /// </summary>
        public static TDestination MapTo<TDestination>(this object? source, TDestination destination)
            where TDestination : class
        {
            if (source is null || destination is null)
            {
                return destination!;
            }

            var key = typeof(CacheKey<,>).MakeGenericType(source.GetType(), typeof(TDestination));
            var map = (Action<object, TDestination>)UpdateCache.GetOrAdd(key, static t => BuildUpdate(t));
            map(source, destination);
            return destination;
        }

        /// <summary>
        /// 把集合中每个元素新建为 <typeparamref name="TDestination"/>。
        /// <paramref name="sources"/> 为 null 时返回空列表；元素为 null 时对应位置为 null。
        /// </summary>
        public static List<TDestination> MapTo<TDestination>(this IEnumerable? sources)
            where TDestination : class, new()
        {
            if (sources is null)
            {
                return new List<TDestination>();
            }

            var list = sources is ICollection collection
                ? new List<TDestination>(collection.Count)
                : new List<TDestination>();
            foreach (var item in sources)
            {
                list.Add(item.MapTo<TDestination>());
            }

            return list;
        }

        /// <summary>
        /// 按位置把源集合写入已有目标集合并返回目标。只更新两者重叠的前 N 项，多出来的一侧不动。
        /// 调用时请写出目标类型，如 models.MapTo&lt;AEntity&gt;(entities)，以免和单条 <see cref="MapTo{TDestination}(object, TDestination)"/> 混淆。
        /// 任一参数为 null 则原样返回 <paramref name="destinations"/>。
        /// </summary>
        public static IList<TDestination> MapTo<TDestination>(this IEnumerable? sources, IList<TDestination> destinations)
            where TDestination : class
        {
            if (sources is null || destinations is null)
            {
                return destinations!;
            }

            var index = 0;
            foreach (var item in sources)
            {
                if (index >= destinations.Count)
                {
                    break;
                }

                item.MapTo(destinations[index]);
                index++;
            }

            return destinations;
        }

        /// <summary>
        /// 按 <paramref name="key"/> 把源集合写入已有目标集合并返回目标。源上的键属性按与目标相同的配对规则解析（含 <see cref="MapNameAttribute"/>）。
        /// 对得上的覆盖目标，对不上的源跳过、目标保留原值。键为 null 或重复时：空键跳过，目标重复键以先出现的为准。
        /// 调用示例：models.MapTo(entities, x => x.Id)。
        /// </summary>
        public static IList<TDestination> MapTo<TDestination, TKey>(
            this IEnumerable? sources,
            IList<TDestination> destinations,
            Expression<Func<TDestination, TKey>> key)
            where TDestination : class
            where TKey : notnull
        {
            if (sources is null || destinations is null)
            {
                return destinations!;
            }

            var destKeyProp = GetKeyProperty(key);
            var getDestKey = key.Compile();
            var destByKey = new Dictionary<TKey, TDestination>();
            foreach (var dest in destinations)
            {
                if (dest is null)
                {
                    continue;
                }

                var destKey = getDestKey(dest);
                if (destKey is not null)
                {
                    destByKey.TryAdd(destKey, dest);
                }
            }

            foreach (var item in sources)
            {
                if (item is null)
                {
                    continue;
                }

                var sourceKeyProp = FindSourceKeyProperty(item.GetType(), typeof(TDestination), destKeyProp.Name);
                if (sourceKeyProp is null)
                {
                    continue;
                }

                var sourceKey = sourceKeyProp.GetValue(item);
                if (sourceKey is TKey matched && destByKey.TryGetValue(matched, out var dest))
                {
                    item.MapTo(dest);
                }
            }

            return destinations;
        }

        private static PropertyInfo GetKeyProperty(LambdaExpression key)
        {
            var body = key.Body;
            if (body is UnaryExpression convert && convert.NodeType == ExpressionType.Convert)
            {
                body = convert.Operand;
            }

            if (body is MemberExpression member && member.Member is PropertyInfo property)
            {
                return property;
            }

            throw new ArgumentException("Key 必须是属性访问，例如 x => x.Id。");
        }

        private static PropertyInfo? FindSourceKeyProperty(Type sourceType, Type destType, string destKeyName)
        {
            foreach (var prop in sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanRead || prop.GetIndexParameters().Length != 0)
                {
                    continue;
                }

                if (ResolveMappedName(prop, destType).Equals(destKeyName, StringComparison.OrdinalIgnoreCase))
                {
                    return prop;
                }
            }

            var destKeyProp = destType.GetProperty(destKeyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (destKeyProp is null)
            {
                return null;
            }

            var sourceName = ResolveMappedName(destKeyProp, sourceType);
            return sourceType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p => p.CanRead && p.GetIndexParameters().Length == 0
                    && p.Name.Equals(sourceName, StringComparison.OrdinalIgnoreCase));
        }

        private static Delegate BuildCreate(Type keyType)
        {
            var args = keyType.GetGenericArguments();
            var sourceType = args[0];
            var destType = args[1];
            var sourceObj = Expression.Parameter(typeof(object), "source");
            var source = Expression.Variable(sourceType, "src");
            var dest = Expression.Variable(destType, "dest");

            var body = new List<Expression>
            {
                Expression.Assign(source, Expression.Convert(sourceObj, sourceType)),
                Expression.Assign(dest, Expression.New(destType))
            };
            body.AddRange(BuildAssigns(source, dest, sourceType, destType));
            body.Add(dest);

            var block = Expression.Block(new[] { source, dest }, body);
            return Expression.Lambda(typeof(Func<,>).MakeGenericType(typeof(object), destType), block, sourceObj).Compile();
        }

        private static Delegate BuildUpdate(Type keyType)
        {
            var args = keyType.GetGenericArguments();
            var sourceType = args[0];
            var destType = args[1];
            var sourceObj = Expression.Parameter(typeof(object), "source");
            var dest = Expression.Parameter(destType, "dest");
            var source = Expression.Variable(sourceType, "src");

            var body = new List<Expression>
            {
                Expression.Assign(source, Expression.Convert(sourceObj, sourceType))
            };
            body.AddRange(BuildAssigns(source, dest, sourceType, destType));

            var block = Expression.Block(new[] { source }, body);
            return Expression.Lambda(typeof(Action<,>).MakeGenericType(typeof(object), destType), block, sourceObj, dest).Compile();
        }

        private static IEnumerable<Expression> BuildAssigns(
            ParameterExpression source,
            ParameterExpression dest,
            Type sourceType,
            Type destType)
        {
            foreach (var (srcProp, destProp) in MatchProperties(sourceType, destType))
            {
                var converted = BuildConversion(
                    Expression.Property(source, srcProp),
                    srcProp.PropertyType,
                    destProp.PropertyType);
                if (converted is null)
                {
                    continue;
                }

                yield return Expression.Assign(Expression.Property(dest, destProp), converted);
            }
        }

        /// <summary>
        /// 生成把 <paramref name="from"/> 类型的值转成 <paramref name="to"/> 的表达式，转不了返回 null（该属性跳过）。
        /// 依次尝试：直接赋值 → CLR 强转（数值互转）→ 运行时解析（字符串、0/1↔bool、时间戳↔DateTime）。
        /// </summary>
        private static Expression? BuildConversion(Expression value, Type from, Type to)
        {
            if (to.IsAssignableFrom(from))
            {
                return value;
            }

            // int? → int 这类：先 GetValueOrDefault，否则源为 null 时 Convert 会抛 "Nullable object must have a value"
            var fromCore = Nullable.GetUnderlyingType(from);
            if (fromCore is not null && to.IsValueType && Nullable.GetUnderlyingType(to) is null)
            {
                value = Expression.Call(value, from.GetMethod(nameof(Nullable<int>.GetValueOrDefault), Type.EmptyTypes)!);
                from = fromCore;
                if (to.IsAssignableFrom(from))
                {
                    return value;
                }
            }

            // 数值、枚举、char 之间由 CLR 直接转，编译后等同手写强转
            if (IsCastable(from) && IsCastable(to))
            {
                return Expression.Convert(value, to);
            }

            // 字符串与基础类型互转需要解析/格式化，交给运行时方法
            if (CanChangeValue(from, to))
            {
                var call = Expression.Call(ChangeValueMethod, Expression.Convert(value, typeof(object)), Expression.Constant(to));
                return Expression.Convert(call, to);
            }

            return null;
        }

        /// <summary>数值、枚举、char 这类能被 CLR 直接强转的类型（Nullable&lt;T&gt; 按 T 判断）。</summary>
        private static bool IsCastable(Type type)
        {
            var core = Nullable.GetUnderlyingType(type) ?? type;
            return core.IsEnum
                || core == typeof(decimal)
                || (core.IsPrimitive && core != typeof(bool));
        }

        /// <summary>
        /// 需要走运行时解析的组合：字符串互转、整数 0/1 与 bool、long/int 时间戳与 DateTime。
        /// </summary>
        private static bool CanChangeValue(Type from, Type to)
        {
            var fromCore = Nullable.GetUnderlyingType(from) ?? from;
            var toCore = Nullable.GetUnderlyingType(to) ?? to;
            if (toCore == typeof(string) || fromCore == typeof(string))
            {
                return toCore == typeof(string)
                    || toCore.IsEnum
                    || toCore == typeof(Guid)
                    || toCore == typeof(bool)
                    || typeof(IConvertible).IsAssignableFrom(toCore);
            }

            if (toCore == typeof(bool) && IsInteger(fromCore))
            {
                return true;
            }

            if (fromCore == typeof(bool) && IsInteger(toCore))
            {
                return true;
            }

            if (toCore == typeof(DateTime) && IsInteger(fromCore))
            {
                return true;
            }

            return fromCore == typeof(DateTime) && toCore == typeof(long);
        }

        private static bool IsInteger(Type type)
        {
            return type == typeof(int)
                || type == typeof(long)
                || type == typeof(short)
                || type == typeof(byte)
                || type == typeof(sbyte)
                || type == typeof(uint)
                || type == typeof(ushort)
                || type == typeof(ulong);
        }

        /// <summary>
        /// 运行时转换。空值/空串返回目标默认值；0/1 以外的数字转 bool、非法字符串会抛异常。
        /// </summary>
        private static object? ChangeValue(object? value, Type to)
        {
            var toCore = Nullable.GetUnderlyingType(to) ?? to;
            if (value is null)
            {
                return to.IsValueType && Nullable.GetUnderlyingType(to) is null ? Activator.CreateInstance(to) : null;
            }

            if (toCore == typeof(bool))
            {
                return ToBool(value);
            }

            if (value is bool flag && IsInteger(toCore))
            {
                return Convert.ChangeType(flag ? 1 : 0, toCore, CultureInfo.InvariantCulture);
            }

            if (toCore == typeof(DateTime))
            {
                return ToDateTime(value);
            }

            if (value is DateTime time && toCore == typeof(long))
            {
                return ToUnixMilliseconds(time);
            }

            var text = value is string s ? s : Convert.ToString(value, CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(text))
            {
                return to.IsValueType && Nullable.GetUnderlyingType(to) is null ? Activator.CreateInstance(to) : null;
            }

            if (toCore == typeof(string))
            {
                return text;
            }

            if (toCore.IsEnum)
            {
                return Enum.Parse(toCore, text, true);
            }

            if (toCore == typeof(Guid))
            {
                return Guid.Parse(text);
            }

            return Convert.ChangeType(text, toCore, CultureInfo.InvariantCulture);
        }

        /// <summary>仅接受 0/1（含字符串 "0"/"1"）；其它数字抛异常。</summary>
        private static bool ToBool(object value)
        {
            if (value is bool flag)
            {
                return flag;
            }

            if (value is string text)
            {
                text = text.Trim();
                if (text == "0")
                {
                    return false;
                }

                if (text == "1")
                {
                    return true;
                }

                throw new FormatException($"无法将 \"{text}\" 转为 bool，仅支持 0 和 1。");
            }

            var number = Convert.ToInt64(value, CultureInfo.InvariantCulture);
            return number switch
            {
                0 => false,
                1 => true,
                _ => throw new InvalidCastException($"无法将数值 {number} 转为 bool，仅支持 0 和 1。")
            };
        }

        private static DateTime ToDateTime(object value)
        {
            if (value is DateTime time)
            {
                return time;
            }

            if (value is string text && long.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            {
                return FromUnixTimestamp(parsed);
            }

            return FromUnixTimestamp(Convert.ToInt64(value, CultureInfo.InvariantCulture));
        }

        /// <summary>与 <see cref="JsonExtension"/> 相同：绝对值小于 1e11 按秒，否则按毫秒，结果为本地时间。</summary>
        private static DateTime FromUnixTimestamp(long timestamp)
        {
            return timestamp is > -100_000_000_000 and < 100_000_000_000
                ? DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime
                : DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime;
        }

        private static long ToUnixMilliseconds(DateTime value)
        {
            var kind = value.Kind == DateTimeKind.Unspecified ? DateTimeKind.Local : value.Kind;
            return new DateTimeOffset(DateTime.SpecifyKind(value, kind)).ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// 找出要赋值的属性对：先按源属性找目标（源上的 <see cref="MapNameAttribute"/> 按对面类型解析），
        /// 再补上目标属性通过 <see cref="MapNameAttribute"/> 反向指定的源属性。
        /// </summary>
        private static List<(PropertyInfo Src, PropertyInfo Dest)> MatchProperties(Type sourceType, Type destType)
        {
            var sources = sourceType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .ToList();

            var dests = destType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0)
                .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

            var pairs = new List<(PropertyInfo, PropertyInfo)>();
            var mappedDest = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var src in sources)
            {
                var destName = ResolveMappedName(src, destType);
                if (!dests.TryGetValue(destName, out var dest))
                {
                    continue;
                }

                pairs.Add((src, dest));
                mappedDest.Add(dest.Name);
            }

            foreach (var dest in dests.Values)
            {
                if (mappedDest.Contains(dest.Name))
                {
                    continue;
                }

                var srcName = ResolveMappedName(dest, sourceType);
                var src = sources.FirstOrDefault(p => p.Name.Equals(srcName, StringComparison.OrdinalIgnoreCase));
                if (src is not null)
                {
                    pairs.Add((src, dest));
                }
            }

            return pairs;
        }

        /// <summary>
        /// 解析对面属性名：先取 <see cref="MapNameAttribute.Type"/> 等于 <paramref name="counterpartType"/> 的条目，
        /// 没有则取 Type 为 null 的条目，再没有则用属性名本身。
        /// </summary>
        private static string ResolveMappedName(PropertyInfo property, Type counterpartType)
        {
            var attrs = property.GetCustomAttributes<MapNameAttribute>().ToArray();
            var typed = attrs.FirstOrDefault(a => a.Type == counterpartType);
            if (typed is not null)
            {
                return typed.Name;
            }

            var untyped = attrs.FirstOrDefault(a => a.Type is null);
            return untyped?.Name ?? property.Name;
        }

        private sealed class CacheKey<TSource, TDestination>
        {
        }
    }
    /// <summary>
    /// 告诉 <see cref="Mapper"/> 该属性对应另一侧的哪个属性，用于两边属性名不一致的情况。同一属性可标多条。
    /// </summary>
    /// <remarks>
    /// <para>标在源属性上：本属性的值写入目标的 <see cref="Name"/>。</para>
    /// <para>标在目标属性上：本属性的值从源的 <see cref="Name"/> 读取。</para>
    /// <para>
    /// <see cref="Type"/> 指定对面是哪个类：映射到（或来自）该类时用这条。
    /// 同一属性可写多条，分别对应不同类；映射时先找 Type 等于对面类型的，没有则用 Type 为 null 的（即 <see cref="MapNameAttribute(string)"/>），再没有则用属性名本身。
    /// </para>
    /// <para>两边任选一处标注即可；都标了以源上的为准。属性名本身一致时不需要标。</para>
    /// <example>
    /// <code>
    /// public class AModel
    /// {
    ///     [MapName(typeof(AEntity), "Nameyyy")]
    ///     [MapName(typeof(BEntity), "NameB")]
    ///     [MapName("NameDefault")]
    ///     public string Namexx { get; set; }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class MapNameAttribute : Attribute
    {
        /// <param name="name">另一侧对应的属性名，忽略大小写。适用于任意对面类型（<see cref="Type"/> 为 null）。</param>
        public MapNameAttribute(string name)
            : this(null, name)
        {
        }

        /// <param name="type">对面类型：标在源上表示映射到哪个类，标在目标上表示从哪个类来。null 表示不限定类型，作回退。</param>
        /// <param name="name">该对面类型下对应的属性名，忽略大小写。</param>
        public MapNameAttribute(Type? type, string name)
        {
            Type = type;
            Name = name;
        }

        /// <summary>对面类型；null 表示不限定，找不到匹配类型时回退到这条。</summary>
        public Type? Type { get; }

        /// <summary>另一侧对应的属性名。</summary>
        public string Name { get; }
    }
}
