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

        public static List<IRecipe> ResolveAllRecipes(IRecipaediaRecipeItem item, RecipaediaCraftingContext context) {
            List<IRecipe> recipes = [];
            foreach (IRecipe recipe in RecipaediaEXManager.Recipes.AsValueEnumerable().Where(item.Match)) {
                if (!ContainsRecipe(recipes, recipe)) recipes.Add(recipe);
            }

            TryAddDynamicPreviewRecipes(item, context, recipes);
            return recipes.AsValueEnumerable().OrderBy(r => r.DisplayOrder).ToList();
        }

        public static List<RecipaediaCrafterRecipeGroup> BuildCrafterGroups(IReadOnlyList<IRecipe> recipes, RecipaediaCraftingContext context) {
            Dictionary<string, RecipaediaCrafterRecipeGroup> groupsByCrafterId = [];
            foreach (IRecipe recipe in recipes) {
                foreach (int blockValue in GetCrafterBlockValues(recipe)) {
                    string crafterId = GetCrafterId(blockValue);
                    if (string.IsNullOrEmpty(crafterId)) continue;
                    if (!groupsByCrafterId.TryGetValue(crafterId, out RecipaediaCrafterRecipeGroup? group)) {
                        group = new RecipaediaCrafterRecipeGroup {
                            CrafterId = crafterId,
                            RepresentativeBlockValue = blockValue,
                        };
                        groupsByCrafterId[crafterId] = group;
                    }
                    if (!ContainsRecipe(group.Recipes, recipe)) group.Recipes.Add(recipe);
                }
            }

            List<RecipaediaCrafterRecipeGroup> groups = [.. groupsByCrafterId.Values];
            foreach (RecipaediaCrafterRecipeGroup group in groups) {
                group.Recipes.Sort((a, b) => a.DisplayOrder.CompareTo(b.DisplayOrder));
            }

            string hostCrafterId = context.CrafterBlockValue != 0
                ? GetCrafterId(context.CrafterBlockValue)
                : string.Empty;
            groups.Sort((a, b) => CompareGroups(a, b, hostCrafterId));
            return groups;
        }

        public static int SelectDefaultGroupIndex(IReadOnlyList<RecipaediaCrafterRecipeGroup> groups, RecipaediaCraftingContext context) {
            if (groups.Count == 0) return -1;
            if (context.CrafterBlockValue != 0) {
                string hostCrafterId = GetCrafterId(context.CrafterBlockValue);
                for (int i = 0; i < groups.Count; i++) {
                    if (groups[i].CrafterId == hostCrafterId) return i;
                }
            }
            return 0;
        }

        static int CompareGroups(RecipaediaCrafterRecipeGroup a, RecipaediaCrafterRecipeGroup b, string hostCrafterId) {
            if (a.CrafterId == hostCrafterId) return -1;
            if (b.CrafterId == hostCrafterId) return 1;
            return string.Compare(a.DisplayName, b.DisplayName, StringComparison.CurrentCulture);
        }

        static string GetCrafterId(int blockValue) {
            if (blockValue == 0) return string.Empty;
            Block block = BlocksManager.Blocks[Terrain.ExtractContents(blockValue)];
            return block.GetCraftingId(blockValue);
        }

        static List<int> GetCrafterBlockValues(IRecipe recipe) {
            Dictionary<string, int> craftersById = [];
            foreach (Block block in BlocksManager.Blocks) {
                if (block is not ICrafter crafter) continue;
                foreach (int blockValue in block.GetCreativeValues()) {
                    if (!crafter.IsCrafter(blockValue, recipe)) continue;
                    string crafterId = GetCrafterId(blockValue);
                    if (string.IsNullOrEmpty(crafterId) || craftersById.ContainsKey(crafterId)) continue;
                    craftersById[crafterId] = blockValue;
                }
            }
            return [.. craftersById.Values];
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
    }
}
