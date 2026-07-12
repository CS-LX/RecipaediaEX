# RecipaediaEX

RecipaediaEX 是一个面向 Survivalcraft 模组开发的配方与图鉴扩展框架，核心目标是把“配方定义”“配方加载”“图鉴展示”解耦，让你可以：

- 自定义任意 `IRecipe` 类型，不再局限原版 `CraftingRecipe`。
- 通过 `IRecipesLoader` 读取任意来源的配方（文件、程序生成、混合方式）。
- 使用统一的 `RecipaediaEXManager` 进行匹配查询。
- 在图鉴界面通过 `RecipeDescriptor` 按配方类型渲染 UI。

## 版本与依赖

- 当前项目版本：`2.0.0.0`（以 `modinfo.json` 为准）
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

建议在配方里维护 `ValuesDictionary`，键名使用 **`RecipeExtraKeys`**（`RecipesExtra/RecipeExtraKeys.cs`），并至少写入：

- `RecipeExtraKeys.MatchedResultBlockValues`（`int[]`），供图鉴方块产物匹配；
- 按需 `MatchedIngredientBlockValues`。

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
- `RecipaediaEXManager.FindMatchingRecipe<T>(actual)` — 若 `actual` 含 `RecipeExtraKeys.Project`，会先走动态配方（`IDynamicRecipeLoader`），再查静态表。
- `RecipaediaEXManager.FindDynamicRecipe(actual, project)` — 仅查动态配方，不查静态表。
- `RecipaediaEXManager.FindMatchingRecipes(actual)`

扩展工作台/熔炉在构造 `actual` 时会写入 `RecipeExtraKeys.Project` 等 Extra，因此 `FindMatchingRecipe<T>` 可自动解析原版 AdHoc 配方。自定义机器若需 AdHoc，请同样在 `actual` 上设置 `Project`。

### 3.1 动态配方（AdHoc，可选）

实现 `IDynamicRecipeLoader` 以对接原版 `Block.GetAdHocCraftingRecipe` 等运行时生成逻辑。框架内置 `AdHocRecipeLoader`（`Order = 0`）。详见 [API 使用文档 · 动态配方](docs/API使用文档.md#24-动态配方idynamicrecipeloader--dynamicloaders)。

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

## 事件总线（其它模组订阅）

RecipaediaEX 提供 `RecipaediaEX.Events.RecipaediaEventBus`：按事件类型维护独立通道，**无需修改 RX 源码**即可订阅框架生命周期与工作台/熔炉行为。

```csharp
using RecipaediaEX.Events;

// 内置事件：配方表重建后刷新本地缓存
RecipaediaEventBus.RecipesReset.Subscribe(e => RebuildMyRecipeIndex(e.RecipeCount));

// 自定义事件类型（跨模组约定载荷 struct/class 即可）
RecipaediaEventBus.GetPublisher<MyModEvent>().Publish(new MyModEvent(...));
```

内置事件一览见 [API 使用文档 · 事件总线](docs/API使用文档.md#25-recipaediaeventbus)。

## 文档导航

- [更新日志（模组开发者）](docs/CHANGELOG.md)
- [API 使用文档](docs/API使用文档.md)
- [图鉴搜索功能策划](docs/图鉴搜索功能策划.md)
- [合成助手策划（Crafting Overlay）](docs/工作台悬浮助手策划.md)
- [合成助手 · JEI 对标与基元语句](docs/合成助手-JEI对标与基元语句.md)
- [路线图 · ResolveBus 与原料贡献（2.0 之后）](docs/路线图-ResolveBus与原料贡献.md)
- [打包 → 发布 CI 策划](docs/打包发布CI策划.md)
- [发版指南](docs/RELEASE.md)

## 构建与打包

版本号以 **`modinfo.json` → `Version`** 为准；构建前 `tools/sync-version.ps1` 自动同步到 csproj。

### 本机开发

1. 复制 `tools/pack.config.example.json` 为 `tools/pack.config.json`，填写 `ModsFolder`（游戏 Mods 目录）。
2. 在仓库根目录执行 `dotnet build RecipaediaEX.csproj -c Release`（或 Debug）。
3. 构建结束后 `tools/pack.ps1` 自动将输出目录打成 **`RecipaediaEX.scmod`**（短名）并复制到 `ModsFolder`。

若 monorepo 内与同仓宿主模组联合开发，打包脚本可复用宿主提供的 `7z`；独立 clone 本仓库时回退为 PowerShell `Compress-Archive`。

跳过自动打包：`-p:RecipaediaEXSkipPack=true`。跳过版本同步：`-p:RecipaediaEXSkipSyncVersion=true`。

### GitHub CI

| 工作流 | 触发 | 产物 |
|--------|------|------|
| `build.yml` | push main、PR | `RecipaediaEX-ci.{sha7}.scmod` |
| `release.yml` | 推送 tag `v*` | GitHub Release + 模组站 post 1739：`RecipaediaEX-{Version}.scmod` |

发版步骤见 [docs/RELEASE.md](docs/RELEASE.md)。Release 还需在 GitHub 配置 Secret **`MOD_SITE_TOKEN`**（模组站 Bearer Token）。RecipaediaEX 无宿主模组编译期依赖，CI 仅需 checkout 本仓库。

## 说明

- `RecipeReaderAttribute` / `RecipeFileLoaderAttribute` 当前在框架核心流程中不是必需入口，主要作为扩展约定与兼容保留。
- 若你在自定义生态中使用自己的 Reader 体系（例如在自定义 Loader 内按 `Reader` 字段分发），这是推荐做法。