# Framework.Common.Core

.NET 8 公共库。提供对象映射、JSON、字符串/集合扩展，以及 ClickHouse 单表增删改查。

依赖：ClickHouse.Client、Dapper、Microsoft.Extensions.Logging.Abstractions。

## 对象映射

扁平对象按属性名互转，忽略大小写。名字不同时用 `[MapName]`。

```csharp
model.MapTo<AEntity>();                 // 新建
model.MapTo(entity);                    // 写入已有对象
models.MapTo<AEntity>();                // 批量新建
models.MapTo<AEntity>(entities);        // 按位置更新
models.MapTo(entities, x => x.Id);      // 按键更新
```

`[MapName("Name")]` 作为不限类型的回退。`[MapName(typeof(AEntity), "Name")]` 只在对面是该类时生效。匹配顺序：对面类型 → 不限类型的名字 → 属性名本身。配不上的属性跳过。

数值按 CLR 强转。`int` 与 `string`、枚举、Guid 可以互转。`0`/`1` 与 `bool` 互转，其它数字转 `bool` 会抛错。`long` 时间戳与 `DateTime` 互转：绝对值小于 `1e11` 按秒，否则按毫秒；写出为本地时间的毫秒。不映射嵌套对象和集合元素。

## JSON

`ToJson()` / `ToObject<T>()`。属性名小驼峰。时间默认 `yyyy-MM-dd HH:mm:ss`，`ToJson(true)` 输出 Unix 毫秒。反序列化认日期字符串、ISO、秒/毫秒时间戳。`null` 序列化为空串，空 JSON 反序列化为 `null`。

## 字符串和集合

- `ToUpperFirst()`：首字母大写，`status` → `Status`
- `ToSqlIn()`：集合转 SQL IN。数字 `1,2,3`，字符串加单引号并转义 `'`
- `IsNullOrEmpty()` / `HasValue()`：集合是否为空

## ClickHouse

只处理单表。实体用特性标表和列：

```csharp
[TabelName("adgroup_all")]
public class Adgroup
{
    [PK]
    public long Id { get; set; }

    [ColumnName("name", "String")]
    public string Name { get; set; }

    [Ignore]
    public string NameText { get; set; }
}
```

`[Increment]` 表示自增列。查询会带 `FINAL`，并固定 `sign = 1`。

```csharp
repo.Where(x => x.Id == 1).OrderByDesc(x => x.Name).QueryList();
repo.Select(x => new { x.Id, x.Name }).Top(10).Query();
repo.Insert(entity);
repo.InsertList(list);
repo.Update(entity);                         // 按主键
repo.Where(x => x.Id == 1).Delete();
repo.QueryByPage(sql, pageInfo, parameters); // PageIndex 从 1 开始
```

`Where`、`OrderBy`、`Select`、`Top` 可以连续写。条件支持 `==`、`!=`、比较、`&&`、`||`、字符串 `Contains`（生成 `LIKE`）、集合 `Contains`（生成 `IN`）。

ClickHouse 的更新和删除通过再写入完成，列表写入走批量插入。`CKDBContext.ViewType = true` 时不连库，只把 SQL 打到控制台。

`CKDBSet<T>` 从依赖注入取出对应的 `CKBaseRepository<T>`。使用前设置：

```csharp
ServiceHelper.Instance = app.Services;
```
