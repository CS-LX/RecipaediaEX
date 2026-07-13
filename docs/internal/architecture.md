# 架构笔记

本文是 RecipaediaEX 内部贡献者的快速架构入口。它不替代源码和 API 文档，只说明当前文档分层后的主要边界。

## 模块边界

| 模块 | 职责 | 主要读者 |
|------|------|----------|
| Recipes / Loader | `IRecipe`、静态配方、动态配方、匹配入口 | 外部开发者、内部开发者 |
| Recipaedia UI | 图鉴分类、条目、详情、配方展示 | 外部开发者、内部开发者 |
| Search | 搜索索引、查询解析、筛选 Dialog、历史 | 玩家、外部开发者、内部开发者 |
| Events | EventBus、InterceptBus、后续 ResolveBus | 外部开发者、内部开发者 |
| Overlay | 合成助手 Dialog、Host、Placement、快捷键路由 | 玩家、外部开发者、内部开发者 |
| Release tooling | version sync、pack、CI、模组站发布 | 维护者 |

## 文档单一真相源

| 信息类型 | 维护位置 |
|----------|----------|
| 玩家如何使用 | `docs/user/` |
| 外部 API 如何接入 | `docs/api/` |
| 设计背景和历史决策 | `docs/internal/` |
| 版本变化和迁移 | `docs/release/changelog.md` |
| 发版流程和 CI | `docs/release/` |

策划案中如果出现 API 片段，只作为历史语境；稳定接入方式应同步到 `docs/api/`。

## 当前重要边界

- REX 核心不写死内容模组专有机器逻辑。
- 合成助手的专有机器摆放规则由内容模组的 `IRecipePlacementTarget` 解释。
- Host 接入进度不在 REX 仓库维护；REX 只维护协议和通用实现。
- 玩家偏好和搜索历史不是世界存档数据。
- 发布版本号以 `modinfo.json` 的 `Version` 为唯一真相源。

## 代码锚点

| 主题 | 代码锚点 |
|------|----------|
| 配方匹配 | `RecipaediaEXManager`、`RecipesLoadManager` |
| 动态配方 | `IDynamicRecipeLoader`、`AdHocRecipeLoader` |
| 图鉴页 | `RecipaediaEXScreen`、`RecipeDescriptor` |
| 搜索 | `RecipaediaSearchEngine`、`RecipaediaSearchIndex` |
| 事件 / 拦截 | `RecipaediaEventBus`、`RecipaediaInterceptBus` |
| 合成助手 | `RecipaediaCraftingOverlayDialog`、`RecipaediaCraftingOverlayController` |
| 摆放 | `IRecipePlacementTarget`、`PlacableRecipeAdapter`、`FormattedGridPlacementPlanner` |
