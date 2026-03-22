using System.Collections.Generic;
using System.Linq;
using Game;
using RecipaediaEX.UI;

namespace RecipaediaEX.Implementation {
    public class BlocksCategoryProvider : IRecipaediaCategoryProvider {
        static List<IRecipaediaCategory> m_cachedCategories;

        public IEnumerable<IRecipaediaCategory> GetCategories() {
            if (m_cachedCategories == null) {
                m_cachedCategories = BlocksManager.Categories
                    .Select(x => (IRecipaediaCategory)new BlocksCategory(x))
                    .Prepend(new BlocksCategory("All Blocks"))
                    .ToList();
            }
            return m_cachedCategories;
        }
    }
}