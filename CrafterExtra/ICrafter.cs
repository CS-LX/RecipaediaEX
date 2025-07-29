using System;

namespace RecipaediaEX {
    /// <summary>
    /// 表示一个制造配方的制造站
    /// </summary>
    public interface ICrafter {
        /// <summary>
        /// 这个方块的某一特殊值方块是否为目标配方类型的Crafter
        /// </summary>
        /// <param name="blockValue">这个方块的特殊值方块的完整值</param>
        /// <param name="recipeType">配方类型</param>
        /// <returns></returns>
        public bool IsCrafter(int blockValue, Type recipeType);
    }
}