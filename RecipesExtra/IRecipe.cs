namespace RecipaediaEX {
    /// <summary>
    /// 表示一个配方
    /// </summary>
    public interface IRecipe {
        /// <summary>
        /// 配方的解释
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// 配方无法合成时向玩家提供的消息
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 在配方表中的显示顺序，DisplayOrder越小，配方越靠前
        /// </summary>
        public int DisplayOrder { get; }

        /// <summary>
        /// 是否与其他配方匹配
        /// </summary>
        /// <param name="actual">实际上的配方(玩家放入的)</param>
        /// <returns></returns>
        public bool Match(IRecipe actual);
    }
}