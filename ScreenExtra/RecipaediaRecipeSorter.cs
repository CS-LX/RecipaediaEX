using System.Collections.Generic;

namespace RecipaediaEX.UI {
    /// <summary>
    /// 配方列表的展示排序，与原版 <c>CraftingRecipesManager</c> 的排序规则保持一致：
    /// <list type="number">
    ///     <item><description>**主产物(<c>Result</c>)配方在前，副产物(<c>Remains</c>)配方在后**；</description></item>
    ///     <item><description><see cref="IRecipe.DisplayOrder"/> 升序；</description></item>
    ///     <item><description><see cref="IRecipe.IngredientsCount"/> 降序（原料多的在前，同原版；如「用铜锭合成桶」排在「把桶倒空」前）。</description></item>
    /// </list>
    /// <para>完全并列的配方保持原有顺序（稳定排序），避免结果随机抖动。</para>
    /// <para>对未重写 <see cref="IRecipaediaRecipeItem.MatchesAsResult"/> 的条目，全部归入主产物组，
    /// 退化为「<c>DisplayOrder</c> → 原料数」，与依赖方预期一致。</para>
    /// </summary>
    public static class RecipaediaRecipeSorter {
        /// <summary>排序分组键：主产物为 0，副产物为 1（越小越靠前）。</summary>
        public static int GetDisplayGroup(IRecipaediaRecipeItem item, IRecipe recipe) {
            return item.MatchesAsResult(recipe) ? 0 : 1;
        }

        /// <summary>
        /// 比较两个配方的展示顺序（分组 → <c>DisplayOrder</c> → 原料数降序），
        /// 供 <see cref="List{T}.Sort(System.Comparison{T})"/> 使用。
        /// </summary>
        public static int CompareForDisplay(IRecipaediaRecipeItem item, IRecipe a, IRecipe b) {
            int groupCompare = GetDisplayGroup(item, a).CompareTo(GetDisplayGroup(item, b));
            if (groupCompare != 0) return groupCompare;
            int orderCompare = a.DisplayOrder.CompareTo(b.DisplayOrder);
            if (orderCompare != 0) return orderCompare;
            return b.IngredientsCount.CompareTo(a.IngredientsCount);
        }

        /// <summary>筛出该条目对应的配方（含被配方放行的副产物配方）并按展示顺序排好。</summary>
        public static List<IRecipe> GetDisplayRecipes(IEnumerable<IRecipe> allRecipes, IRecipaediaRecipeItem item) {
            List<IRecipe> recipes = [];
            foreach (IRecipe recipe in allRecipes) {
                if (item.Match(recipe)) recipes.Add(recipe);
            }
            return OrderForDisplay(recipes, item);
        }

        /// <summary>就地按展示顺序排列配方列表，并原样返回，便于链式书写。</summary>
        public static List<IRecipe> OrderForDisplay(List<IRecipe> recipes, IRecipaediaRecipeItem item) {
            // 以原下标作最终并列判定，得到一个全序，从而让 List.Sort 表现得稳定
            // （List.Sort 本身不稳定，只靠 CompareForDisplay 会让完全并列的配方随机换位）。
            Dictionary<IRecipe, int> originalIndices = new(recipes.Count);
            for (int i = 0; i < recipes.Count; i++) {
                originalIndices.TryAdd(recipes[i], i);
            }
            recipes.Sort((a, b) => {
                int compare = CompareForDisplay(item, a, b);
                return compare != 0 ? compare : originalIndices[a].CompareTo(originalIndices[b]);
            });
            return recipes;
        }
    }
}
