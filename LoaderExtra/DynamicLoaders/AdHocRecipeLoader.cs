using System.Collections.Generic;
using System.Reflection;
using Game;
using GameEntitySystem;
using RecipaediaEX.ComponentsExtra;

namespace RecipaediaEX.Implementation {
    /// <summary>
    /// 原版 AdHoc 配方的动态装载器。
    /// <para>
    /// <see cref="Block.GetAdHocCraftingRecipe" /> 的基类实现恒返回 <c>null</c>，实际覆写它的只有 <c>CarpetBlock</c> /
    /// <c>ClothingBlock</c> / <c>FurnitureBlock</c> 这类少数方块。原先这里每轮都要对 <see cref="BlocksManager.Blocks" />
    /// 里的每个方块做一次虚调用（几千个方块 × 每次放入物品都要重跑），其中绝大多数注定返回 <c>null</c>。
    /// 因此这里把「覆写了该方法」的方块缓存成候选列表，只在方块表重建时重新扫描。
    /// </para>
    /// </summary>
    public class AdHocRecipeLoader : IDynamicRecipeLoader {
        public int Order => 0;

        public void Initialize() { }

        static List<Block> m_candidates;
        static int m_cachedBlockCount = -1;

        /// <summary>
        /// 丢弃候选方块缓存。方块表重建（含 <c>BlocksInitalized</c>）后调用；
        /// 不调用也不会出错——<see cref="GetCandidates" /> 会按方块数组长度自行失效。
        /// </summary>
        public static void InvalidateCandidates() {
            m_candidates = null;
            m_cachedBlockCount = -1;
        }

        static List<Block> GetCandidates() {
            Block[] blocks = BlocksManager.Blocks;
            if (m_candidates != null && m_cachedBlockCount == blocks.Length) return m_candidates;

            List<Block> candidates = new();
            for (int i = 0; i < blocks.Length; i++) {
                Block block = blocks[i];
                if (block == null) continue;
                MethodInfo method = block.GetType().GetMethod(
                    nameof(Block.GetAdHocCraftingRecipe),
                    BindingFlags.Instance | BindingFlags.Public);
                if (method == null || method.DeclaringType == typeof(Block)) continue;
                candidates.Add(block);
            }
            m_candidates = candidates;
            m_cachedBlockCount = blocks.Length;
            Engine.Log.Information(
                $"[RecipaediaEX] AdHoc 配方候选方块 {candidates.Count} 个（方块总数 {blocks.Length}），已缓存。");
            return candidates;
        }

        public IRecipe GetDynamicRecipe(IRecipe actual, Project project) {
            if (actual is not FormattedRecipe formattedRecipe) return null;
            SubsystemTerrain subsystemTerrain = project.FindSubsystem<SubsystemTerrain>();
            // 只遍历真正覆写了 GetAdHocCraftingRecipe 的方块，顺带保持原有遍历顺序。
            List<Block> candidates = GetCandidates();
            for (int i = 0; i < candidates.Count; i++) {
                CraftingRecipe adHocCraftingRecipe = candidates[i].GetAdHocCraftingRecipe(
                    subsystemTerrain,
                    formattedRecipe.Ingredients,
                    formattedRecipe.RequiredHeatLevel,
                    formattedRecipe.RequiredPlayerLevel);
                if (adHocCraftingRecipe != null) {
                    FormattedRecipe formattedAdHocRecipe;
                    if (formattedRecipe.RequiredHeatLevel > 0) {
                        formattedAdHocRecipe = adHocCraftingRecipe.ToFormattedRecipe<OriginalSmeltingRecipe>();
                    }
                    else {
                        formattedAdHocRecipe = adHocCraftingRecipe.ToFormattedRecipe<OriginalCraftingRecipe>();
                    }
                    if (!formattedAdHocRecipe.Match(actual)) continue;
                    return formattedAdHocRecipe;
                }
            }
            return null;
        }
    }
}
