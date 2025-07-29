using System.Collections.Generic;
using Game;
using ZLinq;

namespace RecipaediaEX {
    public static class RecipesCrafterManager {
        public static Dictionary<IRecipe, List<int>> m_crafters = [];

        /// <summary>
        /// 合成配方所需的Crafter（用于展示，不实现功能）
        /// <para>Key: 配方</para>
        /// <para>Value: Crafter的ID</para>
        /// </summary>
        public static Dictionary<IRecipe, List<int>> Crafters => m_crafters;

        public static void Initialize() {
            //获取所有配方的Crafter
            GetRecipeCrafters();
        }

        /// <summary>
        /// 获取每个配方对应的Crafter
        /// </summary>
        static void GetRecipeCrafters() {
            m_crafters.Clear();
            var recipes = RecipaediaEXManager.Recipes.AsValueEnumerable();
            if (recipes.Count() == 0) return;
            var blocks = BlocksManager.Blocks.AsValueEnumerable();

            foreach (var recipe in recipes) {
                foreach (var block in blocks) {
                    if (block is not ICrafter crafter) continue;
                    IEnumerable<int> blockValues = block.GetCreativeValues();
                    foreach (var blockValue in blockValues) {
                        if (!crafter.IsCrafter(blockValue, recipe.GetType())) continue;
                        if (!m_crafters.TryGetValue(recipe, out var crafters)) {
                            crafters = new List<int>();
                            m_crafters[recipe] = crafters;
                        }
                        crafters.Add(blockValue);
                    }
                }
            }
        }
    }
}