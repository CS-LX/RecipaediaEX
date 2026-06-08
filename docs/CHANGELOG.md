# RecipaediaEX 更新日志

面向**依赖本模组**的 Survivalcraft 模组开发者。版本号以 `modinfo.json` 的 `Version` 为准。

## 如何阅读

每条版本下分四类（无内容可省略该节）：

| 章节 | 含义 |
|------|------|
| **新增** | 新 API、新事件、新扩展点 |
| **修复** | 行为错误或文档/实现不一致的修正 |
| **变更** | 已有 API 签名、语义或默认行为的变化（含破坏性变更） |
| **适配指南** | 从上一标注版本升级时，建议修改的代码与 `modinfo` 依赖写法 |

**破坏性变更**在条目前标注 `⚠️`。

---

<!-- 新版本请复制下方「版本块」模板，插入到本文件最上方（最新版本在上）。 -->

<!--
## [x.y.z] — YYYY-MM-DD

**对比基准：** `a.b.c-旧版`（提交 `xxxxxxxx`）→ `a.b.c-新版`（提交 `yyyyyyyy`）

### 新增
- …

### 修复
- …

### 变更
- …

### 适配指南（从 x.y.z-旧版 升级）
1. …
-->

---

## [Unreleased]

### 新增

- **图鉴搜索 Phase 1**：`RecipaediaSearchIndex` / `Parser` / `Engine`；`RecipaediaEXScreen` 社区同款搜索行；`RecipaediaSearchFilterDialog`（滚动表单、`TextBoxArea` 衬底、固定底栏按钮）；`Assets/Lang` 语言包（键位于 `ContentWidgets`）。
- **图鉴搜索 Phase 2（核心）**：拼音索引（`NPinyin.Core`）；`or` / `()` 查询 AST；`@recipes>=N` 等比较；搜索历史（`RecipaediaEXSearchHistory.txt` + 历史图标按钮）。
- **图鉴搜索栏 UI**：搜索 / 历史 / 筛选改为 Bevelled 图标按钮（`ButtonStyle_Search|History|Filter` + 模组纹理）。

### 修复

- 排除词 `-keyword` / Dialog「排除词」双重取反导致结果反转。
- 外部 PNG 图标在 `RectangleWidget` 上默认 `AlphaBlend` 导致白色抗锯齿光晕；图标样式改用 `BlendState=NonPremultiplied`（对齐宿主 `MainMenuScreen`）。

### 变更

- ⚠️ **移除** `RecipeExtraKeys.MatchedResultFluidValues`。REX 核心仅约定方块产物/原料 Extra；其它产物语义（如流体）由依赖模组自定键名，并通过 `IRecipaediaRecipeItem` / `IRecipaediaSearchContributor` 扩展。
- 图鉴搜索：移除 `ItemSearchKind.Fluid`、`ResultFluidValues`、`@t:fluid` 及类名推断流体逻辑。

### 适配指南

1. 若模组曾使用 `RecipeExtraKeys.MatchedResultFluidValues`，改为模组内自有常量（如 `MyRecipeExtraKeys.MatchedResultFluidValues`），并在 `FluidItem.Match` 等处引用该常量。
2. 搜索 `@t:fluid` 改为 `@t:custom` 或由 `IRecipaediaSearchContributor` 注册标签。

---

## [2.0.0.0-preview5] — 2026-05-20

**对比基准：** `2.0.0.0-preview2`（提交 [`cc765e3`](https://github.com/CS-LX/RecipaediaEX/commit/cc765e3)）→ `2.0.0.0-preview5`（提交 [`b71e6a0`](https://github.com/CS-LX/RecipaediaEX/commit/b71e6a0)，Git 标签 `preivew5`）

> 中间曾打标签 `preview4`（[`8733ec1`](https://github.com/CS-LX/RecipaediaEX/commit/8733ec1)），对应「事件中心」首次合入；preview5 在其基础上继续扩展事件与配方约定。

### 新增

- **`RecipaediaEX.Events` 事件总线**（`RecipaediaEventBus`、`EventChannel<T>`、`IPublisher<T>` / `ISubscriber<T>`）
  - 其它模组可 `GetPublisher<T>` / `GetSubscriber<T>` 发布或订阅**自定义事件类型**，无需改 RX 源码。
  - 内置事件（均提供 `RecipaediaEventBus.*` 便捷订阅属性，详见 [API 文档 · 事件总线](API使用文档.md#25-recipaediaeventbus)）：
    - `RecipesResetEvent` — 静态配方总表 `ResetRecipes()` 完成
    - `RecipeMatchedEvent` — `FindMatchingRecipe` / 动态链匹配成功
    - `CraftingRecipeChangedEvent` — 扩展工作台预览配方变化
    - `SmeltingRecipeChangedEvent` — 扩展熔炉激活冶炼配方变化
    - `CrafterOutputProducedEvent` — 熔炉冶炼完成并写入产物格
    - `CrafterOutputRemovedEvent` — 工作台/熔炉从产物格成功取出
    - `FurnaceFuelUsedEvent` — 熔炉成功消耗燃料
- **动态配方链** — `IDynamicRecipeLoader`、`RecipesLoadManager.DynamicRecipeLoaders`、`RecipaediaEXManager.FindDynamicRecipe`
  - 内置 `AdHocRecipeLoader`：对接原版 `Block.GetAdHocCraftingRecipe`（`Order = 0`）。
  - `FindMatchingRecipe<T>(actual)` 在 `actual` 含 `Project` Extra 时**先**走动态链，再查静态表。
- **`RecipeExtraKeys`** — `IRecipe` Extra 约定键名常量（`MatchedResultBlockValues`、`MatchedIngredientBlockValues`、`Project`、`ActualIngredients`、`Inventory`）。
- **图鉴原料匹配** — `FormattedRecipe.PreTransformIngredients()` 结束时自动写入 `MatchedIngredientBlockValues`；`BlockItem.IsIngredient` 优先读该 Extra。

### 修复

- 图鉴「原料」条目对未手写原料 Extra 的 `FormattedRecipe`，现可通过自动填充的 `MatchedIngredientBlockValues` 正确匹配（仍保留 `CompareIngredients` 回退）。
- 扩展工作台/熔炉与图鉴侧 AdHoc 配方：由统一的动态 Loader 链处理，避免各模组重复遍历方块。

### 变更

- ⚠️ **移除** `OriginalComponentsExtensions.FindCraftingRecipe<T>(SubsystemTerrain, T)`。AdHoc 改由 `AdHocRecipeLoader` + `FindMatchingRecipe<T>` 提供；调用方须在 `actual` 上设置 `RecipeExtraKeys.Project`（扩展台/炉组件已内置）。
- ⚠️ **移除** `FormattedRecipe.MatchedResultBlockValuesKey`、`FormattedRecipe.MatchedIngredientBlockValuesKey`。请改用 `RecipeExtraKeys` 中同名常量（字符串值未变，仅入口迁移）。
- `RecipaediaEventBus`：由仅暴露 `CrafterOutputRemoved` 扩展为多种内置事件；`CrafterOutputRemoved` 仍保留，语义不变。
- `IRecipe` 文档注释：Extra 键名统一指向 `RecipeExtraKeys`。
- `modinfo.json`：`Version` 由 `2.0.0.0-preview2` 升为 `2.0.0.0-preview5`（`ApiVersion` 仍为 `1.9.0.2`）。

### 适配指南（从 preview2 升级）

1. **依赖版本** — 在己方 `modinfo.json` 的 `Dependencies` 中将 `com.recipaediaex` 改为 `2.0.0.0-preview5`（或与之兼容的更高 preview/正式版）。

2. **Extra 键名** — 将手写字符串或 `FormattedRecipe.*Key` 全部替换为 `RecipeExtraKeys`：
   ```csharp
   // 旧（preview2）
   recipe.SetExtraValue("MatchedResultBlockValues", blockValues);
   recipe.SetExtraValue(FormattedRecipe.MatchedIngredientBlockValuesKey, values);

   // 新（preview5）
   recipe.SetExtraValue(RecipeExtraKeys.MatchedResultBlockValues, blockValues);
   recipe.SetExtraValue(RecipeExtraKeys.MatchedIngredientBlockValues, values);
   ```

3. **AdHoc / 临时配方** — 若曾调用 `FindCraftingRecipe<T>(subsystemTerrain, actual)`：
   ```csharp
   actual.SetExtraValue(RecipeExtraKeys.Project, project);
   T recipe = RecipaediaEXManager.FindMatchingRecipe<T>(actual);
   ```
   自定义机器需要 AdHoc 时，务必写入 `Project`；可选实现自有 `IDynamicRecipeLoader`（`Order` 大于 `0` 时在 AdHoc 之后执行）。

4. **图鉴原料** — 若 Loader 已调用 `PreTransformIngredients()`，一般**无需**再手写 `MatchedIngredientBlockValues`；若自定义 `IRecipe` 非 `FormattedRecipe`，请在 `Match`/`IsIngredient` 路径自行设置 `RecipeExtraKeys.MatchedIngredientBlockValues`。

5. **事件订阅（可选）** — 在 `ModLoader` 初始化或世界就绪后订阅，世界/模组卸载时 `Dispose` 订阅句柄：
   ```csharp
   using RecipaediaEX.Events;

   IDisposable sub = RecipaediaEventBus.RecipesReset.Subscribe(e => OnRecipesReset(e.RecipeCount));
   // 不再需要时 sub.Dispose();
   ```

6. **无需改动的常见情况** — 仅使用 `IRecipesLoader` 注册静态配方、仅继承 `ComponentEXCraftingTable` / `ComponentEXFurnace` 且调用 `base`、仅通过 `ICrafter` 供图鉴工作站映射：通常可直接换依赖版本编译运行。

---

## [2.0.0.0-preview2] — 基准说明

**提交基准：** [`cc765e3`](https://github.com/CS-LX/RecipaediaEX/commit/cc765e3)（`ApiVersion` `1.9.0.2`）

该版本已包含（相对更早的 2.0 开发线）：`IRecipesLoader` 统一加载、`ActualIngredients` / `Inventory` Extra、扩展工作台/熔炉 6×6、`ICrafter` 传入配方实例、`MatchPriority`、熔炉逻辑拆分等。自 preview2 起至 preview5 的**增量**见上一节。

---

## 链接

- [API 使用文档](API使用文档.md)
- [README](../README.md)
