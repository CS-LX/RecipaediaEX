# RecipaediaEX API 使用文档

本文档基于当前 `Dependencies/RecipaediaEX` 源码整理，描述稳定可用的接口、扩展点和推荐实践。

## 1. 核心概念

- `IRecipe`：运行时配方对象，负责匹配逻辑与扩展数据。
- `IRecipesLoader`：配方来源提供器，负责初始化与返回配方集。
- `RecipaediaEXManager`：配方容器与匹配入口。
- `ICrafter` + `RecipesCrafterManager`：配方对应工作站（用于 UI 展示）。
- `IRecipaedia*`：图鉴条目、分类、详情、配方页接口。
- `RecipeDescriptor`：配方在 `RecipaediaEXRecipesScreen` 的渲染器。

---

## 2. 逻辑层 API

### 2.1 `IRecipe`

路径：`RecipesExtra/IRecipe.cs`

必须实现：

- `int DisplayOrder { get; }`：图鉴配方排序（越小越靠前）。
- `int MatchPriority { get; }`：匹配优先级（当前实现按升序）。
- `bool Match(IRecipe actual)`：与“实际配方”比较。
- `T GetExtraValue<T>(string key, T defaultValue)`
- `void SetExtraValue<T>(string key, T value)`

推荐约定：

- 在配方里维护 `ValuesDictionary`。
- 设置 `MatchedResultBlockValues`（`int[]`），用于图鉴条目匹配。

### 2.2 `IRecipesLoader`

路径：`LoaderExtra/IRecipesLoader.cs`

- `void Initialize()`：首次加载时调用，建议做扫描和缓存准备。
- `IEnumerable<IRecipe> GetRecipes()`：返回配方集合（首次加载和进入存档时会调用）。
- `int Order { get; }`：加载器优先级。

默认加载器示例：

- `SurvivalcraftRecipesLoader`（原版配方）
- `BlockProceduralRecipesLoader`（程序生成配方）
- `CrXmlRecipesLoader`（`.cr` 文件）

### 2.3 `RecipaediaEXManager`

路径：`RecipaediaEXManager.cs`

常用方法：

- `FindMatchingRecipe(IRecipe actual)`
- `FindMatchingRecipe<T>(IRecipe actual)`
- `TryFindMatchingRecipe<T>(IRecipe actual, out T recipe)`
- `FindMatchingRecipes(IRecipe actual)`
- `ResetRecipes()`（方块 ID 变化后重建）

---

## 3. 工作站（Crafter）API

### 3.1 `ICrafter`

路径：`CrafterExtra/ICrafter.cs`

```csharp
bool IsCrafter(int blockValue, IRecipe recipe);
```

说明：

- 用于告诉图鉴“某配方可在哪些方块上执行”。
- 不直接实现生产逻辑（生产逻辑仍在你的组件中）。

### 3.2 `RecipesCrafterManager`

路径：`RecipesCrafterManager.cs`

- `Crafters`：`Dictionary<IRecipe, List<int>>`
- `Initialize()`：扫描实现了 `ICrafter` 的方块并建立映射。

---

## 4. 图鉴 UI API

### 4.1 条目接口

- `IRecipaediaItem`：图鉴列表条目基接口
- `IRecipaediaDescriptionItem`：详情页数据
- `IRecipaediaRecipeItem`：配方页匹配条件

关键点：

- `IRecipaediaItem.RecipeScreenName` / `DetailScreenName` 决定跳转界面。
- `IRecipaediaRecipeItem.Match()` 决定某条目能看到哪些配方。

### 4.2 分类接口

- `IRecipaediaCategoryProvider`：返回分类集合（要求无参构造）
- `IRecipaediaCategory`：分类定义
- `IAdvancedCategory`：额外控制 `ListItemSize` 和 `ListDirection`

---

## 5. 配方展示 API（Descriptor）

### 5.1 `RecipeDescriptor`

路径：`ScreenExtra/RecipeDescriptor.cs`

必须实现：

- `Show(IRecipe recipe, string nameSuffix)`
- `Hide()`
- `GetCrafterButton(IRecipe recipe)`

### 5.2 `RecipeDescriptorAttribute`

路径：`ScreenExtra/RecipeDescriptorAttribute.cs`

```csharp
[RecipeDescriptor(new[] { typeof(YourRecipe) }, order: 0)]
```

选择规则（同一 `recipeType` 多个 Descriptor 时）：

1. `order` 更高者生效
2. `order` 相同按类名字典序，后者覆盖前者

注意：

- Descriptor 构造函数必须是 `ctor(RecipaediaEXRecipesScreen)`。
- `RecipaediaEXRecipesScreen` 会缓存 Descriptor 实例复用。

---

## 6. 推荐接入步骤

1. 定义你的 `IRecipe` 类型。
2. 定义 `IRecipesLoader`，返回该配方集合。
3. 在配方中写好 `MatchedResultBlockValues`。
4. 为要展示的条目实现 `IRecipaediaRecipeItem`。
5. 为配方类型实现 `RecipeDescriptor` 并加特性。
6. （可选）给工作站方块实现 `ICrafter`。

---

## 7. 示例骨架

### 7.1 自定义配方

```csharp
public class MyRecipe : IRecipe {
    public int DisplayOrder { get; init; }
    public int MatchPriority { get; init; }
    public int ResultValue { get; init; }
    public string Ingredient { get; init; } = string.Empty;
    readonly ValuesDictionary _extra = new();

    public bool Match(IRecipe actual) {
        if (actual is not MyRecipe a) return false;
        return CraftingRecipesManager.CompareIngredients(Ingredient, a.Ingredient);
    }

    public T GetExtraValue<T>(string key, T defaultValue) => _extra.GetValue(key, defaultValue);
    public void SetExtraValue<T>(string key, T value) => _extra.SetValue(key, value);
}
```

### 7.2 自定义加载器

```csharp
public class MyRecipesLoader : IRecipesLoader {
    readonly List<IRecipe> _recipes = [];
    public int Order => 100;
    public void Initialize() {
        // 扫描文件 / 构建索引
    }
    public IEnumerable<IRecipe> GetRecipes() => _recipes;
}
```

---

## 8. 常见问题

- **Q: 为什么图鉴里看不到我的配方？**  
  A: 先检查该配方是否设置了 `MatchedResultBlockValues`，以及条目的 `IRecipaediaRecipeItem.Match()` 是否命中。

- **Q: 为什么配方页显示了错误的 UI？**  
  A: 检查该配方类型是否有多个 `RecipeDescriptor`，确认 `order` 规则和构造函数签名。

- **Q: 我是否必须使用 `.cr`？**  
  A: 不必须。你可以实现自己的 `IRecipesLoader` 读取任意格式。

---

## 9. 兼容说明

- `RecipeReaderAttribute`、`RecipeFileLoaderAttribute` 在当前主流程中并非硬依赖入口。
- 建议通过 `IRecipesLoader` 作为主扩展入口，在 Loader 内部自行组织 Reader 分发策略。
