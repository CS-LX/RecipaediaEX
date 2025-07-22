using System.Text;
using System.Xml.Linq;
using Game;
using XmlUtilities;

namespace RecipaediaEX.Implementation {
    /// <summary>
    /// <para>默认的配方文件读取器</para>
    /// <para>某个Mod如果不适用自定义的读取器（也就是程序集中没有其对应的读取器），则RecipaediaEX通过这个默认读取器读取配方文件</para>
    /// <para>此读取器优先级是0</para>
    /// </summary>
    public class DefaultRecipeFileLoader : IRecipeFileLoader {
        public XElement GetRecipesXml(ModEntity modEntity) {
            XElement xElement = null;
            modEntity.GetFiles(".cr", (_, stream) => { xElement = XmlUtils.LoadXmlFromStream(stream, Encoding.UTF8, true); });
            return xElement;
        }

        public int Order => 0;
    }
}