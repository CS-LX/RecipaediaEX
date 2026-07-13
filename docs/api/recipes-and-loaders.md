# 配方与加载器

本页说明依赖模组如何把自定义配方接入 RecipaediaEX 的匹配和图鉴体系。完整接口细节见 [旧版 API 汇总](../API使用文档.md#2-逻辑层-api)。

## 核心类型

| 类型 | 职责 |
|------|------|
| `IRecipe` | 运行时配方对象，负责匹配和扩展数据 |
| `RecipeExtraKeys` | REX 约定的 Extra 键名集合 |
| `IRecipesLoader` | 提供静态配方集合 |
| `IDynamicRecipeLoader` | 按世界或运行时状态临时生成配方 |
| `RecipaediaEXManager` | 配方总表、动态匹配和查询入口 |

## `IRecipe` 最小要求

自定义配方至少需要实现：

- `DisplayOrder`：图鉴排序。
- `MatchPriority`：匹配优先级。
- `Match(IRecipe actual)`：判断“实际输入”是否匹配该配方。
- `GetExtraValue<T>` / `SetExtraValue<T>`：读写扩展数据。

推荐在配方内部维护 `ValuesDictionary`，并使用 `RecipeExtraKeys` 常量，不要手写字符串。

常用 Extra：

| 键 | 用途 |
|----|------|
| `MatchedResultBlockValues` | 图鉴按产物匹配条目 |
| `MatchedIngredientBlockValues` | 图鉴按原料匹配条目 |
| `Project` | 触发动态配方链 |
| `ActualIngredients` | 当前槽位快照 |
| `Inventory` | 发起匹配时的库存引用 |

## 静态加载器

实现 `IRecipesLoader` 后，REX 会通过反射自动发现：

```csharp
public sealed class MyRecipesLoader : IRecipesLoader {
    public int Order => 100;

    public void Initialize() {
        // 扫描文件、缓存索引或准备数据。
    }

    public IEnumerable<IRecipe> GetRecipes() {
        return m_recipes;
    }
}
```

`Order` 数值越大，排序越靠后。Loader 实现类放在已加载程序集里即可，不需要 xdb 注册。

## 动态配方

需要读取世界状态、玩家状态或原版 AdHoc 配方时，实现 `IDynamicRecipeLoader`。调用方需要在 `actual` 上写入 `RecipeExtraKeys.Project`，再使用泛型匹配入口：

```csharp
actual.SetExtraValue(RecipeExtraKeys.Project, project);
MyRecipe recipe = RecipaediaEXManager.FindMatchingRecipe<MyRecipe>(actual);
```

非泛型 `FindMatchingRecipe(actual)` 只查静态配方总表，不走动态 Loader。

## 推荐接入顺序

1. 定义配方数据类并实现 `IRecipe.Match`。
2. 写 `IRecipesLoader` 提供配方列表。
3. 为图鉴产物和原料写入 `MatchedResultBlockValues` / `MatchedIngredientBlockValues`。
4. 在机器组件中构造 `actual`，调用 `RecipaediaEXManager.FindMatchingRecipe<T>`。
5. 如需 AdHoc 或世界状态配方，再补 `IDynamicRecipeLoader`。

## 常见坑

- 不写 `MatchedResultBlockValues` 时，图鉴按产物跳转可能找不到你的条目。
- 需要动态配方但没有写 `Project` 时，泛型匹配也不会进入动态链。
- `IRecipesLoader` 的 `Initialize()` 适合做缓存准备，不建议在 `GetRecipes()` 里重复重 IO。
