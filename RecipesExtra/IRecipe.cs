namespace RecipaediaEX {
    /// <summary>
    /// 表示一个配方
    /// </summary>
    public interface IRecipe {
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
        
        /// <summary>
        /// 配方中应维护一个容器（强烈建议ValuesDictionary），用于储存额外的信息，可用于跨模组通信、拓展配方匹配机制等
        /// <para>这个方法用于读取容器中值</para>
        /// </summary>
        /// <param name="key">值对应的键</param>
        /// <param name="defaultValue">默认值</param>
        /// <typeparam name="T">值的类型</typeparam>
        /// <returns>读取到的值</returns>
        public T GetExtraValue<T>(string key, T defaultValue);
        
        /// <summary>
        /// 配方中应维护一个容器（强烈建议ValuesDictionary），用于储存额外的信息，可用于跨模组通信、拓展配方匹配机制等
        /// <para>这个方法用于设置容器中值</para>
        /// </summary>
        /// <param name="key">值对应的键</param>
        /// <param name="value">值</param>
        /// <typeparam name="T">值的类型</typeparam>
        public void SetExtraValue<T>(string key, T value);
    }
}