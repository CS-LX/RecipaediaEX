using System.Diagnostics;
using GameEntitySystem;
using RecipaediaEX.Implementation;
using ZLinq;

namespace RecipaediaEX {
    public enum CrafterMatchMode {
        CraftingTable,
        Furnace,
    }

    public readonly struct RecipeMatchResult {
        public FormattedRecipe? Recipe { get; init; }
        public bool IsHint { get; init; }
    }

    /// <summary>一次重匹配的分段耗时快照（毫秒）。</summary>
    public struct MatchProfile {
        public long Calls;
        public double TotalMs;
        public double DynamicMs;
        public double StaticMs;
        public double IngredientOnlyMs;
        public double LastTotalMs;
        public double MaxTotalMs;
    }

    /// <summary>
    /// 工作台 / 熔炉：动态配方 → 静态匹配 → 原料形状匹配 → 约束检查。
    /// </summary>
    public static class RecipeMatchPipeline {
        /// <summary>
        /// 打开后 <see cref="Profile" /> 会累计最近的分段耗时，用于定位「放入物品卡一下」这类问题。
        /// 默认关闭；开启时每次匹配额外做 4 次 <see cref="Stopwatch.GetTimestamp" />，开销可忽略。
        /// </summary>
        public static bool ProfilingEnabled { get; set; }

        /// <summary>
        /// 单次重匹配超过该毫秒数时输出一条诊断日志（含分段耗时）；<c>0</c> 表示关闭。
        /// <para>
        /// 默认关闭。历史上默认值曾设为 10ms，结果一旦出现「每一帧都在重匹配」的机器，
        /// 就会变成每帧一条同步磁盘日志，把原本偶发的卡顿放大成持续掉帧。
        /// 需要单次明细时再手动开。
        /// </para>
        /// </summary>
        public static float SlowMatchLogThresholdMs { get; set; }

        /// <summary>
        /// 低频汇总诊断：每秒最多输出一条，且只在「明显异常」时输出
        /// （一秒内重匹配 ≥ 20 次，或有单次 ≥ 8ms，或有配方退回逐字符串慢路径）。
        /// 用来回答「打开界面后是不是每一帧都在重匹配」这个关键问题，正常游玩时完全静默。
        /// </summary>
        public static bool DiagnosticsEnabled { get; set; } = true;

        public static MatchProfile Profile { get; private set; }

        public static void ResetProfile() => Profile = default;

        // ---- 每秒窗口统计 ----
        static long s_windowCalls;
        static double s_windowTotalMs;
        static double s_windowMaxMs;
        static double s_windowDynamicMs;
        static double s_windowStaticMs;
        static double s_windowIngredientMs;
        static int s_windowCraftingTableCalls;
        static int s_windowFurnaceCalls;
        static bool s_windowStarted;
        static double s_windowStart;

        /// <summary>每秒调用一次（由 <c>GuiUpdate</c> 钩子驱动），汇总上一秒的匹配开销。</summary>
        public static void TickDiagnostics() {
            if (!DiagnosticsEnabled) return;
            double now = Engine.Time.FrameStartTime;
            if (!s_windowStarted) {
                s_windowStarted = true;
                s_windowStart = now;
                return;
            }
            double elapsed = now - s_windowStart;
            if (elapsed < 1.0) return;
            FlushWindow(elapsed);
            s_windowStart = now;
        }

        static void FlushWindow(double elapsed) {
            long calls = s_windowCalls;
            if (calls > 0) {
                double maxMs = s_windowMaxMs;
                long preTransform = FormattedRecipe.TakePreTransformCallCount();
                long slowPath = FormattedRecipe.TakeSlowPathCallCount();
                bool suspicious = calls >= 20 || maxMs >= 8.0 || slowPath > 0;
                if (suspicious) {
                    Engine.Log.Information(string.Format(
                        "[RecipaediaEX] 配方匹配 {0:F1}s 内 {1} 次（工作台 {2} / 熔炉 {3}），合计 {4:F1}ms，最长 {5:F2}ms"
                        + "；动态 {6:F1}ms / 静态 {7:F1}ms / 仅原料 {8:F1}ms；配方表 {9} 条；慢路径 {10} 次；编码重建 {11} 次",
                        elapsed, calls, s_windowCraftingTableCalls, s_windowFurnaceCalls,
                        s_windowTotalMs, maxMs,
                        s_windowDynamicMs, s_windowStaticMs, s_windowIngredientMs,
                        RecipaediaEXManager.Recipes.Count, slowPath, preTransform));
                }
            }
            else {
                FormattedRecipe.TakePreTransformCallCount();
                FormattedRecipe.TakeSlowPathCallCount();
            }
            s_windowCalls = 0;
            s_windowTotalMs = 0;
            s_windowMaxMs = 0;
            s_windowDynamicMs = 0;
            s_windowStaticMs = 0;
            s_windowIngredientMs = 0;
            s_windowCraftingTableCalls = 0;
            s_windowFurnaceCalls = 0;
        }

        public static string DescribeProfile() {
            MatchProfile p = Profile;
            if (p.Calls <= 0) return "[RecipaediaEX] 匹配耗时统计：无样本";
            return string.Format(
                "[RecipaediaEX] 匹配耗时 {0} 次：总计 {1:F1}ms 平均 {2:F3}ms 最长 {3:F3}ms | 动态 {4:F1}ms 静态 {5:F1}ms 仅原料 {6:F1}ms",
                p.Calls, p.TotalMs, p.TotalMs / p.Calls, p.MaxTotalMs, p.DynamicMs, p.StaticMs, p.IngredientOnlyMs);
        }

        static double Ms(long from, long to) => (to - from) * 1000.0 / Stopwatch.Frequency;

        public static RecipeMatchResult Resolve(IRecipe actual, CrafterMatchMode mode, Project? project) {
            float slowThreshold = SlowMatchLogThresholdMs;
            bool profiling = ProfilingEnabled;
            bool timing = profiling || slowThreshold > 0f || DiagnosticsEnabled;
            long tStart = timing ? Stopwatch.GetTimestamp() : 0L;
            long tDynamic = 0L;
            long tStatic = 0L;
            long tIngredient = 0L;

            RecipeMatchResult outcome = ResolveCore(actual, mode, project, timing, ref tDynamic, ref tStatic, ref tIngredient);

            if (timing) {
                long tEnd = Stopwatch.GetTimestamp();
                double total = Ms(tStart, tEnd);
                // ResolveCore 的早退分支（actual 不合格 / 全空）不会打点，此时分段耗时无意义，一律记 0。
                bool measured = tIngredient != 0L;
                double dynamicMs = measured ? Ms(tStart, tDynamic) : 0.0;
                double staticMs = measured ? Ms(tDynamic, tStatic) : 0.0;
                double ingredientMs = measured ? Ms(tStatic, tIngredient) : 0.0;
                if (profiling) {
                    MatchProfile p = Profile;
                    p.Calls++;
                    p.TotalMs += total;
                    p.LastTotalMs = total;
                    if (total > p.MaxTotalMs) p.MaxTotalMs = total;
                    p.DynamicMs += dynamicMs;
                    p.StaticMs += staticMs;
                    p.IngredientOnlyMs += ingredientMs;
                    Profile = p;
                }
                if (slowThreshold > 0f && total >= slowThreshold && measured) {
                    Engine.Log.Information(string.Format(
                        "[RecipaediaEX] 配方重匹配耗时 {0:F1}ms（模式 {1}，动态 {2:F1}ms / 静态 {3:F1}ms / 仅原料 {4:F1}ms，配方表 {5} 条）",
                        total, mode, dynamicMs, staticMs, ingredientMs,
                        RecipaediaEXManager.Recipes.Count));
                }
                AccumulateWindow(mode, total, dynamicMs, staticMs, ingredientMs);
            }
            return outcome;
        }

        static void AccumulateWindow(CrafterMatchMode mode, double total, double dynamic, double staticMs, double ingredient) {
            s_windowCalls++;
            s_windowTotalMs += total;
            s_windowDynamicMs += dynamic;
            s_windowStaticMs += staticMs;
            s_windowIngredientMs += ingredient;
            if (total > s_windowMaxMs) s_windowMaxMs = total;
            if (mode == CrafterMatchMode.CraftingTable) s_windowCraftingTableCalls++;
            else s_windowFurnaceCalls++;
        }

        static RecipeMatchResult ResolveCore(
            IRecipe actual,
            CrafterMatchMode mode,
            Project? project,
            bool timing,
            ref long tDynamic,
            ref long tStatic,
            ref long tIngredient) {
            if (actual is not FormattedRecipe actualFormatted) return default;
            if (actualFormatted.Ingredients.AsValueEnumerable().All(string.IsNullOrEmpty)) return default;

            float heatLevel = actualFormatted.RequiredHeatLevel;
            float playerLevel = actualFormatted.RequiredPlayerLevel;
            FormattedRecipe? matched = null;
            bool fromDynamic = false;

            if (project != null) {
                matched = RecipaediaEXManager.FindDynamicRecipe(actual, project) as FormattedRecipe;
                if (matched != null) fromDynamic = true;
            }
            // project 为 null 时没有动态阶段，也要打点，否则分段耗时会被算成负数。
            if (timing) tDynamic = Stopwatch.GetTimestamp();
            if (matched == null) matched = TryStaticMatch(actual, mode);
            if (timing) tStatic = Stopwatch.GetTimestamp();
            if (matched == null) matched = TryIngredientOnlyMatch(actualFormatted);
            if (timing) tIngredient = Stopwatch.GetTimestamp();

            FormattedRecipe? result = RecipeConstraintChecker.Apply(matched, heatLevel, playerLevel);
            bool isHint = RecipeConstraintChecker.IsHintOnly(result);
            if (!isHint) result = FilterProductiveRecipe(result, mode);

            if (result != null) {
                RecipaediaEXManager.NotifyRecipeMatched(actual, result, fromDynamic);
            }
            return new RecipeMatchResult { Recipe = result, IsHint = isHint };
        }

        static FormattedRecipe? TryStaticMatch(IRecipe actual, CrafterMatchMode mode) {
            foreach (IRecipe recipe in RecipaediaEXManager.Recipes) {
                if (mode == CrafterMatchMode.CraftingTable && recipe is not OriginalCraftingRecipe) continue;
                if (mode == CrafterMatchMode.Furnace && recipe is not OriginalSmeltingRecipe) continue;
                if (recipe.Match(actual)) return recipe as FormattedRecipe;
            }
            return null;
        }

        static FormattedRecipe? TryIngredientOnlyMatch(FormattedRecipe actual) {
            return RecipaediaEXManager.Recipes
                .AsValueEnumerable()
                .OfType<FormattedRecipe>()
                .Where(recipe => recipe.MatchIngredientsOnly(actual))
                .OrderBy(recipe => recipe.MatchPriority)
                .FirstOrDefault();
        }

        static FormattedRecipe? FilterProductiveRecipe(FormattedRecipe? recipe, CrafterMatchMode mode) {
            if (recipe == null) return null;
            if (mode == CrafterMatchMode.CraftingTable) {
                return recipe is OriginalCraftingRecipe { ResultValue: not 0 } ? recipe : null;
            }
            return recipe is OriginalSmeltingRecipe { RequiredHeatLevel: > 0 } ? recipe : null;
        }
    }
}
