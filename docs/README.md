# RecipaediaEX 文档索引

RecipaediaEX 文档按读者分为三层，加一层发布运维资料。优先从与你身份匹配的入口开始；旧的策划和中文长文仍保留，作为历史记录与深度参考。

## 用户层

面向生存战争玩家，说明游戏里实际能看到和操作的功能。

| 文档 | 内容 |
|------|------|
| [用户文档入口](user/README.md) | 玩家向阅读顺序 |
| [图鉴搜索](user/recipaedia-search.md) | 搜索框、筛选、查询语法、搜索历史 |
| [合成助手](user/crafting-overlay.md) | 合成界面内查配方、`+` 自动摆放、长按连放、已知限制 |

## 外部开发者层

面向依赖 RecipaediaEX 的其它模组作者，说明稳定 API、扩展点和迁移注意事项。

| 文档 | 内容 |
|------|------|
| [API 文档入口](api/README.md) | API 主题索引与接入顺序 |
| [配方与加载器](api/recipes-and-loaders.md) | `IRecipe`、`IRecipesLoader`、动态配方、匹配入口 |
| [图鉴 UI 扩展](api/recipaedia-ui.md) | 图鉴条目、分类、详情页、Descriptor |
| [事件与拦截总线](api/events-and-intercepts.md) | `RecipaediaEventBus`、`RecipaediaInterceptBus`、订阅生命周期 |
| [合成助手接入](api/crafting-overlay-integration.md) | Overlay Host、Placement Target、`+` 摆放接入 |
| [完整 API 参考（兼容旧入口）](API使用文档.md) | 旧版单文件 API 汇总 |

依赖模组的具体 Host 接入进度仍由各内容模组维护；工业时代 2 的接入清单见主仓 `docs/guides/合成助手-工业机器接入清单.md`。

## 内部开发者层

面向贡献 RecipaediaEX 本体的人，保留设计背景、策划案、路线图和验收记录。

| 文档 | 内容 |
|------|------|
| [内部开发文档入口](internal/README.md) | 内部资料导航 |
| [架构笔记](internal/architecture.md) | 当前架构边界与代码锚点 |
| [图鉴搜索策划](internal/plans/recipaedia-search-plan.md) | 搜索功能历史策划入口 |
| [合成助手策划](internal/plans/crafting-overlay-plan.md) | 合成助手历史策划入口 |
| [JEI 对标与基元语句](internal/plans/jei-primitives.md) | 合成助手设计依据 |
| [ResolveBus 路线图](internal/roadmap/resolve-bus.md) | 2.0 之后扩展总线路线 |

## 发布与运维层

面向维护者，说明版本、构建、CI、GitHub Release 与模组站发布。

| 文档 | 内容 |
|------|------|
| [发布文档入口](release/README.md) | 发版资料导航 |
| [更新日志](release/changelog.md) | 版本变化与依赖模组适配指南 |
| [发版流程](release/release-process.md) | 本地验证、tag、Release、模组站发布 |
| [CI 与打包决策记录](release/ci-packaging.md) | 已实施的 CI / 打包方案与历史背景 |

## 旧文档状态

以下文件暂时保留在 `docs/` 根目录，避免外部链接失效。新文档会逐步把它们拆成主题页；需要追溯设计细节时仍可阅读。

| 旧文档 | 现在归属 |
|--------|----------|
| [API使用文档.md](API使用文档.md) | 外部开发者层完整参考 |
| [CHANGELOG.md](CHANGELOG.md) | 发布与运维层更新日志 |
| [RELEASE.md](RELEASE.md) | 发布与运维层发版流程 |
| [打包发布CI策划.md](打包发布CI策划.md) | 发布与运维层历史决策 |
| [图鉴搜索功能策划.md](图鉴搜索功能策划.md) | 内部开发者层历史策划 |
| [工作台悬浮助手策划.md](工作台悬浮助手策划.md) | 内部开发者层历史策划 |
| [合成助手-JEI对标与基元语句.md](合成助手-JEI对标与基元语句.md) | 内部开发者层设计依据 |
| [路线图-ResolveBus与原料贡献.md](路线图-ResolveBus与原料贡献.md) | 内部开发者层路线图 |
