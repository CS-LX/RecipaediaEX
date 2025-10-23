using System;
using System.Collections.Generic;
using Game;

namespace RecipaediaEX.UI {
    /// <summary>
    /// 表示一个分类的配置体
    /// </summary>
    public interface IRecipaediaCategory {
        /// <summary>
        /// 分类ID，一般来说可以选择分类的英文名
        /// </summary>
        string Id { get; }

        /// <summary>
        /// 分类的显示用名称
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// 返回该分类下所有图鉴条目
        /// </summary>
        /// <returns>该分类下所有图鉴条目</returns>
        IEnumerable<IRecipaediaItem> GetItems();

        /// <summary>
        /// 定义这个分类如何展示一个Item
        /// </summary>
        Func<IRecipaediaItem, Widget> ItemWidgetFactory { get; }
    }
}