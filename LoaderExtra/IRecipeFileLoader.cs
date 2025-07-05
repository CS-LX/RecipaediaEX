using System.Xml.Linq;
using Game;

namespace RecipaediaEX {
    /// <summary>
    /// 为Mod提供自定义读取配方xml文件的好东西
    /// </summary>
    public interface IRecipeFileLoader {
        /// <summary>
        /// 从ModEntity中获取配方的xml文件数据
        /// </summary>
        /// <param name="modEntity"></param>
        /// <returns></returns>
        public XElement GetRecipesXml(ModEntity modEntity);

        /// <summary>
        /// 此读取器的优先级
        /// <para>对于目标mod包名一致的读取器，优先级高的会覆盖优先级低的；若优先级相等，则被RecipaediaEX后发现的读取器会覆盖先前发现的那个</para>
        /// </summary>
        public int Order { get; }
    }
}