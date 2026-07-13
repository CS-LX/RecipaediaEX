# 合成助手接入

本页说明依赖模组如何让自己的工作台、熔炉或专有机器接入合成助手。玩家向行为见 [合成助手](../user/crafting-overlay.md)，完整 API 细节见 [旧版 API 汇总](../API使用文档.md#10-合成助手crafting-overlay)。

## 接入目标

一个机器接入合成助手通常分两步：

1. **Host 接入**：机器界面能打开右侧合成助手，提供当前工作站、玩家等级、世界和库存上下文。
2. **Placement 接入**：配方卡上的 `+` 能从背包或可访问库存把材料摆进机器输入区。

只做 Host 时，玩家可以同屏查配方；同时做 Placement 时，玩家才能使用 `+` 自动摆放。

## `IRecipaediaOverlayHost`

合成 Modal Widget 实现该接口：

```csharp
public interface IRecipaediaOverlayHost {
    RecipaediaCraftingContext? GetCraftingContext();
    ContainerWidget OverlayParent { get; }
    IRecipePlacementTarget? GetPlacementTarget() => null;
}
```

推荐规则：

- `GetCraftingContext()` 返回 `null` 时，不显示助手入口，Recipaedia 键走全屏图鉴。
- `OverlayParent` 通常返回当前 Modal 根 Widget。
- `GetPlacementTarget()` 可以先返回 `null`，等预览稳定后再接 `+`。

## `RecipaediaCraftingContext`

上下文应尽量只描述 REX 通用信息：

| 字段 | 用途 |
|------|------|
| `CrafterBlockValue` | 默认 Crafter Tab、`+` 门控 |
| `PlayerLevel` | 匹配需要玩家等级的配方 |
| `RequiredHeatLevel` | 熔炼或热量匹配 |
| `Project` | 动态配方和世界状态 |
| `Inventory` | 玩家背包或当前可访问库存 |

不要把内容模组专有的流体、能量、管网等 API 塞进 REX 公共上下文。专有能力应留在你的 `IRecipePlacementTarget` 实现里解释。

## `IRecipePlacementTarget`

`IRecipePlacementTarget` 负责判断和执行 `+`：

```csharp
public interface IRecipePlacementTarget {
    bool CanAccept(IRecipe recipe);
    PlacementResult TryPlaceRecipe(
        IRecipe recipe,
        PlacementSources sources,
        PlacementOptions options,
        bool execute);
}
```

约定：

- `execute: false` 做预检，不移动物品。
- `execute: true` 真正移动物品并刷新机器结果。
- 材料不足时应返回清晰的缺失信息。
- 自定义机器的槽位、流体、能量、容器规则由 Target 自己解释。

## 可摆放配方

配方卡是否显示可用 `+` 由 `PlacableRecipeAdapter.IsPlacable(recipe)` 判断。REX 内置支持原版有形合成和熔炼输入区；内容模组专有配方需要：

- 让配方类直接实现 `IPlacableRecipe`，或
- 在模组启动时通过 `PlacableRecipeAdapter.Register` 注册包装工厂。

推荐让配方数据类保持纯粹，把 Overlay 相关包装放在接入层，避免 `Recipes` 模块反向依赖 UI。

## 接入清单

1. 找到机器 Modal Widget。
2. 实现 `IRecipaediaOverlayHost`。
3. 在 Widget 构造时放置助手入口按钮，或确保 Recipaedia 键可识别该 Host。
4. 填充 `RecipaediaCraftingContext`。
5. 如果只做预览，到这里即可。
6. 为机器实现 `IRecipePlacementTarget`。
7. 为专有配方注册 `IPlacableRecipe` / `PlacableRecipeAdapter`。
8. 手工验证：有料、缺料、非当前机器、关闭 Modal、连续打开关闭。

## 边界

- REX 核心不写死任何内容模组专有机器逻辑。
- Host 接入进度由内容模组维护，不在 REX 仓库维护。
- 对工业时代 2，请看主仓 `docs/guides/合成助手-工业机器接入清单.md`。
