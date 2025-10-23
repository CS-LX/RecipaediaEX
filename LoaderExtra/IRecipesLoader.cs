using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Game;

namespace RecipaediaEX {
    /// <summary>
    /// 为Mod提供自定义读取配方xml文件的东西
    /// </summary>
    public interface IRecipesLoader {
        /// <summary>
        /// RecipaediaEXManager首次加载时调用
        /// </summary>
        void Initialize();
        /// <summary>
        /// 在游戏首次加载（Initialize完成后）或进入存档时调用
        /// </summary>
        /// <returns>返回模组提供的配方列表</returns>
        IEnumerable<IRecipe> GetRecipes();
        /// <summary>
        /// 此读取器的优先级
        /// <para>对于目标mod包名一致的读取器，优先级高的会覆盖优先级低的；若优先级相等，则被RecipaediaEX后发现的读取器会覆盖先前发现的那个</para>
        /// </summary>
        int Order { get; }
    }
}