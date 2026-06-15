using System.Collections.Generic;
using Game;
using RecipaediaEX.UI;
using ZLinq;

namespace RecipaediaEX.Implementation {
    public class BlocksCategoryProvider : IRecipaediaCategoryProvider {
        static List<IRecipaediaCategory> m_cachedCategories;

        public static void InvalidateCache() => m_cachedCategories = null;

        public IEnumerable<IRecipaediaCategory> GetCategories() {
            if (m_cachedCategories == null) {
                m_cachedCategories = BlocksManager.Categories
                    .AsValueEnumerable()
                    .Select(x => (IRecipaediaCategory)new BlocksCategory(x))
                    .Prepend(new BlocksCategory("All Blocks"))
                    .ToList();
            }
            return m_cachedCategories;
        }
    }
}