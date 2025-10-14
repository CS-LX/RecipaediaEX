using Game;
using RecipaediaEX.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RecipaediaEX.LoaderExtra.Loaders {
    public class SurvivalcraftRecipesLoader : XmlRecipesLoader {
        public override void Initialize() {
            XElement survivalcraftRecipesElement = ContentManager.Get<XElement>("CraftingRecipes");
            if (survivalcraftRecipesElement != null)
                XElements.Add(survivalcraftRecipesElement);
            else Engine.Log.Warning("[RecipaediaEX]Not Found SurvivalcraftRecipesElement!");
        }
        public override int Order => -20;
    }
}
