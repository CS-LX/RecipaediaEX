using System.Xml.Linq;

namespace RecipaediaEX {
    /// <summary>
    /// 将xml条目读取为配方实例的解析器
    /// </summary>
    public interface IRecipeReader {
        /// <summary>
        /// <para>从xml条目中加载配方</para>
        /// <para>xml条目如下：</para>
        /// <para>&lt;Recipe Result="MarbleBlock" ResultCount="1" RequiredHeatLevel="1" a="limestone" b="sand" Description="[0]"&gt;</para>
        ///      "ab"
        /// <para>&lt;/Recipe&gt;</para>
        /// </summary>
        /// <param name="item">xml条目</param>
        /// <returns></returns>
        public IRecipe LoadRecipe(XElement item);
    }
}