# ResolveBus 路线图入口

这是 `2.0.0.0` 之后 ResolveBus 与原料贡献路线的内部入口。完整草案见 [路线图-ResolveBus与原料贡献.md](../../路线图-ResolveBus与原料贡献.md)。

## 目标

ResolveBus 计划解决“多个依赖模组共同贡献一个返回值”的场景，尤其是合成助手 `+` 需要的原料 ID 解析和候选合并。

当前方向：

- 保留 `RecipaediaEventBus` 作为通知通道。
- 保留 `RecipaediaInterceptBus` 作为否决通道。
- 新增 `RecipaediaResolveBus` 作为自定义返回值与聚合策略通道。
- 删除或迁移 `CraftingOverlayIngredientBridge` 这种单槽静态注入点。

## 对外影响

真正实施后，需要同步更新：

- [事件与拦截总线](../../api/events-and-intercepts.md)
- [合成助手接入](../../api/crafting-overlay-integration.md)
- [更新日志](../../release/changelog.md)

在未实现前，本文只代表计划，不作为稳定 API 承诺。
