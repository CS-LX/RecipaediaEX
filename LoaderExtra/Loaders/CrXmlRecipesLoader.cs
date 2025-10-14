using Game;
using RecipaediaEX.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RecipaediaEX.LoaderExtra.Loaders {
    /// <summary>
    /// 读取所有以.cr为后缀名的文件配方
    /// </summary>
    class CrXmlRecipesLoader : XmlRecipesLoader, IRecipesLoader {
        public override int Order => -10;
        public override void Initialize() {
            ControlledFileExtensionName = ".cr";
            base.Initialize();
        }
    }
}
