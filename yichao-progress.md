# 进度

README 已写入仓库根目录，简要覆盖 Mapper / Json / String。

## Mapper 实现进度

## 决策
- `MapNameAttribute` 可重复（`AllowMultiple`）。构造：`(string name)` 表示 Type=null 回退；`(Type type, string name)` 指定对面类型。
- 解析对面属性名：先 Type 等于对面类型 → 再 Type 为 null 的 name → 再用属性名本身。
- 表达式树按「源类型 + 目标类型」缓存委托，反射只在首次。
- 类型转换：CLR 强转数值；`ChangeValue` 处理 string 互转、**0/1↔bool**（其它数字抛错）、**long 时间戳↔DateTime**（与 JsonExtension 相同：&lt;1e11 秒否则毫秒，写出毫秒）。转不了的属性跳过。
- 批量：`MapTo<T>()` 新建列表；`MapTo(IList<T>)` 按下标；`MapTo(IList<T>, x => x.Id)` 按键匹配。

## 状态
已完成并验证：int↔long、int↔string、0/1↔bool、long 时间戳↔DateTime、非法数字转 bool 抛错、类型不兼容跳过。
