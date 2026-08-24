# Framework.Common.Core

常用扩展：对象映射、JSON、字符串。

## Mapper

扁平对象按属性名（忽略大小写）互转。名字不同时用 `[MapName]`；可按对面类型写多条，找不到再用 Type 为空的那条。

```csharp
model.MapTo<AEntity>();           // 新建
model.MapTo(entity);              // 写入已有对象
models.MapTo<AEntity>();          // 批量新建
models.MapTo<AEntity>(entities);  // 按位置更新
models.MapTo(entities, x => x.Id); // 按键更新
```
属性配对
默认按属性名忽略大小写配对。名字不同时用可重复的 MapNameAttribute：

[MapName("NameDefault")]：不限定对面类型，作回退。
[MapName(typeof(AEntity), "Nameyyy")]：只在对面是该类时生效。
解析顺序：对面类型精确匹配 → Type 为 null 的 name → 属性名本身。源或目标任一端标注即可；都标了以源为准。配不上的属性跳过，目标保留原值。

类型转换
能转则转，转不了则跳过该属性：

组合	行为
int ↔ long 等数值
CLR 强转
int ↔ string、string ↔ 枚举 / Guid
解析 / 格式化
0、1（含 "0"/"1"）↔ bool
其它数字抛异常
long 时间戳 ↔ DateTime
与 JsonExtension 相同：绝对值 < 1e11 按秒，否则毫秒；写出一律毫秒、本地时间
不递归映射嵌套对象，也不复制集合元素。非法字符串（如 "abc" → int）会抛异常。

性能
按「源类型 + 目标类型」用表达式树编译赋值委托并缓存；反射只在该组合首次出现时发生。

## Json

`ToJson()` / `ToObject<T>()`。属性名小驼峰；时间默认 `yyyy-MM-dd HH:mm:ss`，`ToJson(true)` 输出 Unix 毫秒。反序列化同时认日期字符串、ISO、秒/毫秒时间戳。null 序列化为空串，空 JSON 反序列化为 null。

## String

- `ToUpperFirst()`：首字母大写，如 `status` → `Status`
- `ToSqlIn()`：集合转 SQL IN。数字 `1,2,3`，字符串加单引号并转义 `'`
