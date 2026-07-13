# 外部开发者 API 文档

本层面向依赖 RecipaediaEX 的其它 Survivalcraft 模组作者。目标是说明稳定扩展点、推荐接入方式和版本迁移注意事项。

## 推荐阅读顺序

1. [配方与加载器](recipes-and-loaders.md)：先把配方对象、加载器和匹配入口接起来。
2. [图鉴 UI 扩展](recipaedia-ui.md)：让条目、分类、详情页和配方展示进入图鉴。
3. [事件与拦截总线](events-and-intercepts.md)：按需订阅生命周期、生产、取出、搜索和助手行为。
4. [合成助手接入](crafting-overlay-integration.md)：让自己的工作台或机器支持同屏查配方和 `+` 自动摆放。

完整旧版 API 汇总仍保留在 [API使用文档.md](../API使用文档.md)。当主题页与旧汇总冲突时，以源码和最新 CHANGELOG 为准。

## 版本迁移

版本差异、破坏性变更和依赖写法见 [更新日志](../release/changelog.md)。发布前请确认你的 `modinfo.json` 中 `com.recipaediaex` 依赖版本与实际使用的 API 对齐。

## 跨仓接入清单

RecipaediaEX 只维护框架 API。各内容模组的 Host / Placement 接入进度应在各自仓库维护。工业时代 2 的接入清单见主仓 `docs/guides/合成助手-工业机器接入清单.md`。
