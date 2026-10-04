# 图鉴 UI 扩展

本页说明依赖模组如何把自定义条目、分类、详情和配方展示接入 RecipaediaEX 图鉴。完整接口细节见 [旧版 API 汇总](../API使用文档.md#4-图鉴-ui-api)。

## 条目接口

| 接口 | 用途 |
|------|------|
| `IRecipaediaItem` | 出现在图鉴列表中的基础条目 |
| `IRecipaediaRecipeItem` | 声明条目与配方主产物 / 副产物 / 原料的关系 |
| `IRecipaediaDescriptionItem` | 提供详情页内容 |
| `IRecipaediaCategoryProvider` | 提供图鉴分类和条目集合 |

如果你的内容是方块，通常可以复用或包装已有方块条目；如果是流体、能量、科技项等非方块内容，需要实现自定义条目。

## 分类提供器

分类提供器必须能被无参构造。REX 会扫描加载程序集中的实现并实例化：

```csharp
public sealed class MyCategoryProvider : IRecipaediaCategoryProvider {
    public IEnumerable<IRecipaediaCategory> GetCategories() {
        yield return new MyCategory();
    }
}
```

分类内条目顺序应稳定，避免图鉴列表在每次刷新时跳动。

## 配方关系

条目想进入“怎么制作 / 能做什么”的配方页，需要实现 `IRecipaediaRecipeItem` 并配合配方 Extra：

- 配方对象写入 `RecipeExtraKeys.MatchedResultBlockValues`，用于主产物匹配。
- 配方对象写入 `RecipeExtraKeys.MatchedRemainsBlockValues`，用于副产物匹配。
- 配方放行副产物展示：默认**不**把配方作为副产物展示；需要放行时，自定义配方重写 `IRecipe.DisplaysAsRemains()`，或写入 `RecipeExtraKeys.DisplaysAsRemains`（`bool`；`.cr` 配方可写 `DisplaysAsRemains="true"`）。
- 配方对象写入 `RecipeExtraKeys.MatchedIngredientBlockValues`，用于原料用途匹配。
- 自定义非方块条目的匹配语义由依赖模组自己定义，但应保持产物、副产物和原料三个方向都能解释。

`Match` 与 `MatchesAsResult` 的区别：`Match` 是该条目的最终展示判定，在 `MatchesAsResult` 之外，还会纳入「配方已通过 `DisplaysAsRemains()` 放行、且条目作为其副产物」的配方；`MatchesAsResult` 始终只认主产物方向。需要「仅统计作为产物」的场合（如搜索索引）应使用 `MatchesAsResult`。

## Descriptor

`RecipeDescriptor` 决定配方在图鉴配方页和合成助手预览里的渲染方式。自定义配方类型需要注册 Descriptor：

```csharp
[RecipeDescriptor(new[] { typeof(MyRecipe) }, order: 0)]
public sealed class MyRecipeDescriptor : RecipeDescriptor {
    // 构建配方展示 Widget。
}
```

同一配方类型存在多个 Descriptor 时：

1. `order` 更高者优先。
2. `order` 相同时，类名字典序靠后的覆盖靠前的。

## 搜索扩展

搜索索引和过滤由 REX 核心维护。依赖模组若有自定义条目，应为条目提供清晰的显示名、描述和必要的搜索元数据，让玩家能通过名称、拼音、来源和配方关系找到它们。

玩家向搜索语法见 [图鉴搜索](../user/recipaedia-search.md)；内部搜索设计背景见 [图鉴搜索策划](../internal/plans/recipaedia-search-plan.md)。
