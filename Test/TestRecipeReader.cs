using System.Xml.Linq;
using XmlUtilities;

namespace RecipaediaEX.Test {
    [RecipeReader([typeof(TestRecipe)])]
    public class TestRecipeReader : IRecipeReader {
        public IRecipe LoadRecipe(XElement item) {
            TestRecipe recipe = new() { Attribute = XmlUtils.GetAttributeValue<int>(item, "Attribute"), Value = item.Value };
            return recipe;
        }
    }
}