using System.Collections.Generic;

namespace RecipaediaEX.UI {
    /// <summary>
    /// 配方列表的展示排序：**主产物(<c>Result</c>)配方在前，副产物(<c>Remains</c>)配方在后**，
    /// 同组内沿用 <see cref="IRecipe.DisplayOrder"/> 升序。
    /// <para>对未重写 <see cref="IRecipaediaRecipeItem.MatchesAsResult"/> 的条目，全部归入主产物组，
    /// 退化为「仅按 <c>DisplayOrder</c>」，与改动前行为一致。</para>
    /// </summary>
    public static class RecipaediaRecipeSorter {
        /// <summary>排序分组键：主产物为 0，副产物为 1（越小越靠前）。</summary>
        public static int GetDisplayGroup(IRecipaediaRecipeItem item, IRecipe recipe) {
            return item.MatchesAsResult(recipe) ? 0 : 1;
        }

        /// <summary>比较两个配方的展示顺序（分组 → <c>DisplayOrder</c>），供 <see cref="List{T}.Sort(System.Comparison{T})"/> 使用。</summary>
        public static int CompareForDisplay(IRecipaediaRecipeItem item, IRecipe a, IRecipe b) {
            int groupCompare = GetDisplayGroup(item, a).CompareTo(GetDisplayGroup(item, b));
            return groupCompare != 0 ? groupCompare : a.DisplayOrder.CompareTo(b.DisplayOrder);
        }

        /// <summary>筛出该条目对应的配方（含符合条件的副产物配方）并按展示顺序排好。</summary>
        public static List<IRecipe> GetDisplayRecipes(IEnumerable<IRecipe> allRecipes, IRecipaediaRecipeItem item) {
            List<IRecipe> recipes = [];
            foreach (IRecipe recipe in allRecipes) {
                if (item.Match(recipe)) recipes.Add(recipe);
            }
            recipes.Sort((a, b) => CompareForDisplay(item, a, b));
            return recipes;
        }

        /// <summary>就地按展示顺序排列配方列表，并原样返回，便于链式书写。</summary>
        public static List<IRecipe> OrderForDisplay(List<IRecipe> recipes, IRecipaediaRecipeItem item) {
            recipes.Sort((a, b) => CompareForDisplay(item, a, b));
            return recipes;
        }
    }
}
