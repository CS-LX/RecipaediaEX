# 内部开发文档

本层面向维护 RecipaediaEX 本体的贡献者，保存架构边界、设计决策、策划案、路线图和验收记录。普通玩家请看 [用户文档](../user/README.md)，依赖模组作者请看 [API 文档](../api/README.md)。

## 当前入口

| 文档 | 状态 | 说明 |
|------|------|------|
| [架构笔记](architecture.md) | 当前 | REX 代码边界、主要模块和文档 SSOT |
| [图鉴搜索策划](plans/recipaedia-search-plan.md) | 已交付核心 | 搜索功能历史背景、决策和验收 |
| [合成助手策划](plans/crafting-overlay-plan.md) | `2.0.0.0` 已发，部分后续搁置 | Overlay、Placement、玩家反馈、验收记录 |
| [JEI 对标与基元语句](plans/jei-primitives.md) | 设计参考 | 合成助手体验和 API 边界依据 |
| [ResolveBus 路线图](roadmap/resolve-bus.md) | 计划 | `2.0.0.0` 之后扩展总线方向 |

## 维护原则

- 玩家能直接感知的玩法，抽到 [用户文档](../user/README.md)。
- 外部模组可依赖的稳定接口，抽到 [API 文档](../api/README.md)。
- 版本变化和适配指南，写到 [发布层更新日志](../release/changelog.md)。
- 策划案保留背景、取舍、验收和历史，不再承担 API 参考或玩家手册职责。

## 历史文件

旧策划文件仍保留在 `docs/` 根目录，避免外部链接失效。内部层的 `plans/` 和 `roadmap/` 文件是面向新结构的入口，会指向旧长文。
