using System.Xml.Linq;
using Game;

namespace RecipaediaEX {
    [RecipeFileLoader("survivalcraft")]
    public class SurvivalcraftRecipeFileLoader : IRecipeFileLoader {

        public XElement GetRecipesXml(ModEntity modEntity) => ContentManager.Get<XElement>("CraftingRecipes");

        public int Order => 0;
    }
}