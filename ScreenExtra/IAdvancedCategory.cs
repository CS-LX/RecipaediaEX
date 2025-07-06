using Engine;
using Game;

namespace RecipaediaEX {
    /// <summary>
    /// 更高级的分类配置体
    /// <para>它本质上是IRecipaediaCategory，不过你可以自定义更多属性</para>
    /// </summary>
    public interface IAdvancedCategory : IRecipaediaCategory {
        /// <summary>
        /// 列表的条目大小
        /// </summary>
        public float ListItemSize { get; }

        /// <summary>
        /// 列表的平铺方向
        /// </summary>
        public LayoutDirection ListDirection { get; }
    }
}