# CKSimpleORM 设计原理

结构、类型职责和调用步骤见 [ARCHITECTURE.md](ARCHITECTURE.md)。本文说明这些拆分是为了什么，以及当前代码和这些原理差在哪里。

## 要解决的问题

ClickHouse 的表不是「改一行、删一行」。常用的是 ReplacingMergeTree 一类引擎：写入新行，用 `version` 决定哪一行更新，用 `sign` 表示这一行是有效还是删除，查询时加 `FINAL` 并过滤 `sign = 1`。

如果按 SQL Server 的方式生成 `UPDATE ... SET` 和 `DELETE FROM`，在这类表上语义是错的。ORM 要做的是：调用方仍然写 `Where`、`Update`、`Delete`，落到 ClickHouse 时变成「查出完整行，改内存里的对象，再整行插入」。

同时只做单表。多表、复杂聚合、索引选择交给手写 SQL。表达式只覆盖日常条件，避免把 ORM 做成又一个查询语言。

## 原理

### 1. 方言和执行分开

`BaseRepository<T>` 只收集调用意图，不拼 ClickHouse SQL。`CKBaseRepository<T>` 在 `EndQuery` / `EndExecute` 里把意图变成 `CKBatch`。`CKExecuteBatch<T>` 只负责拿连接并执行，不理解表达式。

这样换数据库时改的是 `EndQuery` / `EndExecute`，链式 API 和 Dapper / BulkCopy 调用不用重写。`IBatch` 只有三样东西：`Sql`、`DynamicParameters`、`Value`。查询用前两个，批量写入用第三个。

### 2. 先收集，再生成

一次链式调用里，条件、列、排序、更新值来自不同方法，不能边调边拼字符串，否则 `AND`、别名、参数名会散落在各处。

`SQLHelper` 按用途分桶，各桶共用同一份 `DynamicParameters`：

| 桶 | 收集什么 | 谁来用 |
| --- | --- | --- |
| `Query` | SELECT 列 | `EndQuery` |
| `Where` | 条件 | 查询，以及「先查再写」的更新/删除 |
| `Orderby` | 排序列和升降序 | `EndQuery` |
| `UpdateHeader` | `UpdateColumnsWithValue` 里的列和值 | 无实体列表时的更新 |
| `OperatorColumns` | `UpdateColumns` 指定的列名 | 有实体列表时，决定覆盖哪些列 |
| `ListParams` | 要写入的 `List<T>` | `EndExecute` → `CKBatch.Value` |
| `Table` | 表类型和 Insert / Update / Delete | `EndExecute` 分派 |

`EndQuery` 或 `EndExecute` 结束时 `Clear`，下一次链式调用从空桶开始。所以这些桶是「这一次调用的草稿」，不是实体的永久映射。

### 3. 表达式变成列和参数，不直接变成 SQL

`x => x.AdGroupId > 0` 这类表达式先走 `SQLExpressionExtensions`，结果是一串 `ColumnRelevanceMapper` 加上 Dapper 参数，还不是 SQL 文本。

- 实体属性：记下属性名、表类型，稍后用特性换成 `表.列`。
- 常量或闭包里的变量：编一个随机参数名，值放进 `DynamicParameters`。
- `==`、`&&`、`||`、比较、`LIKE`、`IN`：记在下一个列映射的运算符上。

拼 SQL 只发生在 `CKBaseRepository`。列名以特性为准，调用方写的是 C# 属性名。`[Ignore]` 的属性不进元数据，不会出现在 SQL 里。

参数名用随机字母，是为了避免 `a.Id == b.Id` 或同一个条件里两个常量撞成同一个 `@Id`。

### 4. 写入必须是整行

`ClickHouseBulkCopy` 按列写一批行。`CKExecuteBatch` 会补上三列：

- `version`：当前 UTC 毫秒，当作版本号
- `sign`：插入和更新为 1，删除为 0
- `_insert_time`：写入时间

这三列从实体属性里排除，不要求调用方赋值。主键为空或等于类型默认值则拒绝写入，否则会插进无法合并的行。

因此更新不能只传改过的列。`UpdateColumns` 的含义是：先按主键把库里的整行查出来，只用入参覆盖这些列，其余列保持库里的值，再整行插入。`UpdateColumnsWithValue` 则是先按 `Where` 查出集合，把表达式里的常量写到这些对象上，再整行插入。

删除同样是插入：`sign = 0` 的新行把旧行盖掉。查询侧固定 `sign = 1`，被盖掉的行不再出现。

### 5. 两套参数

`Parameters` 是一条 SQL 上的 Dapper 参数，例如 `WHERE id = @id`。

`ListParams` 是要写入的对象列表。批量插入若把每一行都塞进同一个 `DynamicParameters`，列名会重复并抛错。列表因此单独放着，由 BulkCopy 逐行读取属性，不经过 Dapper 参数。

`IN` 里的数组仍然属于 `Parameters`，和 `ListParams` 不是一回事。

### 6. 元数据只反射一次

表名、列名、主键、自增列来自特性，和某一次查询无关。`EntityTableWrapper` 按 `Type` 缓存 `EntityMapTable`。表达式解析和 BulkCopy 都读这份缓存，避免每行都 `GetCustomAttribute`。

### 7. 仓储有状态，连接没有业务状态

`SQLHelper` 挂在仓储实例上。同一次链式调用要能跨 `Where`、`Select`、`Update` 累积，所以它必须有状态。两个请求共用一个实例时，条件会串，`Clear` 还会清掉另一个请求尚未执行的内容。仓储按请求创建，不要注册成单例。

`CKExecuteBatch` 和 `CKDBContext` 不保存查询草稿，只保存连接串、日志和并行度，可以共用。`ViewType = true` 时 `GetConnection()` 返回 null，执行器打印 SQL 或批量语句后返回空结果。预览和真执行走同一条拼 SQL 的路径，避免预览和实际执行各写一套。

## 一次调用怎么走

以 `Where(x => x.AdGroupId > 0).Select(...).QueryList()` 为例：

1. `Where` / `Select` 把表达式写入 `SQLHelper` 的对应桶。
2. `QueryList` 调用 `EndQuery`。
3. `CKBaseRepository` 按桶拼出 `SELECT ... FROM 表 FINAL WHERE sign = 1 AND ...`，参数挂到 `CKBatch`。
4. `finally` 里 `Clear`。
5. `CKExecuteBatch.QueryList` 取连接。有连接就 Dapper 查询；没有连接就 `Console.WriteLine` 这条 SQL，返回空序列。

以 `UpdateList(list)` 且指定了 `UpdateColumns` 为例：

1. 基类标记操作为 Update，列表放进 `ListParams`。
2. `EndExecute` 发现是更新且带了列名单：对列表里每一行 `QueryByPK`，把指定列从入参抄到查出来的对象上。
3. 这些完整对象放进 `CKBatch.Value`。
4. `BulkUpdate` 实际调用 `BulkInsert`，`sign = 1`，整行写入。

`Where(...).UpdateColumnsWithValue(...).Update()` 不带列表。`EndExecute` 先 `QueryList()` 取出符合条件的行，再用表达式里的值覆盖，然后同样整行插入。

## 和当前代码的差距

原理要求「凡是写入都走批量插入」。列表方法已经这样：`InsertList`、`UpdateList`、`Delete(List<T>)` 调用 `Bulk*`，读的是 `CKBatch.Value`。

单条方法还没有对齐：

| 方法 | 基类实际调用 | `EndExecute` 实际产出 | 结果 |
| --- | --- | --- | --- |
| `InsertList` / `UpdateList` / `Delete(List<T>)` | `Bulk*` | `Value = List<T>` | 与原理一致 |
| `Insert(T)` | `Execute(batch)`，执行 `batch.Sql` | 只读 `ListParams`，而单条插入放在 `Parameters` 里 | SQL 为空，数据也取不到 |
| `Update()` / `Update(T)` / `Delete()` / `Delete(T)` | `Execute(batch)` | 准备好了 `Value`，没有 SQL | 准备好的行不会被 BulkCopy 写入 |

要和原理对齐，单条写入应包成只有一个元素的列表，走 `Bulk*`；`Insert(T)` 应写入 `ListParams`，而不是只写入 `DynamicParameters`。

另外两处和原理无关、但会影响使用：

- `BulkDelete` 失败时日志仍写「批量插入失败」，并且 `throw ex` 丢掉原始堆栈。
- `ExecuteReader(IBatch, int)` 尚未实现。接口留着是为了按行读取大结果集，当前查询全部由 Dapper 一次装进内存。
