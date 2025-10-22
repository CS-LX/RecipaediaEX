using Game;
using RecipaediaEX.ComponentsExtra;
using RecipaediaEX.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlUtilities;

namespace RecipaediaEX.LoaderExtra.Loaders {
    public class BlockProceduralRecipesLoader: IRecipesLoader {
        public int Order => -15;
        public void Initialize() {

        }
        public IEnumerable<IRecipe> GetRecipes() {
            List<IRecipe> recipes = new();
            for (int i = 0; i < BlocksManager.Blocks.Count(); i++) {
                Block block = BlocksManager.Blocks[i];
                var originalRecipes = block.GetProceduralCraftingRecipes();
                foreach (Game.CraftingRecipe originalRecipe in originalRecipes) {
                    float requiredHeatLevel = originalRecipe.RequiredHeatLevel;

                    FormattedRecipe craftingRecipe;
                    if(originalRecipe.RequiredHeatLevel > 0) {
                        craftingRecipe = OriginalComponentsExtensions.ToFormattedRecipe<OriginalSmeltingRecipe>(originalRecipe);
                    }
                    else {
                        craftingRecipe = OriginalComponentsExtensions.ToFormattedRecipe<OriginalCraftingRecipe>(originalRecipe);
                    }
                    craftingRecipe.SetExtraValue("MatchedResultBlockValues", new int[] { craftingRecipe.ResultValue });
                    recipes.Add(craftingRecipe);
                }
            }
            return recipes;
        }
    }
}
