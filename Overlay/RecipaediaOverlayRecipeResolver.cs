using System;
using System.Collections.Generic;
using Game;
using RecipaediaEX.Implementation;
using RecipaediaEX.UI;
using ZLinq;

namespace RecipaediaEX.Overlay {
    public static class RecipaediaOverlayRecipeResolver {
        const string AllBlocksCategoryId = "All Blocks";

        public static IEnumerable<IRecipaediaItem> GetAllBlockItems() {
            var category = new BlocksCategory(AllBlocksCategoryId);
            return category.GetItems();
        }

        public static List<IRecipe> ResolvePreviewRecipes(IRecipaediaRecipeItem item, RecipaediaCraftingContext context) {
            List<IRecipe> recipes = [];
            foreach (IRecipe recipe in RecipaediaEXManager.Recipes.AsValueEnumerable().Where(item.Match)) {
                if (PassesContextFilter(recipe, context) && !ContainsRecipe(recipes, recipe)) recipes.Add(recipe);
            }

            TryAddDynamicPreviewRecipes(item, context, recipes);
            return recipes.AsValueEnumerable().OrderBy(r => r.DisplayOrder).ToList();
        }

        static bool ContainsRecipe(List<IRecipe> recipes, IRecipe recipe) {
            foreach (IRecipe existing in recipes) {
                if (ReferenceEquals(existing, recipe)) return true;
            }
            return false;
        }

        static void TryAddDynamicPreviewRecipes(IRecipaediaRecipeItem item, RecipaediaCraftingContext context, List<IRecipe> recipes) {
            if (context.Project == null) return;
            foreach (IDynamicRecipeLoader loader in RecipesLoadManager.DynamicRecipeLoaders) {
                if (loader is AdHocRecipeLoader) continue;
                IRecipe? dynamicRecipe = TryProbeDynamicRecipe(loader, item, context);
                if (dynamicRecipe == null) continue;
                if (!item.Match(dynamicRecipe)) continue;
                if (!PassesContextFilter(dynamicRecipe, context)) continue;
                if (!ContainsRecipe(recipes, dynamicRecipe)) recipes.Add(dynamicRecipe);
            }
        }

        static IRecipe? TryProbeDynamicRecipe(IDynamicRecipeLoader loader, IRecipaediaRecipeItem item, RecipaediaCraftingContext context) {
            if (item is not BlockItem blockItem) return null;
            var probe = new OriginalCraftingRecipe {
                ResultValue = blockItem.m_blockValue,
                RequiredPlayerLevel = context.PlayerLevel,
                RequiredHeatLevel = context.RequiredHeatLevel,
            };
            probe.SetExtraValue(RecipeExtraKeys.Project, context.Project);
            if (context.Inventory != null) probe.SetExtraValue(RecipeExtraKeys.Inventory, context.Inventory);
            return loader.GetDynamicRecipe(probe, context.Project!);
        }

        public static bool PassesContextFilter(IRecipe recipe, RecipaediaCraftingContext context) {
            if (context.CrafterBlockValue != 0) {
                int contents = Terrain.ExtractContents(context.CrafterBlockValue);
                Block block = BlocksManager.Blocks[contents];
                if (block is ICrafter crafter) {
                    if (!crafter.IsCrafter(context.CrafterBlockValue, recipe)) return false;
                }
                else if (!RecipesCrafterManager.Crafters.TryGetValue(recipe, out List<int>? crafters)
                    || !crafters.Contains(context.CrafterBlockValue)) {
                    return false;
                }
            }

            if (context.GridWidth > 0 && recipe is OriginalCraftingRecipe craftingRecipe) {
                int recipeWidth = GetCraftingGridWidth(craftingRecipe.Ingredients);
                if (recipeWidth > 0) {
                    if (recipeWidth > context.GridWidth) return false;
                    // US-C03：4×4 / 5×5 工作站不展示仅 3×3 可用的有形配方。
                    if (context.GridWidth > 3 && recipeWidth == 3) return false;
                }
            }

            return true;
        }

        public static int GetCraftingGridWidth(string[] ingredients) {
            if (ingredients == null || ingredients.Length < 36) return 0;
            int maxCol = 0;
            int maxRow = 0;
            for (int i = 0; i < 36; i++) {
                if (string.IsNullOrEmpty(ingredients[i])) continue;
                maxCol = Math.Max(maxCol, (i % 6) + 1);
                maxRow = Math.Max(maxRow, (i / 6) + 1);
            }
            return Math.Max(maxCol, maxRow);
        }
    }
}
