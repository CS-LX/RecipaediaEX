using System;
using System.Collections.Generic;
using System.Reflection;
using Engine.Serialization;
using Game;
using RecipaediaEX.Implementation;
using ZLinq;

namespace RecipaediaEX.UI {
    /// <summary>图鉴分类目录：扫描 <see cref="IRecipaediaCategoryProvider"/>，供全屏图鉴与合成助手共用。</summary>
    public static class RecipaediaCategoryCatalog {
        static readonly Dictionary<string, IRecipaediaCategory> m_categories = [];
        static readonly List<string> m_categoryIds = [];
        static readonly List<Assembly> m_scannedAssemblies = [];
        static readonly Dictionary<Type, IRecipaediaCategoryProvider> m_providerCache = [];
        static bool m_loaded;

        public static IReadOnlyList<string> CategoryIds {
            get {
                EnsureLoaded();
                return m_categoryIds;
            }
        }

        public static IRecipaediaCategory GetCategory(string categoryId) {
            EnsureLoaded();
            return m_categories[categoryId];
        }

        /// <summary>合成助手默认分类：跳过 All Blocks，降低首屏条目量。</summary>
        public static string DefaultOverlayCategoryId {
            get {
                EnsureLoaded();
                if (m_categoryIds.Count > 1) return m_categoryIds[1];
                return m_categoryIds.Count > 0 ? m_categoryIds[0] : "All Blocks";
            }
        }

        public static void EnsureLoaded() {
            if (m_loaded) return;
            Rebuild();
        }

        public static void Invalidate() {
            m_loaded = false;
            m_categories.Clear();
            m_categoryIds.Clear();
            BlocksCategoryProvider.InvalidateCache();
        }

        static void Rebuild() {
            m_categories.Clear();
            m_categoryIds.Clear();
            ScanProviders();
            foreach (IRecipaediaCategoryProvider provider in m_providerCache.Values) {
                foreach (IRecipaediaCategory category in provider.GetCategories()) {
                    m_categories[category.Id] = category;
                    if (!m_categoryIds.Contains(category.Id)) m_categoryIds.Add(category.Id);
                }
            }
            m_loaded = true;
        }

        static void ScanProviders() {
            foreach (Assembly assembly in TypeCache.LoadedAssemblies.AsValueEnumerable().Where(a => !TypeCache.IsKnownSystemAssembly(a))) {
                if (m_scannedAssemblies.Contains(assembly)) continue;
                foreach (TypeInfo definedType in assembly.DefinedTypes) {
                    Type type = definedType.AsType();
                    if (!typeof(IRecipaediaCategoryProvider).IsAssignableFrom(type) || type.IsAbstract || type.IsInterface) continue;
                    if (m_providerCache.ContainsKey(type)) continue;
                    if (Activator.CreateInstance(type) is IRecipaediaCategoryProvider instance) m_providerCache[type] = instance;
                }
                m_scannedAssemblies.Add(assembly);
            }
        }

        public static bool TryFindCategoryForRecipeItem(IRecipaediaRecipeItem item, out string categoryId, out IRecipaediaRecipeItem listItem) {
            EnsureLoaded();
            foreach (string id in m_categoryIds) {
                foreach (IRecipaediaItem candidate in m_categories[id].GetItems()) {
                    if (candidate is not IRecipaediaRecipeItem recipeCandidate || !SameRecipeItem(recipeCandidate, item)) continue;
                    categoryId = id;
                    listItem = recipeCandidate;
                    return true;
                }
            }
            categoryId = string.Empty;
            listItem = null!;
            return false;
        }

        public static bool SameRecipeItem(IRecipaediaRecipeItem a, IRecipaediaRecipeItem b) {
            if (a is BlockItem blockA && b is BlockItem blockB) return blockA.m_blockValue == blockB.m_blockValue;
            return ReferenceEquals(a, b);
        }
    }
}
