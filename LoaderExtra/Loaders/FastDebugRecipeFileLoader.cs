using System.Xml.Linq;
using Engine;
using Game;

namespace RecipaediaEX.Implementation {
    [RecipeFileLoader("com.fastdebug")]
    public class FastDebugRecipeFileLoader : IRecipeFileLoader {
        public XElement GetRecipesXml(ModEntity modEntity) {
            XElement xElement = new("Recipes");
            foreach (string c in Storage.ListFileNames(ModsManager.ModsPath)) {
                if (c.EndsWith(".cr")) ModsManager.CombineCr(xElement, Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, c), OpenFileMode.Read));
            }
            return xElement;
        }
        public int Order => 0;
    }
}