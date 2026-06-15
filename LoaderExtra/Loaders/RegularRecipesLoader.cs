using Game;
using System.Xml.Linq;

namespace RecipaediaEX.Implementation {
    public class RegularRecipesLoader : XmlRecipesLoader {
        public override void Initialize() {
            XElement xElement = null;
            foreach (ModEntity modEntity in ModsManager.ModList) {
                modEntity.LoadCr(ref xElement);
            }
            XElements.Add(xElement);
        }
        public override int Order => -20;
    }
}
