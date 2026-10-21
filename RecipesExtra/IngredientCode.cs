using System;
using System.Collections.Generic;
using Game;

namespace RecipaediaEX {
    /// <summary>
    /// 把 <c>"craftingId:data"</c> 形式的原料串一次性编码成 <see cref="long" />，
    /// 使配方匹配的内层循环退化为整数比较。
    /// <para>
    /// 动机：<see cref="CraftingRecipesManager.DecodeIngredient" /> 每调用一次都要先走
    /// <c>ModsManager.HookAction("DecodeIngredient")</c>（遍历全部 <see cref="ModLoader" />），
    /// 再做字符串 <c>Split(':')</c> + <c>int.Parse</c>。工作台 / 熔炉每往空槽放入一次物品就会重跑
    /// 一次全表匹配，一张几千条配方 × 数百个变换图案的表意味着每次几十万次解码，
    /// 这是「放入物品卡一下」的主要开销来源。
    /// </para>
    /// <para>
    /// 编码保持 <see cref="CraftingRecipesManager.CompareIngredients" /> 的判定语义：
    /// 槽位同空 ⇒ 相等；一侧为空一侧非空 ⇒ 不等；craftingId 不同 ⇒ 不等；
    /// 配方侧 <c>data</c> 未指定 ⇒ 对 <c>data</c> 通配；双方都指定 ⇒ 必须相等。
    /// 只要语义对齐，粗筛就只会少算、不会漏配。
    /// </para>
    /// </summary>
    public static class IngredientCode {
        /// <summary>空槽（<c>null</c> 原料）的编码。任何非空原料的编码都不会是 0。</summary>
        public const long Empty = 0L;

        const long DataMask = 0xFFFFFFFFL;

        /// <summary><c>data</c> 未指定（通配）时低 32 位的取值。</summary>
        const long WildcardData = 0L;

        /// <summary>
        /// craftingId → 内部整数 id 的映射。
        /// <para>
        /// 只增不删，也不随 <see cref="RecipaediaEXManager.ResetRecipes" /> 清空：编码只需要
        /// 「同一字符串恒得同一 id、不同字符串恒得不同 id」，与具体世界无关；清空反而会让
        /// 同一轮里配方侧与实际侧拿到不同 id。条目数等于全模组出现过的不同 craftingId 数量，可忽略。
        /// </para>
        /// </summary>
        static readonly Dictionary<string, int> m_idByCraftingId = new(StringComparer.Ordinal);

        static int m_nextId = 1;

        static int GetId(string craftingId) {
            if (m_idByCraftingId.TryGetValue(craftingId, out int id)) return id;
            id = m_nextId++;
            m_idByCraftingId.Add(craftingId, id);
            return id;
        }

        /// <summary>
        /// 原料串 → 解码结果缓存。
        /// <para>
        /// 这是本文件存在的前提：<see cref="CraftingRecipesManager.DecodeIngredient" /> 每次调用都要先
        /// <c>ModsManager.HookAction("DecodeIngredient")</c> 遍历全部 <see cref="ModLoader" />，再做
        /// <c>Split(':')</c> + <c>int.Parse</c>，单次约微秒级。而装载期要对「配方数 × 变换图案数 × 36 格」
        /// 做编码（实测约 900 万格），不缓存的话光装载就要几十秒，且一旦有代码在运行期重建编码就会直接卡死。
        /// </para>
        /// <para>
        /// 同一个原料串在同一会话内的解码结果恒定（各 ModLoader 的 <c>DecodeIngredient</c> 只做字符串改写，
        /// 不依赖世界状态），因此可以永久缓存。条目数等于全模组出现过的不同原料串数量（数千级）。
        /// </para>
        /// </summary>
        static readonly Dictionary<string, DecodedIngredient> m_decodeCache = new(StringComparer.Ordinal);

        /// <summary>防御性上限：正常情况下远达不到，避免异常输入把缓存撑爆。</summary>
        const int MaxDecodeCacheEntries = 200000;

        struct DecodedIngredient {
            /// <summary>内部 id，<c>0</c> 表示解码失败。</summary>
            public int Id;
            /// <summary><c>data + 1</c>；<c>0</c> 表示原料未指定 data（对 data 通配）。</summary>
            public int DataPlusOne;
        }

        static void CacheDecoded(string ingredient, DecodedIngredient decoded) {
            if (m_decodeCache.Count >= MaxDecodeCacheEntries) m_decodeCache.Clear();
            m_decodeCache[ingredient] = decoded;
        }

        /// <summary>
        /// 尝试编码单个原料串。
        /// <para>
        /// <paramref name="requireData" /> 区分两侧的严格度：实际槽位一侧必须带 <c>data</c>
        /// （<see cref="FormattedRecipe.CompareIngredient" /> 遇到解析不出 <c>data</c> 的实际原料会直接判否），
        /// 配方一侧则允许缺省 <c>data</c>（表示对 <c>data</c> 通配）。
        /// </para>
        /// <returns><c>false</c> 表示该串无法安全编码（空串、缺 <c>data</c> 的实际原料、解析异常等），调用方应退回逐字符串比较。</returns>
        /// </summary>
        public static bool TryEncode(string ingredient, bool requireData, out long code) {
            code = Empty;
            // 空槽两侧都是 null，与 requireData 无关。
            if (ingredient == null) return true;
            // 空串的语义与 null 不同：它连自己都匹配不上（解析不出 data），
            // 用哨兵表达容易出错，直接让调用方退回原路径。
            if (ingredient.Length == 0) return false;

            if (!m_decodeCache.TryGetValue(ingredient, out DecodedIngredient decoded)) {
                decoded = Decode(ingredient);
                CacheDecoded(ingredient, decoded);
            }
            if (decoded.Id == 0) return false;
            // requireData：实际槽位一侧必须带 data（原实现遇到解析不出 data 的实际原料直接判否）。
            if (requireData && decoded.DataPlusOne == 0) return false;
            code = ((long)decoded.Id << 32) | (uint)decoded.DataPlusOne;
            return true;
        }

        static DecodedIngredient Decode(string ingredient) {
            try {
                CraftingRecipesManager.DecodeIngredient(ingredient, out string craftingId, out int? data);
                if (string.IsNullOrEmpty(craftingId)) return default;
                if (data.HasValue && data.Value < 0) return default;
                return new DecodedIngredient {
                    Id = GetId(craftingId),
                    DataPlusOne = data.HasValue ? data.Value + 1 : 0,
                };
            }
            catch (Exception) {
                // 与原实现一致：解码失败等价于匹配失败，这里把决定权交回慢路径。
                return default;
            }
        }

        /// <summary>
        /// 编码整组原料。任一格无法编码时返回 <c>null</c>，并把 <paramref name="legacy" /> 置为 <c>true</c>。
        /// </summary>
        public static long[] EncodeIngredients(string[] ingredients, bool requireData, ref bool legacy) {
            if (ingredients == null) return Array.Empty<long>();
            long[] codes = new long[ingredients.Length];
            for (int i = 0; i < ingredients.Length; i++) {
                if (!TryEncode(ingredients[i], requireData, out codes[i])) {
                    legacy = true;
                    return null;
                }
            }
            return codes;
        }

        /// <summary>单格比较，等价于 <see cref="CraftingRecipesManager.CompareIngredients" />。</summary>
        public static bool Matches(long required, long actual) {
            if (required == Empty) return actual == Empty;
            if (actual == Empty) return false;
            if ((required >> 32) != (actual >> 32)) return false;
            long requiredData = required & DataMask;
            return requiredData == WildcardData || requiredData == (actual & DataMask);
        }

        /// <summary>整组比较，等价于 <see cref="FormattedRecipe.CompareIngredientsArray" />。</summary>
        public static bool MatchesArray(long[] required, long[] actual) {
            if (required == null || actual == null) return ReferenceEquals(required, actual);
            if (required.Length != actual.Length) return false;
            for (int i = 0; i < required.Length; i++) {
                if (!Matches(required[i], actual[i])) return false;
            }
            return true;
        }

        /// <summary>非空原料格数。</summary>
        public static int NonEmptyCount(long[] codes) {
            if (codes == null) return 0;
            int count = 0;
            for (int i = 0; i < codes.Length; i++) {
                if (codes[i] != Empty) count++;
            }
            return count;
        }

        /// <summary>
        /// 原料的「形状无关」指纹：非空格数 + 排序后的 craftingId 多重集。
        /// <para>
        /// 配方匹配是在 <c>TransformedIngredients</c> 上做平移 / 翻转遍历，而变换只搬运原料、不增不减，
        /// 所以「某变换图案与实际图案逐格全等」必然蕴含「两者的 craftingId 多重集相同」。
        /// 因此本指纹是真匹配的<b>必要条件</b>，可用于粗筛：指纹不等直接否掉。
        /// </para>
        /// </summary>
        public static ulong ShapeKey(long[] codes) {
            if (codes == null) return 0UL;
            Span<int> ids = stackalloc int[codes.Length == 0 ? 1 : codes.Length];
            int n = 0;
            for (int i = 0; i < codes.Length; i++) {
                long code = codes[i];
                if (code == Empty) continue;
                ids[n++] = (int)(code >> 32);
            }
            // n 最多 36，插入排序足够。
            for (int i = 1; i < n; i++) {
                int value = ids[i];
                int j = i - 1;
                while (j >= 0 && ids[j] > value) {
                    ids[j + 1] = ids[j];
                    j--;
                }
                ids[j + 1] = value;
            }
            ulong hash = 14695981039346656037UL;
            hash = (hash ^ (ulong)(uint)n) * 1099511628211UL;
            for (int i = 0; i < n; i++) {
                hash = (hash ^ (ulong)(uint)ids[i]) * 1099511628211UL;
            }
            return hash;
        }
    }
}
