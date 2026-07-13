# CI 与打包决策记录

CI / 打包方案的历史策划和实施记录继续保留在 [打包发布CI策划.md](../打包发布CI策划.md)。

## 当前状态

该旧文档最初是“打包 → 发布 CI 策划”，其中 P0–P3 已实施。现在它应被视为已实施决策记录，而不是待执行任务清单。

## 当前约定

| 场景 | 产物 |
|------|------|
| 本机 `dotnet build` | `ModsFolder/RecipaediaEX.scmod` |
| 日常 CI | `RecipaediaEX-ci.{sha7}.scmod` |
| GitHub Release | `RecipaediaEX-{Version}.scmod` |
| 模组站 | 与 GitHub Release 同版本 `.scmod` |

## 维护规则

- 每次发版的操作步骤写到 [发版流程](release-process.md) 指向的 SOP。
- 版本变化写到 [更新日志](changelog.md)。
- CI 方案发生结构性变化时，再更新旧策划或把新决策整理成本页的正式章节。

## 旧入口

为避免链接断裂，历史记录暂不移动。请继续编辑 [../打包发布CI策划.md](../打包发布CI策划.md)。
