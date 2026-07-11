using System;
using System.Collections.Generic;
using RecipaediaEX;
using RecipaediaEX.Implementation;

namespace RecipaediaEX.Overlay {
    /// <summary>
    /// 配方是否支持自动摆放的判定链。REX 内置 <see cref="OriginalCraftingRecipe"/> / <see cref="OriginalSmeltingRecipe"/>；
    /// 工业/化工等由内容模组 <see cref="Register"/>，摆放算法在各模组 <see cref="IRecipePlacementTarget"/>。
    /// </summary>
    public static class PlacableRecipeAdapter {
        static readonly List<Func<IRecipe, IPlacableRecipe?>> s_customFactories = [];

        /// <summary>内容模组注册工业等非 Formatted 配方适配器。</summary>
        public static void Register(Func<IRecipe, IPlacableRecipe?> factory) => s_customFactories.Add(factory);

        public static bool IsPlacable(IRecipe recipe) {
            if (recipe is IPlacableRecipe) return true;
            if (recipe is OriginalCraftingRecipe or OriginalSmeltingRecipe) return true;
            foreach (Func<IRecipe, IPlacableRecipe?> factory in s_customFactories) {
                if (factory(recipe) != null) return true;
            }
            return false;
        }
    }
}
