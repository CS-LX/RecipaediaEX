# 发布与运维文档

本层面向 RecipaediaEX 维护者，覆盖版本号、构建、CI、GitHub Release、模组站发布和依赖模组通知。

## 入口

| 文档 | 内容 |
|------|------|
| [更新日志](changelog.md) | 版本变化、破坏性变更、依赖模组适配指南 |
| [发版流程](release-process.md) | 本地验证、提交、tag、Release、模组站发布 |
| [CI 与打包决策记录](ci-packaging.md) | 已实施的 CI / pack 方案和历史背景 |

## 版本单一真相源

版本号以 `modinfo.json` 的 `Version` 为唯一真相源。构建前由 `tools/sync-version.ps1` 同步到 `RecipaediaEX.csproj`。

## 分工

- `changelog.md` 是版本变化和迁移指南的入口。
- `release-process.md` 是执行发版时的 SOP。
- `ci-packaging.md` 是 CI / 打包设计背景，不应作为每次发版的操作清单。

旧文件 [CHANGELOG.md](../CHANGELOG.md)、[RELEASE.md](../RELEASE.md)、[打包发布CI策划.md](../打包发布CI策划.md) 暂时保留，避免外部链接失效。
