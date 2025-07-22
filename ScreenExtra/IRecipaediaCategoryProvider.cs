using System.Collections.Generic;

namespace RecipaediaEX.UI {
    /// <summary>
    /// 提供图鉴分类数据的供给器
    /// <para>注：此供给器必须有一个无参构造函数，并且这个供给器在游戏中是以单例存在的</para>
    /// </summary>
    public interface IRecipaediaCategoryProvider {
        /// <summary>
        /// 返回要添加在图鉴界面里的所有分类的供给器
        /// </summary>
        /// <returns>要添加在图鉴界面里的所有分类的供给器</returns>
        IEnumerable<IRecipaediaCategory> GetCategories();
    }
}