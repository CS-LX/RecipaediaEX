using Game;

namespace RecipaediaEX.Implementation {
    public class OriginalCraftingRecipe : FormattedRecipe, IRecipe {
        public override bool Match(IRecipe actual) {
            if (actual == null || actual is not OriginalCraftingRecipe craftingRecipe) return false;
            if (craftingRecipe.RequiredHeatLevel != 0) return false;
            if (craftingRecipe.RequiredPlayerLevel < RequiredPlayerLevel) return false;
            return CraftingRecipesManager.MatchRecipe(Ingredients, craftingRecipe.Ingredients);
        }
    }
}