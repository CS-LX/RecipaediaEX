using System.Collections.Generic;
using System.Linq;
using Game;
using RecipaediaEX.UI;

namespace RecipaediaEX.Implementation {
    public class BlocksCategoryProvider : IRecipaediaCategoryProvider {
        public IEnumerable<IRecipaediaCategory> GetCategories() => BlocksManager.Categories.Select(x => new BlocksCategory(x)).Prepend(new BlocksCategory("All Blocks"));
    }
}