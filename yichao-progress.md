# 进度

README 已写入仓库根目录，覆盖对象映射、JSON、字符串/集合，以及 ClickHouse 单表用法。

## Mapper 实现进度

## 决策
- `MapNameAttribute` 可重复（`AllowMultiple`）。构造：`(string name)` 表示 Type=null 回退；`(Type type, string name)` 指定对面类型。
- 解析对面属性名：先 Type 等于对面类型 → 再 Type 为 null 的 name → 再用属性名本身。
- 表达式树按「源类型 + 目标类型」缓存委托，反射只在首次。
- 类型转换：CLR 强转数值；`ChangeValue` 处理 string 互转、**0/1↔bool**（其它数字抛错）、**long 时间戳↔DateTime**（与 JsonExtension 相同：&lt;1e11 秒否则毫秒，写出毫秒）。转不了的属性跳过。
- 批量：`MapTo<T>()` 新建列表；`MapTo(IList<T>)` 按下标；`MapTo(IList<T>, x => x.Id)` 按键匹配。

## 状态
已完成并验证：int↔long、int↔string、0/1↔bool、long 时间戳↔DateTime、非法数字转 bool 抛错、类型不兼容跳过。

## RandromName
`SqlOperatorExtention.RandromName` 用 `Random.Shared` 从完整大小写字母表抽 7 位，不要求结果同时含两种大小写。方法名与 `this object` 签名未改。

## BaseRepository
`EndQuery` / `EndExecute` 仍是抽象方法，子类把 `m_sqlHelper` 收成 `IBatch`。其余方法已在基类实现：查询走 `EndQuery` + `_executeBatch.Query*`；单条增删改走 `EndExecute` + `Execute`；列表走 `Bulk*`。分页 SQL 按 ClickHouse `LIMIT offset, count` 拼，总数用 `COUNT(1)` 子查询。`m_sqlHelper` 改为 `private protected`，同程序集子类能读到。`CKBaseRepository` 只保留构造函数，两个 End 方法尚未生成 SQL。
架构文档：`CKSimpleORM/ARCHITECTURE.md`（分层与流程）。设计原理：`CKSimpleORM/DESIGN.md`（方言分离、先收集再生成、整行追加写入、两套参数、有状态仓储，以及单条写入与原理的差距）。
已知问题（未修）：单条 `Insert(T)`/`Update`/`Delete` 在基类走 `Execute(batch)`，但 `CKBaseRepository.EndExecute` 只给 `Value` 不给 SQL，所以单条写入无效；列表版走 `Bulk*` 可用。`Insert(T)` 存 `AddDynamicParams`，而 `InsertPreExecute` 读 `ListParams`。
`CKExecuteBatch` 在 `GetConnection()` 为 null 时不访问数据库，只 `Console.WriteLine` 将要执行的 SQL。批量插入/删除没有现成 SQL，打印 `INSERT`/`DELETE` 加表名和列名。查询在这种情况下返回空序列，执行返回 0。
