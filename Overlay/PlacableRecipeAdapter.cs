using System;
using System.Collections.Generic;
using RecipaediaEX;
using RecipaediaEX.Implementation;

namespace RecipaediaEX.Overlay {
    /// <summary>
    /// 配方 → <see cref="IPlacableRecipe"/> 适配链。REX 内置 <see cref="OriginalCraftingRecipe"/>；
    /// 工业/化工等由内容模组 <see cref="Register"/>，摆放算法仍在各模组 <see cref="IRecipePlacementTarget"/>。
    /// </summary>
    public static class PlacableRecipeAdapter {
        static readonly List<Func<IRecipe, IPlacableRecipe?>> s_customFactories = [];

        /// <summary>内容模组注册工业等非 OriginalCrafting 配方适配器。</summary>
        public static void Register(Func<IRecipe, IPlacableRecipe?> factory) => s_customFactories.Add(factory);

        public static bool TryAsPlacable(IRecipe recipe, out IPlacableRecipe placable) {
            if (recipe is IPlacableRecipe existing) {
                placable = existing;
                return true;
            }
            foreach (Func<IRecipe, IPlacableRecipe?> factory in s_customFactories) {
                if (factory(recipe) is IPlacableRecipe custom) {
                    placable = custom;
                    return true;
                }
            }
            if (recipe is OriginalCraftingRecipe craftingRecipe) {
                placable = new OriginalCraftingPlacableRecipe(craftingRecipe);
                return true;
            }
            placable = null!;
            return false;
        }
    }

    sealed class OriginalCraftingPlacableRecipe : IPlacableRecipe {
        readonly OriginalCraftingRecipe m_recipe;

        public OriginalCraftingPlacableRecipe(OriginalCraftingRecipe recipe) => m_recipe = recipe;

        public FormattedRecipe Recipe => m_recipe;

        public IReadOnlyList<PlacementRequirement> GetPlacementRequirements() {
            var requirements = new List<PlacementRequirement>();
            for (int i = 0; i < m_recipe.Ingredients.Length; i++) {
                string ingredient = m_recipe.Ingredients[i];
                if (string.IsNullOrEmpty(ingredient)) continue;
                requirements.Add(new PlacementRequirement {
                    AddressKind = PlacementAddressKind.GridCell,
                    AddressIndex = i,
                    MatchKey = ingredient,
                    Quantity = 1f,
                    AcceptedBlockValues = FormattedRecipe.ExpandIngredientToBlockValues(ingredient),
                });
            }
            return requirements;
        }
    }
}
