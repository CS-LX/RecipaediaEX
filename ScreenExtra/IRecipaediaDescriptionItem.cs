using System.Collections.Generic;
using Game;

namespace RecipaediaEX.UI {
    /// <summary>
    /// 用RecipaediaEXDescriptionScreen展示的IRecipaediaItem就要接上这个接口
    /// </summary>
    public interface IRecipaediaDescriptionItem {
        /// <summary>
        /// RecipaediaEXDescriptionScreen展示的名称
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// RecipaediaEXDescriptionScreen展示的图标
        /// </summary>
        public Widget Icon { get; }
        /// <summary>
        /// RecipaediaEXDescriptionScreen展示的详细信息
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// RecipaediaEXDescriptionScreen展示的属性
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetProperties();
    }
}