# 路线图 · ResolveBus 与原料贡献（2.0.0.0 之后）

> **状态**：计划（未实现）  
> **基线**：RecipaediaEX **`2.0.0.0` 正式版**（2026-07-12）  
> **目标版本**：建议落在 **`2.1.x`**（破坏性迁移可走弃用窗口；不强制与 2.0 补丁混发）  
> **范围**：REX 核心扩展总线；依赖模组迁移（删除 `CraftingOverlayIngredientBridge` 静态槽）

---

## 1. 背景与动机

`2.0.0.0` 已交付两条平行扩展通道：

| 总线 | 语义 | 多模 |
|------|------|------|
| `RecipaediaEventBus` | 事后**通知**（`Action<T>`） | ✅ 多订阅 fan-out |
| `RecipaediaInterceptBus` | 事前**否决**（`Func<T, bool>`，first-veto） | ✅ priority + 多订阅 |

另有 Phase 2b 捷径 **`CraftingOverlayIngredientBridge`**：三个可空静态 `Func?`，供内容模组注入合成 ID ↔ blockValue 解析。该桥：

- **单槽 last-wins**：后注册模组整段覆盖前者，无法「模组 A 认 `I-`、模组 B 认 `X-`」并存；
- **语义不属于拦截**：产出是 `string` / `int[]` / `int`，不是「能否继续」；
- **缓存僵硬**：`ExpandBlockValues` 永久缓存，hook 更换后可能脏读。

与此同时，团队希望 **InterceptBus 升级为可自定义返回值** 的统一扩展面，避免再长出第三套互不相通的静态注入点。

**结论（已拍板）**：采用 **同族统一总线**——保留现有 Intercept（bool 否决）API 兼容，新增 **Resolve（自定义返回值 + 聚合策略）** 通道族；`CraftingOverlayIngredientBridge` **删除**，改为 Resolve 总线上的内置原料通道。

---

## 2. 目标与非目标

### 2.1 目标

1. **统一扩展心智**：依赖模组面对「通知 / 拦截 / 解析贡献」三套语义，但 **同一套通道基础设施**（Subscribe + priority + Dispose）。
2. **多模原料解析**：`ToCraftingId` / `Expand` / `Decode` 可累加注册，按明确策略合并。
3. **可扩展返回值**：任意 `TContext` → `TResult`，内置若干聚合策略；自定义策略可插。
4. **可迁移**：Bridge 删除前提供等价 API 与文档；依赖模组改一两处注册即可。

### 2.2 非目标（本路线图不做）

- 不把 `RecipaediaEventBus` 并入 Intercept/Resolve（通知仍独立）。
- 不改 Placement / Host / `IPlacableRecipe` 协议（原料解析仅服务助手 `+` 与缺料文案）。
- 不在 REX 核心写任何内容模组专有合成 ID（`I-` / `N-` 等仍由依赖模组订阅贡献）。
- 不借此发版改 `modinfo` 大版本号策略以外的宿主 CI（见 [打包发布CI策划.md](打包发布CI策划.md)）。

---

## 3. 推荐形态：同族三通道

对外仍可用一个门面类（或 `RecipaediaInterceptBus` 旁挂 `RecipaediaResolveBus`），底层共享通道骨架：

```
RecipaediaEventBus      →  NotifyChannel    : Action<T>
RecipaediaInterceptBus  →  InterceptChannel : Func<T, bool>     （已有，保持）
RecipaediaResolveBus    →  ResolveChannel   : Func<T, TResult?> + IAggregateStrategy
```

### 3.1 为何不硬改 `TryProceed` 的返回类型

- 现有全部调用点依赖 `bool`（`if (!TryProceed(...)) return`）。
- 「否决链」与「贡献链」聚合语义不同：前者 **任一 false 即停**；后者常为 **first-hit / merge / reduce**。
- 硬改泛型返回值会迫使每个拦截点声明 `TResult`，污染门禁场景。

**因此**：Intercept **保留**；新增 **Resolve** 同族 API，文档中称为「统一总线族」的第三腿。若希望门面只有一个名字，可用：

```csharp
// 可选门面（计划中二选一，实现时定稿）
RecipaediaExtensionBus.Intercept...   // 别名 → InterceptBus
RecipaediaExtensionBus.Resolve...     // 别名 → ResolveBus
```

**默认推荐**：公开 **`RecipaediaResolveBus`**（与 Event / Intercept 并列），避免把 `Intercept` 一词语义撑破。

### 3.2 Resolve 通道核心 API（草案）

```csharp
public interface IResolveSubscriber<TContext, TResult> {
    IDisposable Subscribe(
        Func<TContext, TResult?> handler,
        int priority = 0);
}

public interface IResolvePublisher<TContext, TResult> {
    TResult Resolve(TContext context, IAggregateStrategy<TResult> strategy);
}

// 门面
public static class RecipaediaResolveBus {
    public static IResolveSubscriber<TContext, TResult> GetSubscriber<TContext, TResult>();
    public static TResult Resolve<TContext, TResult>(
        TContext context,
        IAggregateStrategy<TResult> strategy);
}
```

### 3.3 内置聚合策略

| 策略 | 行为 | 典型用途 |
|------|------|----------|
| `FirstNonNull` | priority 升序，第一个非 null 命中即返回 | `ToCraftingId`、`DecodeDisplay` |
| `FirstNonEmptyArray` | 第一个 `Length > 0` 的数组 | 单模组 Expand（兼容今日 Bridge） |
| `ConcatDistinct` | 合并多模组返回的 `int[]`，去重保序 | **多模 Expand 真·共存** |
| `AllThenReduce` | 收集全部非 null，再 `Func<IReadOnlyList<TResult>, TResult>` | 自定义合并 |

**空值约定（锁定）**：

- Handler 返回 `null`（引用）或「未处理哨兵」= **跳过，交给下一订阅者 / 默认实现**。
- Expand：**禁止**用空数组表示「未处理」——空数组表示「已处理且无匹配」；未处理必须返回 `null`（用 `int[]?`）。

### 3.4 与可变 Context 的关系

今日 `OverlaySearchApplyingContext` 靠 **可变 class** 在 Intercept 链上改写 `SearchQuery`。Resolve 落地后：

- **短期**：搜索改写可继续用 Intercept + 可变 Context（已工作）。
- **中期（可选）**：提供 `OverlaySearchQueryResolving` Resolve 通道（`string?` / FirstNonNull），再标记可变字段为过时。  
  **本路线图不强制**；优先完成原料三通道。

---

## 4. 原料通道：取代 `CraftingOverlayIngredientBridge`

### 4.1 内置 Context / Result

| 通道便捷名 | Context | Result | 默认策略 | 无订阅 / 全未命中时 |
|------------|---------|--------|----------|---------------------|
| `BlockValueToCraftingId` | `BlockValueCraftingIdContext(int BlockValue)` | `string?` | FirstNonNull | 原版 `GetCraftingId` + `:` + data |
| `ExpandIngredient` | `ExpandIngredientContext(string Ingredient)` | `int[]?` | ConcatDistinct（或可配置） | `FormattedRecipe.ExpandIngredientToBlockValues` |
| `DecodeIngredientDisplay` | `DecodeIngredientContext(string Ingredient)` | `int?` | FirstNonNull | 走 Expand 首个有效 blockValue |

门面便捷方法（供 Planner 调用，替代 Bridge 静态方法）：

```csharp
RecipaediaResolveBus.ToCraftingId(blockValue);
RecipaediaResolveBus.ExpandBlockValues(ingredient);
RecipaediaResolveBus.TryDecodeDisplayBlockValue(ingredient, out int blockValue);
```

实现内部：`Resolve(...)` + 默认回退；**缓存**改为：

- 键 = `ingredient` + **订阅世代号**（Subscribe/Dispose 时 +1），避免脏缓存；
- 或提供 `InvalidateIngredientCache()` 供模组热重载（可选）。

### 4.2 删除 Bridge

| 项 | 动作 |
|----|------|
| `Overlay/CraftingOverlayIngredientBridge.cs` | **删除** |
| `FormattedGridPlacementPlanner` | 改调 `RecipaediaResolveBus.*` |
| API 文档 § 原料桥接 / CHANGELOG | 写明 Breaking：改订 Resolve 订阅 |
| 依赖模组 | 去掉三个静态赋值，改为三次 `Subscribe` |

依赖模组迁移示例（示意）：

```csharp
// 旧
CraftingOverlayIngredientBridge.BlockValueToCraftingId = MyUtil.ToId;
CraftingOverlayIngredientBridge.ExpandIngredientBlockValues = MyUtil.Expand;
CraftingOverlayIngredientBridge.DecodeIngredientResult = MyUtil.Decode;

// 新
m_subs.Add(RecipaediaResolveBus.BlockValueToCraftingId.Subscribe(
    ctx => MyUtil.TryToId(ctx.BlockValue)));           // 未认返回 null
m_subs.Add(RecipaediaResolveBus.ExpandIngredient.Subscribe(
    ctx => MyUtil.TryExpand(ctx.Ingredient)));         // 未认返回 null
m_subs.Add(RecipaediaResolveBus.DecodeIngredientDisplay.Subscribe(
    ctx => MyUtil.TryDecode(ctx.Ingredient)));         // 未认返回 null
```

### 4.3 InterceptBus 是否「升级返回值」

**产品表述**：统一总线族支持自定义返回值（Resolve）。  
**技术表述**：不破坏现有 `TryProceed`；新增 `Resolve`。  
若社区强要求「一个类名」，可在 `RecipaediaInterceptBus` 上增加过时的转发属性指向 Resolve——**不推荐**，易混淆否决与贡献。

---

## 5. 分期实施

### Phase R0 — 设计冻结（本文档）

- [x] 选定：同族 ResolveBus + 删除 Bridge  
- [ ] API 命名终稿（`RecipaediaResolveBus` vs `ExtensionBus.Resolve`）  
- [ ] Expand 默认策略终稿：`ConcatDistinct` vs `FirstNonEmptyArray`（建议默认 **ConcatDistinct**，并在文档说明冲突时去重）

### Phase R1 — 核心通道（无 Breaking）

1. 新增 `ResolveChannel<TContext, TResult>`、`IAggregateStrategy`、内置策略。  
2. 新增 `RecipaediaResolveBus` 门面 + 单元/手工冒烟（多订阅 priority、异常隔离、Dispose）。  
3. **暂不删 Bridge**：Bridge 内部改为「若无静态 Func，则走 Resolve；若有静态 Func，仍优先旧槽并 `Log.Warning` 弃用」。  
4. 文档：API §2.5 增补 Resolve；CHANGELOG Unreleased。

### Phase R2 — 原料迁入 + 弃用窗口

1. 注册三原料通道；`FormattedGridPlacementPlanner` 只调 Resolve 便捷方法。  
2. `CraftingOverlayIngredientBridge` 标 `[Obsolete]`，实现转发到 Resolve 的「单槽兼容层」（把旧 Func 包成 priority=-1000 的隐式订阅，或继续优先旧槽）。  
3. 通知依赖模组迁移（CHANGELOG / RELEASE 说明）。

### Phase R3 — 删除 Bridge（Breaking，建议 2.1.0）

1. 删除 `CraftingOverlayIngredientBridge` 类型。  
2. 清理兼容层与警告。  
3. 验收：双模组（或双 Subscribe）分别贡献不同前缀 ID，助手 `+` / 缺料名均正确；卸载一侧 Dispose 后行为回退。

### Phase R4 — 可选收口（非必须）

- 搜索改写迁 Resolve；Intercept 可变 Context 收敛。  
- 评估其它「单槽静态注入」是否一并迁入（若有）。

---

## 6. 验收标准

| ID | 场景 | 期望 |
|----|------|------|
| RV-01 | 无订阅 | `ToCraftingId` / Expand 与原版一致 |
| RV-02 | 单模组三通道 | 行为等价今日 Bridge + IE 自定义 ID |
| RV-03 | 双模组 Expand 不同前缀 | `ConcatDistinct` 下双方候选均可匹配背包 |
| RV-04 | 双模组争同一 blockValue→Id | FirstNonNull：priority 更小者胜；文档说明 |
| RV-05 | Dispose 退订 | 该模组贡献消失；缓存世代失效 |
| RV-06 | Handler 抛异常 | Log.Error，视为未贡献，链继续 |
| RV-07 | Bridge 删除后编译 | 依赖模组必须改订阅，无残留类型 |

---

## 7. 风险与决策点

| 风险 | 缓解 |
|------|------|
| Expand 合并导致误匹配变宽 | 默认 ConcatDistinct + 文档；可用 FirstNonEmpty 通道策略覆盖 |
| 性能（每格匹配多次 Resolve） | 保留按世代缓存；handler 应 O(前缀判断) 快速 miss |
| 命名争论 Intercept vs Resolve | 本文推荐并列三总线；门面别名可后补 |
| 依赖模组未及时迁移 | R1→R2 弃用窗口至少一个 preview；R3 再删 |

**待拍板（实现前）**：

1. 公开类型名：`RecipaediaResolveBus`（推荐）还是塞进 `RecipaediaInterceptBus`？  
2. Expand 默认策略：`ConcatDistinct`（推荐）还是 `FirstNonEmptyArray`？  
3. Breaking 版本号：`2.1.0` 删 Bridge，还是 `2.0.1` 仅弃用、`3.0` 再删？

---

## 8. 文档与发版连带

| 文档 | 动作 |
|------|------|
| [API使用文档.md](API使用文档.md) §2.5 | 增补 Resolve；删除 Bridge 小节 |
| [CHANGELOG.md](CHANGELOG.md) | R1/R2/R3 分条 |
| [工作台悬浮助手策划.md](工作台悬浮助手策划.md) | Phase 2b Bridge 勾项改为 Resolve 通道 |
| [RELEASE.md](RELEASE.md) | 依赖模组迁移检查清单 |
| 本文件 | 实现推进时更新勾选与决策结果 |

---

## 9. 修订记录

| 版本 | 日期 | 说明 |
|------|------|------|
| v0.1 | 2026-07-13 | 首稿：统一总线族 + ResolveBus；删除 Bridge；分期 R0–R4 |
