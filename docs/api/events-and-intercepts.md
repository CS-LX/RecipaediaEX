# 事件与拦截总线

RecipaediaEX 提供两条扩展通道，让依赖模组不用修改 REX 源码也能参与生命周期、生产、取出、搜索和合成助手行为。完整接口细节见 [旧版 API 汇总](../API使用文档.md#25-事件与扩展总线)。

## 通道区别

| 通道 | 语义 | 订阅签名 | 能否阻止行为 |
|------|------|----------|--------------|
| `RecipaediaEventBus` | 事后通知或请求广播 | `Action<T>` | 否 |
| `RecipaediaInterceptBus` | 事前门禁和改写 | `Func<T, bool>` | 是，返回 `false` 否决 |

无订阅者时，拦截通道默认放行，不改变游戏行为。

## 订阅示例

```csharp
using RecipaediaEX.Events;

IDisposable sub = RecipaediaEventBus.RecipesReset.Subscribe(e => {
    RebuildMyRecipeIndex(e.RecipeCount);
});
```

拦截示例：

```csharp
IDisposable sub = RecipaediaInterceptBus.CrafterOutputRemoving.Subscribe(ctx => {
    if (!CanPlayerTake(ctx.Player, ctx.Recipe)) {
        return false;
    }

    return true;
});
```

请保存 `IDisposable`，在模组卸载、世界退出或不再需要订阅时释放，避免旧世界状态残留到下一次会话。

## 常见内置场景

| 场景 | 推荐通道 |
|------|----------|
| 配方表重建后刷新缓存 | `RecipaediaEventBus.RecipesReset` |
| 玩家成功取出产物后统计进度 | `CrafterOutputRemoved` 类事件 |
| 阻止玩家取出产物 | `CrafterOutputRemoving` 类拦截 |
| 合成助手打开 / 关闭门禁 | `CraftingOverlayOpening` / `CraftingOverlayClosing` |
| 改写 Overlay 搜索词 | `OverlaySearchApplying` |
| 接管非合成场景 Recipaedia 键 | `OpenFullRecipaediaRequestedEvent` 或 `OpenFullRecipaediaNavigating` |

## 命名约定

- `*Event`：行为已经发生或请求广播，订阅者不能否决。
- `*Context`：行为即将发生，订阅者可以读取上下文、改写可变字段或返回 `false` 否决。

## ResolveBus 路线

`2.0.0.0` 之后计划新增 `RecipaediaResolveBus`，用于“多模组共同贡献一个返回值”的场景，例如原料 ID 解析和候选合并。设计草案见 [ResolveBus 路线图](../internal/roadmap/resolve-bus.md)。
