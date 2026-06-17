using System.Collections.Generic;
using RecipaediaEX;
using RecipaediaEX.Implementation;

namespace RecipaediaEX.Overlay {
    public static class PlacableRecipeAdapter {
        public static bool TryAsPlacable(IRecipe recipe, out IPlacableRecipe placable) {
            if (recipe is IPlacableRecipe existing) {
                placable = existing;
                return true;
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
