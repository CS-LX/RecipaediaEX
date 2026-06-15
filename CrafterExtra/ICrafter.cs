namespace RecipaediaEX {
    /// <summary>
    /// 表示一个制造配方的制造站
    /// </summary>
    public interface ICrafter {
        /// <summary>
        /// 这个方块的某一特殊值方块是否为目标配方类型的Crafter
        /// </summary>
        /// <param name="blockValue">这个方块的特殊值方块的完整值</param>
        /// <param name="recipe">配方实例</param>
        /// <returns></returns>
        bool IsCrafter(int blockValue, IRecipe recipe);
    }
}