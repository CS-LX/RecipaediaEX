using Game;
using RecipaediaEX.ComponentsExtra;
using System.Collections.Generic;
using TemplatesDatabase;
using ZLinq;
using System;

namespace RecipaediaEX.Implementation {
    public abstract class FormattedRecipe : IRecipe {
        public int ResultValue;

        public int ResultCount;

        public int RemainsValue;

        public int RemainsCount;

        public float RequiredHeatLevel;

        public float RequiredPlayerLevel;

        public string[] Ingredients = new string[36];

        public string Description;

        public string Message;

        public bool LogErrorOnMatchFail = false;

        public virtual int MatchPriority => 0;

        public HashSet<string[]> TransformedIngredients = new();
        
        public ValuesDictionary ExtraValues = new();

        /// <summary>
        /// 在配方表中的显示顺序，DisplayOrder越小，配方越靠前
        /// </summary>
        public int DisplayOrder = 0;
        int IRecipe.DisplayOrder => DisplayOrder;

        /// <summary>
        /// 该配方所需原料格数（非空原料计数），用于展示排序的并列判定。
        /// </summary>
        public virtual int IngredientsCount {
            get {
                int count = 0;
                foreach (string ingredient in Ingredients) {
                    if (!string.IsNullOrEmpty(ingredient)) count++;
                }
                return count;
            }
        }

        #region 原料编码（匹配加速）

        /// <summary>本配方原料的整数编码，见 <see cref="IngredientCode" />。由 <see cref="PreTransformIngredients" /> 同步维护。</summary>
        public long[] IngredientCodes = Array.Empty<long>();

        /// <summary>构造上述编码时的 <see cref="Ingredients" /> 引用，用于判定编码是否仍然有效。</summary>
        string[] m_codesSource;

        int m_nonEmptyCount;

        ulong m_shapeKey;

        /// <summary>本配方含无法安全编码的原料，匹配必须退回逐字符串比较。</summary>
        bool m_legacyCompare;

        /// <summary>
        /// 与 <see cref="TransformedIngredients" /> 一一对应（同序）的整数编码，<b>惰性构建</b>。
        /// <para>
        /// 为什么必须惰性：<see cref="OriginalComponentsExtensions.ToFormattedRecipe{T}" /> 每次调用都会跑
        /// <see cref="PreTransformIngredients" />，而 AdHoc 动态装载器每轮匹配都会为命中方块的配方新建一个临时
        /// <see cref="FormattedRecipe" />。若在 <see cref="PreTransformIngredients" /> 里就为 338 个变换图案各建一份
        /// 36 格编码，这个「每次重匹配都新建临时配方」的路径会变成每帧十几万次编码，直接把游戏拖成持续掉帧。
        /// </para>
        /// <para>
        /// 实际上绝大多数比较在标量指纹（非空格数 + craftingId 多重集）阶段就被否掉了，根本走不到这里；
        /// 只有指纹命中（真正可能匹配）时才值得付这份构建成本。
        /// </para>
        /// </summary>
        long[][] m_transformedCodes;

        /// <summary>构建 <see cref="m_transformedCodes" /> 时的 <see cref="Ingredients" /> 引用。</summary>
        string[] m_transformedCodesSource;

        /// <summary>变换图案里存在无法编码的原料，逐格编码路径对本配方永久不可用。</summary>
        bool m_transformedCodesUnavailable;

        // actual 侧缓存：actual 由 <see cref="ComponentEXCraftingTable" /> 每次重匹配新建，
        // 而它的 <see cref="Ingredients" /> 会被整表配方各读一次，缓存正好一轮有效。
        string[] m_actualCodeSource;
        long[] m_actualCodes;
        int m_actualNonEmptyCount;
        ulong m_actualShapeKey;
        bool m_actualLegacyCompare;

        /// <summary>是否需要退回逐字符串比较（含 <c>""</c> / 解码失败的原料，或编码已过期）。</summary>
        public bool NeedsLegacyIngredientCompare => m_legacyCompare || !ReferenceEquals(m_codesSource, Ingredients);

        // ---- 诊断计数（由 RecipeMatchPipeline 每秒取走并清零）----

        static long s_preTransformCalls;

        static long s_slowPathCalls;

        public static long TakePreTransformCallCount() {
            long value = s_preTransformCalls;
            s_preTransformCalls = 0;
            return value;
        }

        public static long TakeSlowPathCallCount() {
            long value = s_slowPathCalls;
            s_slowPathCalls = 0;
            return value;
        }

        /// <summary>
        /// 按当前 <see cref="Ingredients" /> 重建标量指纹与本体编码。只解 36 格，不碰
        /// <see cref="TransformedIngredients" />（那部分见 <see cref="m_transformedCodes" />）。
        /// <see cref="PreTransformIngredients" /> 会自动调用。
        /// </summary>
        public void RebuildIngredientCodes() {
            InvalidateTransformedCodes();
            bool legacy = false;
            // 配方侧允许 data 缺省（表示对 data 通配）。
            long[] codes = IngredientCode.EncodeIngredients(Ingredients, false, ref legacy);
            m_codesSource = Ingredients;
            m_legacyCompare = legacy;
            if (legacy) {
                IngredientCodes = Array.Empty<long>();
                m_nonEmptyCount = 0;
                m_shapeKey = 0UL;
                return;
            }

            IngredientCodes = codes;
            m_nonEmptyCount = IngredientCode.NonEmptyCount(codes);
            m_shapeKey = IngredientCode.ShapeKey(codes);
        }

        void InvalidateTransformedCodes() {
            m_transformedCodes = null;
            m_transformedCodesSource = null;
            m_transformedCodesUnavailable = false;
        }

        /// <summary>
        /// 取「变换图案 → 每格整数编码」表；首次调用时构建。返回 <c>null</c> 表示本配方无法走整数逐格比较。
        /// </summary>
        long[][] GetTransformedCodes() {
            if (ReferenceEquals(m_transformedCodesSource, Ingredients)) return m_transformedCodes;
            if (m_transformedCodesUnavailable) return null;
            InvalidateTransformedCodes();

            int count = TransformedIngredients.Count;
            if (count == 0) {
                m_transformedCodesSource = Ingredients;
                return m_transformedCodes = Array.Empty<long[]>();
            }
            long[][] result = new long[count][];
            int index = 0;
            bool legacy = false;
            // HashSet 在未被修改时枚举顺序稳定，构建与遍历使用同一顺序。
            foreach (string[] array in TransformedIngredients) {
                long[] encoded = IngredientCode.EncodeIngredients(array, false, ref legacy);
                if (legacy) {
                    m_transformedCodesUnavailable = true;
                    m_transformedCodesSource = Ingredients;
                    return null;
                }
                result[index++] = encoded;
            }
            m_transformedCodesSource = Ingredients;
            return m_transformedCodes = result;
        }

        long[] GetOrBuildActualCodes() {
            if (m_actualCodes != null
                && ReferenceEquals(m_actualCodeSource, Ingredients)) {
                return m_actualCodes;
            }
            bool legacy = false;
            // 实际槽位一侧必须带 data：原实现里解析不出 data 的实际原料一律判否，这里把它交给慢路径保持等价。
            long[] codes = IngredientCode.EncodeIngredients(Ingredients, true, ref legacy);
            m_actualCodeSource = Ingredients;
            m_actualLegacyCompare = legacy;
            m_actualCodes = codes ?? Array.Empty<long>();
            m_actualNonEmptyCount = legacy ? 0 : IngredientCode.NonEmptyCount(m_actualCodes);
            m_actualShapeKey = legacy ? 0UL : IngredientCode.ShapeKey(m_actualCodes);
            return m_actualCodes;
        }

        #endregion

        public virtual bool MatchIngredientsOnly(IRecipe actual) {
            if (actual is not FormattedRecipe formattedRecipe) return false;

            // 外部若改写了 Ingredients 而未重跑 PreTransformIngredients，就地重建（只解 36 格，代价可忽略），
            // 避免整条配方长期退化成慢路径——那会让匹配悄悄变慢却看不出原因。
            if (!ReferenceEquals(m_codesSource, Ingredients)) RebuildIngredientCodes();

            long[] actualCodes = formattedRecipe.GetOrBuildActualCodes();
            if (m_legacyCompare || formattedRecipe.m_actualLegacyCompare) {
                return MatchIngredientsOnlySlow(actual);
            }
            // 粗筛：变换只搬运原料、不增不减，所以「非空格数 + craftingId 多重集」相同是真匹配的必要条件。
            // 指纹不等 ⇒ 一定匹配不上，直接返回，连变换图案的逐格编码都不必构建。
            if (m_nonEmptyCount != formattedRecipe.m_actualNonEmptyCount) return false;
            if (m_shapeKey != formattedRecipe.m_actualShapeKey) return false;

            long[][] transformed = GetTransformedCodes();
            if (transformed == null) return MatchIngredientsOnlySlow(actual);
            for (int i = 0; i < transformed.Length; i++) {
                if (IngredientCode.MatchesArray(transformed[i], actualCodes)) return true;
            }
            return false;
        }

        /// <summary>逐字符串比较的原始实现，语义与加速前完全一致。</summary>
        bool MatchIngredientsOnlySlow(IRecipe actual) {
            s_slowPathCalls++;
            if (actual is not FormattedRecipe formattedRecipe) return false;
            return TransformedIngredients.AsValueEnumerable().Any(
                ingredients => CompareIngredientsArray(ingredients, formattedRecipe.Ingredients)
            );
        }

        public virtual bool Match(IRecipe actual) {
            try {
                if (actual is not FormattedRecipe formattedRecipe) return false;
                return MatchIngredientsOnly(actual);
            }
            catch(Exception e) {
                if (LogErrorOnMatchFail) {
                    Engine.Log.Error("[RecipaediaEX]Formatted Recipe Match Error: " + e);
                }
                return false;
            }
        }
        public virtual bool CompareIngredientsArray(string[]? requiredIngredient, string[]? actualIngredient) {
            if (requiredIngredient == null || actualIngredient == null)
                return requiredIngredient == actualIngredient;

            // 如果长度不同，则直接返回 false
            if (requiredIngredient.Length != actualIngredient.Length)
                return false;

            // 使用 CompareIngredients 比较每个元素
            for (int i = 0; i < requiredIngredient.Length; i++) {
                if (!CompareIngredient(requiredIngredient[i], actualIngredient[i]))
                    return false;
            }

            return true;
        }
        public virtual bool CompareIngredient(string requiredIngredient, string actualIngredient, bool throwOnNotSpecified = false) {
            if (!throwOnNotSpecified && actualIngredient != null) {//如果不想CraftingRecipesManager.CompareIngredients报错，则预先检查报错case，如果符合报错case则直接返回false
                CraftingRecipesManager.DecodeIngredient(actualIngredient, out string craftingId2, out int? data2);
                if (!data2.HasValue) return false;
            }
            return CraftingRecipesManager.CompareIngredients(requiredIngredient, actualIngredient);
        }

        public virtual T GetExtraValue<T>(string key, T defaultValue) => ExtraValues.GetValue(key, defaultValue);

        public void SetExtraValue<T>(string key, T value) => ExtraValues.SetValue(key, value);

        public void PreTransformIngredients() {
            s_preTransformCalls++;
            TransformedIngredients.Clear();
            for (int i = 0; i < 2; i++) {
                for (int j = -6; j <= 6; j++) {
                    for (int k = -6; k <= 6; k++) {
                        bool flip = i != 0;
                        string[] array = new string[36];
                        if (!OriginalComponentsExtensions.TransformRecipe(array, Ingredients, k, j, flip)) {
                            continue;
                        }
                        TransformedIngredients.Add(array);
                    }
                }
            }
            UpdateMatchedIngredientBlockValues();
            RebuildIngredientCodes();
        }

        public void UpdateMatchedIngredientBlockValues() {
            SetExtraValue(RecipeExtraKeys.MatchedIngredientBlockValues, ExpandIngredientsToBlockValues(Ingredients));
        }

        public static int[] ExpandIngredientsToBlockValues(IEnumerable<string> ingredients) {
            HashSet<int> values = new();
            foreach (string ingredient in ingredients) {
                if (!string.IsNullOrEmpty(ingredient)) {
                    AddIngredientBlockValues(values, ingredient);
                }
            }
            return values.AsValueEnumerable().ToArray();
        }

        public static int[] ExpandIngredientToBlockValues(string ingredient) {
            if (string.IsNullOrEmpty(ingredient)) return Array.Empty<int>();
            HashSet<int> values = new();
            AddIngredientBlockValues(values, ingredient);
            return values.AsValueEnumerable().ToArray();
        }

        static void AddIngredientBlockValues(HashSet<int> values, string ingredient) {
            CraftingRecipesManager.DecodeIngredient(ingredient, out string craftingId, out int? data);
            foreach (Block block in BlocksManager.FindBlocksByCraftingId(craftingId)) {
                values.Add(Terrain.MakeBlockValue(block.BlockIndex, 0, data ?? 0));
            }
        }
    }
}