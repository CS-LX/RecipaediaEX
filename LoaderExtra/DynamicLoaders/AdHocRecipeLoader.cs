using System;
using Game;
using GameEntitySystem;
using RecipaediaEX.ComponentsExtra;

namespace RecipaediaEX.Implementation {
    public class AdHocRecipeLoader : IDynamicRecipeLoader {
        public int Order => 0;

        public void Initialize() { }

        public IRecipe GetDynamicRecipe(IRecipe actual, Project project) {
            if (actual is not FormattedRecipe formattedRecipe) return null;
            SubsystemTerrain subsystemTerrain = project.FindSubsystem<SubsystemTerrain>();
            foreach (Block block in BlocksManager.Blocks) {
                CraftingRecipe adHocCraftingRecipe = block.GetAdHocCraftingRecipe(subsystemTerrain, formattedRecipe.Ingredients, formattedRecipe.RequiredHeatLevel, formattedRecipe.RequiredPlayerLevel);
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