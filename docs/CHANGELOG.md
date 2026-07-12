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

## 计划中（尚未发版）

- **ResolveBus / 原料贡献**：`2.0.0.0` 之后路线图见 [路线图-ResolveBus与原料贡献.md](路线图-ResolveBus与原料贡献.md)（同族自定义返回值通道；删除 `CraftingOverlayIngredientBridge`）。

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

（暂无）

---

## [2.0.0.0] — 2026-07-12

**对比基准：** `2.0.0.0-preview8`（提交 [`f0258e7`](https://github.com/CS-LX/RecipaediaEX/commit/f0258e7)，Git 标签 `preview8`）→ `2.0.0.0`（Git 标签 `v2.0.0.0`）

> **2.0 首个正式版。** 自 preview5 起的配方事件总线、图鉴搜索 Phase 1/2、合成助手 Phase 1～2b 与 preview6～8 社区反馈收口（W1/W4/W4.5/W5）均已包含在 preview 链中；本版在 preview8 基础上新增 **W2 长按连放**、**W3 熔炼输入区 `+`** 与 **RecipaediaInterceptBus**。  
> **已知限制（计划 2.0.x 后续）：** 合成助手 **Phase 4c（W6）材料推荐 / 原料反查** 暂缓，待 JEI 对齐方案评审后再做。

### 新增

- **`RecipaediaInterceptBus`**：`InterceptChannel<T>` + `IInterceptPublisher` / `IInterceptSubscriber`；订阅方 `return false` 否决操作。详见 [API 文档 · 拦截总线](API使用文档.md#25-recipaediaeventbus)。
- **P0 生产拦截**：`CrafterOutputRemoving`、`CrafterOutputProducing`、`FurnaceFuelConsuming`、`RecipePlacementPlanBuilding`、`RecipePlacementExecuting`（已挂接扩展工作台/熔炉与 `FormattedGridPlacementPlanner`）。
- **P1 助手拦截**：`CraftingOverlayOpening` / `Closing`、`OpenFullRecipaediaNavigating`、`OverlaySearchApplying`、`OverlayRecipePreviewShowing`；系统生命周期 teardown 走 `DismissSilently()`（不触发 `CraftingOverlayClosing`）。
- **合成助手 W3（Phase 2c）**：熔炼输入区 `+` — `FormattedGridPlacementPlanner`、`FurnacePlacementTarget`（`OriginalSmeltingRecipe`）；`RecipaediaFurnaceWidget` + Loader 可替换原版 `FurnaceWidget`。
- **合成助手 W2**：每卡 `+` **长按连续放置** — `PlacementLongPressRepeater`（连放 1 秒内累计 40 组，二次缓入由慢至快）；有形合成格 **广度优先** 每轮 +1（JEI 式多组 pattern）。
- 绿色 `+` 悬停提示「长按可连续放置」（`RecipaediaCraftingOverlay:14`）。
- **文档**：`API使用文档.md` 重组 §2.5 事件/拦截总线、上下文字段表、`DismissSilently` 边界、§10 合成助手 API、拦截 FAQ 与示例。

### 变更

- `IRecipaediaOverlayDescriptorHost.PlaceRecipe` 返回 `bool`；新增 `clearGridBeforePlace`、`showFeedback` 参数（连放调用方使用）。
- ⚠️ **`CraftingGridPlacementPlanner` 移除** — 工作台与熔炉统一走 **`FormattedGridPlacementPlanner`**。
- ⚠️ **`PlacableRecipeAdapter.TryAsPlacable` / `FormattedGridPlacableRecipe` 移除** — 改为 **`IsPlacable(IRecipe)`** 判定可否显示 `+`；实际摆放仍由 **`IRecipePlacementTarget.TryPlaceRecipe`** 执行。

### 适配指南（从 `2.0.0.0-preview8` 升级）

1. **依赖版本** — `modinfo` 中 `com.recipaediaex` 改为 **`2.0.0.0`**（或 `>=2.0.0.0`）。
2. **Placement 规划器** — 删除对 `CraftingGridPlacementPlanner` 的引用；有形合成与熔炼输入区改用 `FormattedGridPlacementPlanner`。
3. **Placable 判定** — `PlacableRecipeAdapter.TryAsPlacable(...)` 改为 `PlacableRecipeAdapter.IsPlacable(recipe)`；自定义 `IPlacableRecipe` 包装类若仅用于 UI 门控，可移除，保留 `IRecipePlacementTarget` 即可。
4. **熔炼炉 Host** — 实现 `IRecipaediaOverlayHost`，`GetPlacementTarget()` 返回 `new FurnacePlacementTarget(furnace)`；`GetCraftingContext()` 可用内容模组提供的 `BuildCrafterContext`。
5. **长按连放** — 自定义 `IRecipePlacementTarget` 时，连放后续次调用 **`clearGridBeforePlace: false`**，勿每次 `TryPlaceRecipe` 清空输入槽；单槽灌满需支持「已达配方数量且未满堆叠上限时继续 +1」。
6. **拦截总线（可选）** — 在 `ModLoader` 初始化或世界就绪后订阅 `RecipaediaInterceptBus.*`；卸载时 `Dispose` 订阅句柄。

### 适配指南（从任意 preview 直跳 stable）

1. 若仍依赖 **`2.0.0.0-preview5` 之前**：请先阅读本文件 **preview5 → preview6 → preview7 → preview8** 各节的 ⚠️ 变更（`RecipeDescriptor` 构造、`RecipeExtraKeys`、`Close()` 移除等）。
2. 合成助手 Overlay 协议见 [工作台悬浮助手策划.md](工作台悬浮助手策划.md)；内容模组 Placement 接入见 §10 与策划 §6（各依赖模组在其仓库维护 Host 清单）。

---

## [2.0.0.0-preview8] — 2026-06-25

**对比基准：** `2.0.0.0-preview7`（提交 [`8abafba`](https://github.com/CS-LX/RecipaediaEX/commit/8abafba)）→ `2.0.0.0-preview8`（提交 [`f0258e7`](https://github.com/CS-LX/RecipaediaEX/commit/f0258e7)，Git 标签 `preview8`）

### 新增

- **合成助手 Phase 4b（W5）**：有搜索词时 Overlay **全库 Filter**（`RecipaediaCategoryCatalog.GetOverlayGlobalSearchCandidates`：All Blocks + 非方块分类条目）；Category ◀▶ 仅影响无 query 浏览；搜索模式下分类条显示「全库搜索」且禁用 ◀▶（D27 / F6）。

### 变更

- 合成助手换分类 **不再清空** 搜索词（Q4b-1：有 query 时换分类不改列表）。
- Overlay 搜索 placeholder 改为「搜索全库条目…」（`RecipaediaCraftingOverlay:13`）。

### 适配指南（从 preview7 升级）

1. **依赖版本** — `modinfo` 中 `com.recipaediaex` 改为 `2.0.0.0-preview8`。
2. 全屏图鉴搜索语义 **不变**（D17）；仅合成助手 Overlay 有 query 时全库搜。

---

## [2.0.0.0-preview7] — 2026-06-25

**对比基准：** `2.0.0.0-preview6`（提交 [`2b85f5d`](https://github.com/CS-LX/RecipaediaEX/commit/2b85f5d)）→ `2.0.0.0-preview7`（提交 [`8abafba`](https://github.com/CS-LX/RecipaediaEX/commit/8abafba)，Git 标签 `preview7`）

> 反馈期首轮：**W1 / W4 / W4.5**（Hide 不 Destroy、Modal 生命周期、默认 All + debounce 输入即搜）。

### 新增

- **合成助手 Phase 4a（W4）**：`RecipaediaCraftingOverlayController.DismissForModalWidget` — Host Modal 关闭/替换时销毁助手；`TryGetOverlayHost` 与角标门控要求 `GetCraftingContext() != null`。
- **合成助手 Phase 4d（W4.5）**：Overlay 搜索框 **250ms debounce 自动过滤**（Enter / 放大镜仍立即提交并写入历史）；无会话记忆时默认浏览 **All Blocks**（D32）。

### 变更

- **合成助手 Phase 2a.3（W1）**：toggle 关闭改为 **`Hide()`**（保留 Dialog 实例与会话态）；Host 销毁仍 **`Dismiss()`** Remove Widget。
- ⚠️ **移除** `RecipaediaCraftingOverlayController.Close()` — 请改用 `Hide()`（toggle）或 `Dismiss()` / `DismissForModalWidget()`（Host 关闭）。
- `DefaultOverlayCategoryId` 改为返回 **All Blocks**（废止 Phase 1.6「默认首个非 All」）。

### 适配指南（从 preview6 升级）

1. **依赖版本** — `modinfo` 中 `com.recipaediaex` 改为 `2.0.0.0-preview7`。
2. **Modal 生命周期** — 若内容模组在 `OnModalPanelWidgetSet` 等处手动关闭合成助手，请改调 `RecipaediaCraftingOverlayController.DismissForModalWidget(oldModal)`（或 `Dismiss()`），勿再调用已移除的 `Close()`。

---

## [2.0.0.0-preview6] — 2026-06-19

**对比基准：** `2.0.0.0-preview5`（提交 [`b71e6a0`](https://github.com/CS-LX/RecipaediaEX/commit/b71e6a0)）→ `2.0.0.0-preview6`（提交 [`2b85f5d`](https://github.com/CS-LX/RecipaediaEX/commit/2b85f5d)，Git 标签 `preview6`）

> 本版为 **合成助手 Pre-release**：在 preview5 事件总线与动态配方之上，交付图鉴搜索、工作台悬浮助手（Phase 1～2b）、搜索性能优化与会话体验打磨（D24/D25）。依赖模组须自行对齐 `modinfo` 依赖并发布配套内容包。

### 新增

- **合成助手 Phase 1**：`RecipaediaCraftingOverlayDialog`（右侧条带 + JEI 式二级配方弹窗）；`IRecipaediaOverlayHost` / `RecipaediaCraftingContext`；`IRecipaediaRecipeNavigator` + `RecipeDescriptorRegistry`；`Recipaedia` 键 Hook（合成 Modal toggle / EventBus 全屏图鉴）；`OpenFullRecipaediaRequestedEvent`。
- **合成助手 Phase 2a**：`IPlacableRecipe` / `PlacableRecipeAdapter`；`IRecipePlacementTarget` + `CraftingGridPlacementPlanner`；Descriptor 操作条 `+` / `★`；`RecipaediaCraftingOverlaySessionState`（分类、滚动记忆）。
- **合成助手 Phase 2b**：`CraftingOverlayIngredientBridge`（背包 ↔ 合成格原料桥接）；内容模组 Host 通过 `GetPlacementTarget()` 接入 `+` 摆放。
- **图鉴搜索 Phase 1**：`RecipaediaSearchIndex` / `Parser` / `Engine`；`RecipaediaEXScreen` 搜索行；`RecipaediaSearchFilterDialog`；`Assets/Lang` 语言包。
- **图鉴搜索 Phase 2（核心）**：拼音索引（`NPinyin.Core`）；`or` / `()` 查询 AST；`@recipes>=N` 等比较；搜索历史（`RecipaediaEXSearchHistory.txt` + 历史图标按钮）。
- **图鉴搜索栏 UI**：搜索 / 历史 / 筛选 Bevelled 图标按钮（`ButtonStyle_Search|History|Filter` + 模组纹理）。
- **搜索性能（P0-PR-01）**：`RecipeSearchMetadata` 惰性配方元数据；纯文本 `MightMatchPlainText` 预筛；`(categoryId, query)` 结果缓存。
- **会话体验（D24/D25）**：`SessionState.SearchQuery` toggle 恢复；灰显 `+` 悬停显示 `disabledReason`（`RecipaediaOverlayDescriptorActionBar`）。

### 修复

- 排除词 `-keyword` / Dialog「排除词」双重取反导致结果反转。
- 外部 PNG 图标 `AlphaBlend` 白色光晕；图标样式改用 `BlendState=NonPremultiplied`。
- Phase 2b 验收期：工作台 / One2One 缺料文案、摆放性能与布局早退。
- **TD-01**：`FluidItem` 接入 `IRecipaediaDescriptionItem`；删除临时 `IRecipaediaNamedItem`；流体详情 Esc 返回导航。

### 变更

- ⚠️ **`RecipeDescriptor` 构造签名**：由 `RecipaediaEXRecipesScreen` 改为 `IRecipaediaRecipeNavigator`；依赖模组中所有 `[RecipeDescriptor]` 实现须同步迁移。
- ⚠️ **移除** `RecipeExtraKeys.MatchedResultFluidValues`。流体等扩展产物语义由依赖模组自定 Extra 键，并通过 `IRecipaediaRecipeItem` / `IRecipaediaSearchContributor` 扩展。
- 图鉴搜索：移除 `ItemSearchKind.Fluid`、`ResultFluidValues`、`@t:fluid` 及类名推断流体逻辑。

### 适配指南（从 preview5 升级）

1. **依赖版本** — 在己方 `modinfo.json` 的 `Dependencies` 中将 `com.recipaediaex` 改为 `2.0.0.0-preview6`（或与之兼容的更高 preview/正式版）。
2. **`RecipeDescriptor`** — 构造函数首参改为 `IRecipaediaRecipeNavigator`（原 `RecipaediaEXRecipesScreen`）。
3. **流体 Extra** — 若曾使用 `RecipeExtraKeys.MatchedResultFluidValues`，改为模组内自有常量，并在 `IRecipaediaSearchContributor` 等处注册。
4. **专有机器摆放** — 实现 `IRecipaediaOverlayHost.GetPlacementTarget()` 与 `IPlacableRecipe` 适配；步骤见 API §10 与策划 §6。
5. **合成助手订阅（可选）** — 事件与拦截见 [API 文档 · 事件与扩展总线](API使用文档.md#25-事件与扩展总线)；合成助手 Host 见 [§10](API使用文档.md#10-合成助手crafting-overlay)。

---

## [2.0.0.0-preview5] — 2026-05-20

**对比基准：** `2.0.0.0-preview2`（提交 [`cc765e3`](https://github.com/CS-LX/RecipaediaEX/commit/cc765e3)）→ `2.0.0.0-preview5`（提交 [`b71e6a0`](https://github.com/CS-LX/RecipaediaEX/commit/b71e6a0)，Git 标签 `preivew5`）

> 中间曾打标签 `preview4`（[`8733ec1`](https://github.com/CS-LX/RecipaediaEX/commit/8733ec1)），对应「事件中心」首次合入；preview5 在其基础上继续扩展事件与配方约定。

### 新增

- **`RecipaediaEX.Events` 事件总线**（`RecipaediaEventBus`、`EventChannel<T>`、`IPublisher<T>` / `ISubscriber<T>`）
  - 其它模组可 `GetPublisher<T>` / `GetSubscriber<T>` 发布或订阅**自定义事件类型**，无需改 RX 源码。
  - 内置事件（均提供 `RecipaediaEventBus.*` 便捷订阅属性，详见 [API 文档 · 事件与扩展总线](API使用文档.md#25-事件与扩展总线)）：
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
