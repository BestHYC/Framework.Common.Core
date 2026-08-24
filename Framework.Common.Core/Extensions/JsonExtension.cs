using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Framework.Common.Core
{
    /// <summary>
    /// JSON 扩展：小驼峰属性名，时间默认 <c>yyyy-MM-dd HH:mm:ss</c>，可选 Unix 毫秒时间戳。
    /// </summary>
    /// <remarks>
    /// 仅配置 <see cref="JsonSerializerOptions"/>，属性名走 <see cref="JsonNamingPolicy.CamelCase"/>，不做额外改写。
    /// <c>ToJson(null)</c> 返回空字符串；空 JSON 调用 <c>ToObject</c> 返回 <c>null</c>。
    /// </remarks>
    public static class JsonExtension
    {
        private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

        private static readonly JsonSerializerOptions DateStringOptions = CreateOptions(false);
        private static readonly JsonSerializerOptions TimestampOptions = CreateOptions(true);

        /// <summary>
        /// 序列化为 JSON。属性名为小驼峰；时间默认 <c>yyyy-MM-dd HH:mm:ss</c>，<paramref name="useTimestamp"/> 为 true 时输出 Unix 毫秒时间戳。
        /// </summary>
        public static string ToJson(this object value, bool useTimestamp = false)
        {
            if (value is null)
            {
                return string.Empty;
            }

            return JsonSerializer.Serialize(value, useTimestamp ? TimestampOptions : DateStringOptions);
        }

        /// <summary>
        /// 将 JSON 反序列化为对象。属性名为小驼峰；时间支持日期字符串、ISO 字符串以及秒/毫秒时间戳。
        /// </summary>
        public static T ToObject<T>(this string json) where T : class
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null!;
            }

            return JsonSerializer.Deserialize<T>(json, DateStringOptions)!;
        }

        private static JsonSerializerOptions CreateOptions(bool useTimestamp)
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new DateTimeJsonConverter(useTimestamp),
                    new NullableDateTimeJsonConverter(useTimestamp)
                }
            };
        }

        private sealed class DateTimeJsonConverter : JsonConverter<DateTime>
        {
            private readonly bool _useTimestamp;

            public DateTimeJsonConverter(bool useTimestamp)
            {
                _useTimestamp = useTimestamp;
            }

            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return ReadDateTime(ref reader);
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                WriteDateTime(writer, value, _useTimestamp);
            }
        }

        private sealed class NullableDateTimeJsonConverter : JsonConverter<DateTime?>
        {
            private readonly bool _useTimestamp;

            public NullableDateTimeJsonConverter(bool useTimestamp)
            {
                _useTimestamp = useTimestamp;
            }

            public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                {
                    return null;
                }

                return ReadDateTime(ref reader);
            }

            public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
            {
                if (!value.HasValue)
                {
                    writer.WriteNullValue();
                    return;
                }

                WriteDateTime(writer, value.Value, _useTimestamp);
            }
        }

        private static DateTime ReadDateTime(ref Utf8JsonReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Number:
                    if (reader.TryGetInt64(out var number))
                    {
                        return FromUnixTimestamp(number);
                    }

                    throw new JsonException("无法将数字解析为时间戳。");

                case JsonTokenType.String:
                    var text = reader.GetString();
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        throw new JsonException("空字符串无法转换为 DateTime。");
                    }

                    if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var timestamp))
                    {
                        return FromUnixTimestamp(timestamp);
                    }

                    if (DateTime.TryParseExact(text, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var formatted))
                    {
                        return formatted;
                    }

                    if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed))
                    {
                        return parsed;
                    }

                    throw new JsonException($"无法将 \"{text}\" 转换为 DateTime。");

                default:
                    throw new JsonException($"不支持的 DateTime JSON 标记：{reader.TokenType}。");
            }
        }

        private static void WriteDateTime(Utf8JsonWriter writer, DateTime value, bool useTimestamp)
        {
            if (useTimestamp)
            {
                writer.WriteNumberValue(ToUnixMilliseconds(value));
                return;
            }

            writer.WriteStringValue(value.ToString(DateFormat, CultureInfo.InvariantCulture));
        }

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
    }
}
