using System;
using System.Collections.Generic;
using System.Reflection;
using Engine.Serialization;
using Game;
using RecipaediaEX.Events;
using RecipaediaEX.Implementation;
using RecipaediaEX.Overlay;
using RecipaediaEX.UI;
using ZLinq;

namespace RecipaediaEX.Search {
    public static class RecipaediaSearchIndex {
        static readonly Dictionary<IRecipaediaItem, ItemSearchDocument> m_documents = new();
        static readonly Dictionary<IRecipaediaItem, RecipeSearchMetadata> m_recipeMetadata = new();
        static readonly List<IRecipaediaSearchContributor> m_contributors = [];
        static IDisposable m_resetSubscription;
        static bool m_initialized;

        public static void Initialize() {
            if (m_initialized) return;
            m_initialized = true;
            DiscoverContributors();
            m_resetSubscription?.Dispose();
            m_resetSubscription = RecipaediaEventBus.RecipesReset.Subscribe(_ => {
                m_documents.Clear();
                m_recipeMetadata.Clear();
                RecipaediaSearchEngine.ClearFilterCache();
                // 进档重分配后必须丢掉菜单期冻结的 BlockItem / 绝对 BlockValue，否则 Match 与 Overlay 会挂错配方。
                RecipaediaCategoryCatalog.Invalidate();
                CraftingOverlayIngredientBridge.InvalidateCache();
                FormattedGridPlacementPlanner.InvalidateCache();
            });
        }

        static void DiscoverContributors() {
            m_contributors.Clear();
            foreach (Assembly assembly in TypeCache.LoadedAssemblies.AsValueEnumerable().Where(a => !TypeCache.IsKnownSystemAssembly(a))) {
                foreach (TypeInfo typeInfo in assembly.DefinedTypes) {
                    Type type = typeInfo.AsType();
                    if (!typeof(IRecipaediaSearchContributor).IsAssignableFrom(type) || type.IsAbstract || type.IsInterface) continue;
                    if (Activator.CreateInstance(type) is IRecipaediaSearchContributor contributor) {
                        m_contributors.Add(contributor);
                    }
                }
            }
        }

        public static ItemSearchDocument GetDocument(IRecipaediaItem item, string categoryId) {
            Initialize();
            if (m_documents.TryGetValue(item, out ItemSearchDocument cached)) return cached;

            ItemSearchDocument doc = BuildDocument(item, categoryId);
            m_documents[item] = doc;
            return doc;
        }

        /// <summary>高级筛选（@in / @crafter 等）才需要；简单文本搜索不触发全库配方扫描。</summary>
        public static RecipeSearchMetadata GetRecipeMetadata(IRecipaediaItem item) {
            Initialize();
            if (item is not IRecipaediaRecipeItem recipeItem) return RecipeSearchMetadata.Empty;
            if (m_recipeMetadata.TryGetValue(item, out RecipeSearchMetadata cached)) return cached;

            RecipeSearchMetadata metadata = BuildRecipeMetadata(recipeItem);
            m_recipeMetadata[item] = metadata;
            return metadata;
        }

        /// <summary>文本搜索预筛：仅用显示名 / 合成 id / 描述，不构建完整文档。</summary>
        public static bool MightMatchPlainText(IRecipaediaItem item, string term) {
            if (string.IsNullOrWhiteSpace(term)) return true;
            string normalized = Normalize(term);
            string displayName = Normalize(GetDisplayName(item));
            if (displayName.Contains(normalized, StringComparison.Ordinal)) return true;
            string craftingId = GetCraftingId(item);
            if (!string.IsNullOrEmpty(craftingId)
                && craftingId.Contains(term, StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
            string description = GetDescription(item);
            if (!string.IsNullOrEmpty(description)
                && Normalize(description).Contains(normalized, StringComparison.Ordinal)) {
                return true;
            }
            if (PinyinHelper.IsAsciiLetters(term)) {
                string display = GetDisplayName(item);
                string pyFull = PinyinHelper.ToFullPinyin(display);
                string pyInitials = PinyinHelper.ToInitials(display);
                string pyTerm = Normalize(term);
                if ((!string.IsNullOrEmpty(pyFull) && pyFull.Contains(pyTerm, StringComparison.Ordinal))
                    || (!string.IsNullOrEmpty(pyInitials) && pyInitials.Contains(pyTerm, StringComparison.Ordinal))) {
                    return true;
                }
            }
            return false;
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
            };

            foreach (IRecipaediaSearchContributor contributor in m_contributors) {
                contributor.EnrichItem(item, doc);
            }

            return doc;
        }

        static RecipeSearchMetadata BuildRecipeMetadata(IRecipaediaRecipeItem recipeItem) {
            int recipeCountAsResult = 0;
            int recipeCountAsIngredient = 0;
            HashSet<int> crafterBlockValues = [];
            HashSet<string> recipeTypeNames = [];
            HashSet<int> resultBlockValues = [];
            HashSet<int> ingredientBlockValues = [];

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

            return new RecipeSearchMetadata {
                RecipeCountAsResult = recipeCountAsResult,
                RecipeCountAsIngredient = recipeCountAsIngredient,
                CrafterBlockValues = crafterBlockValues,
                RecipeTypeNames = recipeTypeNames,
                ResultBlockValues = resultBlockValues,
                IngredientBlockValues = ingredientBlockValues,
            };
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
