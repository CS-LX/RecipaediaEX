using Game;
using RecipaediaEX.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
