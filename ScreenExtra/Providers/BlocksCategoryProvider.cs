using System.Collections.Generic;
using System.Linq;
using Game;

namespace RecipaediaEX {
    public class BlocksCategoryProvider : IRecipaediaCategoryProvider {
        public IEnumerable<IRecipaediaCategory> GetCategories() => BlocksManager.Categories.Select(x => new BlocksCategory(x)).Prepend(new BlocksCategory("All Blocks"));
    }
}