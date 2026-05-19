# RecipaediaEX

RecipaediaEX 是一个面向 Survivalcraft 模组开发的配方与图鉴扩展框架，核心目标是把“配方定义”“配方加载”“图鉴展示”解耦，让你可以：

- 自定义任意 `IRecipe` 类型，不再局限原版 `CraftingRecipe`。
- 通过 `IRecipesLoader` 读取任意来源的配方（文件、程序生成、混合方式）。
- 使用统一的 `RecipaediaEXManager` 进行匹配查询。
- 在图鉴界面通过 `RecipeDescriptor` 按配方类型渲染 UI。

## 版本与依赖

- 当前项目版本：`2.0.0.0`
- 目标框架：`net10.0`
- 主要依赖：
    - `SurvivalcraftAPI.Engine`
    - `SurvivalcraftAPI.EntitySystem`
    - `SurvivalcraftAPI.Survivalcraft`
    - `ZLinq`

## 运行流程（框架内部）

`RecipaediaEXLoader` 在 `OnLoadingFinished` 阶段执行：

1. `RecipesLoadManager.Initialize()`：扫描并实例化全部 `IRecipesLoader` 与 `IDynamicRecipeLoader`（按 `Order` 排序）。
2. `RecipaediaEXManager.Initialize()`：调用各 `IRecipesLoader` 的 `Initialize()` / `GetRecipes()`，建立静态配方总表。
3. `RecipesCrafterManager.Initialize()`：扫描方块上的 `ICrafter`，建立“配方 -> 可用工作站”映射。
4. 注入图鉴页面：
    - `Recipaedia`
    - `RecipaediaDescription`
    - `RecipaediaRecipes`

之后在：

- `CraftingRecipesManagerInitialized` 钩子中执行 `RecipaediaEXManager.ResetRecipes()`。
- `BlocksInitalized` 钩子中执行 `RecipesCrafterManager.Initialize()`。

## 快速开始

### 1. 定义配方类型

实现 `IRecipe`，最少需要：

- `DisplayOrder`
- `MatchPriority`
- `Match(IRecipe actual)`
- `GetExtraValue/SetExtraValue`

建议在配方里维护 `ValuesDictionary`，并至少写入：

- `MatchedResultBlockValues`（`int[]`），供图鉴条目匹配使用。

### 2. 提供配方加载器

实现 `IRecipesLoader`：

- `Initialize()`：做一次性的扫描、缓存、索引构建。
- `GetRecipes()`：返回配方序列。
- `Order`：加载优先级（数值越大，排序越靠后）。

项目内可参考：

- `SurvivalcraftRecipesLoader`
- `BlockProceduralRecipesLoader`
- `CrXmlRecipesLoader`

### 3. 在工作站使用配方匹配

在你的组件中构造“实际配方”，再调用：

- `RecipaediaEXManager.FindMatchingRecipe(actual)` — 仅在静态配方总表中查找。
- `RecipaediaEXManager.FindMatchingRecipe<T>(actual)` — 若 `actual` 的 `ExtraValues` 含 `"Project"`，会先走动态配方（`IDynamicRecipeLoader`），再查静态表。
- `RecipaediaEXManager.FindDynamicRecipe(actual, project)` — 仅查动态配方，不查静态表。
- `RecipaediaEXManager.FindMatchingRecipes(actual)`

扩展工作台/熔炉在构造 `actual` 时会写入 `SetExtraValue("Project", Project)`，因此 `FindMatchingRecipe<T>` 可自动解析原版 AdHoc 配方。自定义机器若需 AdHoc，请同样在 `actual` 上设置 `Project`。

### 3.1 动态配方（AdHoc，可选）

实现 `IDynamicRecipeLoader` 以对接原版 `Block.GetAdHocCraftingRecipe` 等运行时生成逻辑。框架内置 `AdHocRecipeLoader`（`Order = 0`）。详见 [API 使用文档](docs/API使用文档.md#动态配方idynamicrecipeloader)。

### 4. 接入图鉴展示（可选）

- 条目实现 `IRecipaediaItem`。
- 若要在配方页展示，实现 `IRecipaediaRecipeItem`。
- 若要在详情页展示，实现 `IRecipaediaDescriptionItem`。
- 分类提供器实现 `IRecipaediaCategoryProvider`（必须无参构造）。

### 5. 自定义配方 UI（可选）

继承 `RecipeDescriptor` 并加：

- `[RecipeDescriptor(new[] { typeof(YourRecipe) }, order: 0)]`

`RecipaediaEXRecipesScreen` 会按 `recipe.GetType()` 映射到 Descriptor。若同一配方类型有多个 Descriptor，按：

1. `order` 高者优先
2. `order` 相同则类名字典序后的覆盖前者

## 文档导航

- [API 使用文档](docs/API使用文档.md)

## 说明

- `RecipeReaderAttribute` / `RecipeFileLoaderAttribute` 当前在框架核心流程中不是必需入口，主要作为扩展约定与兼容保留。
- 若你在自定义生态中使用自己的 Reader 体系（例如在自定义 Loader 内按 `Reader` 字段分发），这是推荐做法。