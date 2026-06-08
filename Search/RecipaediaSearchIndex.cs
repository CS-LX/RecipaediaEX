using System;
using System.Collections.Generic;
using System.Reflection;
using Engine.Serialization;
using Game;
using RecipaediaEX.Events;
using RecipaediaEX.Implementation;
using RecipaediaEX.UI;
using ZLinq;

namespace RecipaediaEX.Search {
    public static class RecipaediaSearchIndex {
        static readonly Dictionary<IRecipaediaItem, ItemSearchDocument> s_documents = new();
        static readonly List<IRecipaediaSearchContributor> s_contributors = [];
        static IDisposable m_resetSubscription;
        static bool m_initialized;

        public static void Initialize() {
            if (m_initialized) return;
            m_initialized = true;
            DiscoverContributors();
            m_resetSubscription?.Dispose();
            m_resetSubscription = RecipaediaEventBus.RecipesReset.Subscribe(_ => {
                s_documents.Clear();
                BlocksCategoryProvider.InvalidateCache();
            });
        }

        static void DiscoverContributors() {
            s_contributors.Clear();
            foreach (Assembly assembly in TypeCache.LoadedAssemblies.AsValueEnumerable().Where(a => !TypeCache.IsKnownSystemAssembly(a))) {
                foreach (TypeInfo typeInfo in assembly.DefinedTypes) {
                    Type type = typeInfo.AsType();
                    if (!typeof(IRecipaediaSearchContributor).IsAssignableFrom(type) || type.IsAbstract || type.IsInterface) continue;
                    if (Activator.CreateInstance(type) is IRecipaediaSearchContributor contributor) {
                        s_contributors.Add(contributor);
                    }
                }
            }
        }

        public static ItemSearchDocument GetDocument(IRecipaediaItem item, string categoryId) {
            if (s_documents.TryGetValue(item, out ItemSearchDocument cached)) return cached;

            ItemSearchDocument doc = BuildDocument(item, categoryId);
            s_documents[item] = doc;
            return doc;
        }

        public static IEnumerable<ItemSearchDocument> GetDocuments(IEnumerable<IRecipaediaItem> items, string categoryId) {
            foreach (IRecipaediaItem item in items) {
                yield return GetDocument(item, categoryId);
            }
        }

        static ItemSearchDocument BuildDocument(IRecipaediaItem item, string categoryId) {
            string displayName = GetDisplayName(item);
            string description = GetDescription(item);
            ItemSearchKind kind = InferKind(item);
            (string packId, string modName) = ResolveModInfo(item);
            string craftingId = GetCraftingId(item);

            int recipeCountAsResult = 0;
            int recipeCountAsIngredient = 0;
            HashSet<int> crafterBlockValues = [];
            HashSet<string> recipeTypeNames = [];
            HashSet<int> resultBlockValues = [];
            HashSet<int> ingredientBlockValues = [];

            if (item is IRecipaediaRecipeItem recipeItem) {
                foreach (IRecipe recipe in RecipaediaEXManager.Recipes) {
                    if (recipeItem.Match(recipe)) {
                        recipeCountAsResult++;
                        recipeTypeNames.Add(recipe.GetType().Name);
                        AppendBlockValues(recipe.GetExtraValue(RecipeExtraKeys.MatchedResultBlockValues, Array.Empty<int>()), resultBlockValues);
                        if (RecipesCrafterManager.Crafters.TryGetValue(recipe, out List<int> crafters)) {
                            foreach (int crafter in crafters) crafterBlockValues.Add(crafter);
                        }
                    }
                    if (recipeItem.IsIngredient(recipe)) {
                        recipeCountAsIngredient++;
                        recipeTypeNames.Add(recipe.GetType().Name);
                        AppendBlockValues(recipe.GetExtraValue(RecipeExtraKeys.MatchedIngredientBlockValues, Array.Empty<int>()), ingredientBlockValues);
                        if (RecipesCrafterManager.Crafters.TryGetValue(recipe, out List<int> crafters)) {
                            foreach (int crafter in crafters) crafterBlockValues.Add(crafter);
                        }
                    }
                }
            }

            int displayOrder = item is BlockItem blockItem ? blockItem.m_order : 0;
            ItemSearchDocument doc = new() {
                Item = item,
                CategoryId = categoryId,
                DisplayName = displayName,
                NormalizedName = Normalize(displayName),
                PinyinFull = PinyinHelper.ToFullPinyin(displayName),
                PinyinInitials = PinyinHelper.ToInitials(displayName),
                DescriptionSnippet = Truncate(description, 120),
                Kind = kind,
                PackId = packId,
                ModDisplayName = modName,
                CraftingId = craftingId,
                DisplayOrder = displayOrder,
                RecipeCountAsResult = recipeCountAsResult,
                RecipeCountAsIngredient = recipeCountAsIngredient,
                CrafterBlockValues = crafterBlockValues,
                RecipeTypeNames = recipeTypeNames,
                ResultBlockValues = resultBlockValues,
                IngredientBlockValues = ingredientBlockValues,
            };

            foreach (IRecipaediaSearchContributor contributor in s_contributors) {
                contributor.EnrichItem(item, doc);
            }

            return doc;
        }

        public static int[] ResolveBlockValuesByName(string name) {
            if (string.IsNullOrWhiteSpace(name)) return [];
            string normalized = Normalize(name);
            List<int> values = [];
            foreach (Block block in BlocksManager.Blocks) {
                foreach (int blockValue in block.GetCreativeValues()) {
                    string displayName = block.GetDisplayName(null, blockValue);
                    if (Normalize(displayName).Contains(normalized, StringComparison.Ordinal)) {
                        values.Add(blockValue);
                        continue;
                    }
                    string craftingId = block.GetCraftingId(blockValue);
                    if (!string.IsNullOrEmpty(craftingId)
                        && craftingId.Contains(name, StringComparison.OrdinalIgnoreCase)) {
                        values.Add(blockValue);
                    }
                }
            }
            return values.AsValueEnumerable().Distinct().ToArray();
        }

        public static int[] ResolveCrafterBlockValuesByName(string name) {
            if (string.IsNullOrWhiteSpace(name)) return [];
            string normalized = Normalize(name);
            List<int> values = [];
            foreach (Block block in BlocksManager.Blocks) {
                if (block is not ICrafter) continue;
                foreach (int blockValue in block.GetCreativeValues()) {
                    string displayName = block.GetDisplayName(null, blockValue);
                    if (Normalize(displayName).Contains(normalized, StringComparison.Ordinal)) {
                        values.Add(blockValue);
                    }
                }
            }
            return values.AsValueEnumerable().Distinct().ToArray();
        }

        static string GetDisplayName(IRecipaediaItem item) {
            if (item is IRecipaediaDescriptionItem descriptionItem) return descriptionItem.Name ?? string.Empty;
            if (item is BlockItem blockItem) return blockItem.m_block.GetDisplayName(null, blockItem.m_blockValue);
            return item.Value?.ToString() ?? item.GetType().Name;
        }

        static string GetDescription(IRecipaediaItem item) {
            if (item is IRecipaediaDescriptionItem descriptionItem) return descriptionItem.Description ?? string.Empty;
            if (item is BlockItem blockItem) return blockItem.m_block.GetDescription(blockItem.m_blockValue);
            return string.Empty;
        }

        static string GetCraftingId(IRecipaediaItem item) {
            if (item is BlockItem blockItem) return blockItem.m_block.GetCraftingId(blockItem.m_blockValue) ?? string.Empty;
            return string.Empty;
        }

        static ItemSearchKind InferKind(IRecipaediaItem item) {
            if (item is BlockItem) return ItemSearchKind.Block;
            return ItemSearchKind.Custom;
        }

        static (string packId, string modName) ResolveModInfo(IRecipaediaItem item) {
            if (item is not BlockItem blockItem) return (string.Empty, string.Empty);
            Block block = blockItem.m_block;
            foreach (ModEntity modEntity in ModsManager.ModList) {
                if (modEntity.BlockTypes.Contains(block.GetType())) {
                    return (modEntity.modInfo.PackageName ?? string.Empty, modEntity.modInfo.Name ?? string.Empty);
                }
            }
            return ("survivalcraft", "Survivalcraft");
        }

        static void AppendBlockValues(int[] values, HashSet<int> target) {
            foreach (int value in values) target.Add(value);
        }

        public static string Normalize(string text) => string.IsNullOrEmpty(text) ? string.Empty : text.Trim().ToLowerInvariant();

        static string Truncate(string text, int maxLength) {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength) return text ?? string.Empty;
            return text[..maxLength];
        }
    }
}
