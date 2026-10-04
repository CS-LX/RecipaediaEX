namespace RecipaediaEX {
    /// <summary>
    /// 表示一个配方
    /// </summary>
    public interface IRecipe {
        /// <summary>
        /// 在配方表中的显示顺序，DisplayOrder越小，配方越靠前
        /// </summary>
        int DisplayOrder { get; }

        /// <summary>
        /// 匹配的优先级，越高越先被匹配上
        /// </summary>
        int MatchPriority { get; }

        /// <summary>
        /// 该配方是否允许以「副产物(<c>Remains</c>)」的身份展示给对应图鉴条目。
        /// <para>默认 <c>false</c>：配方只在主产物(<c>Result</c>)的条目下出现，不会因副产物身份被展示。</para>
        /// <para>自定义配方可重写本方法自行判定，或写入 Extra 键
        /// <see cref="RecipeExtraKeys.DisplaysAsRemains"/>（<c>bool</c>）——本方法的默认实现即读取该键。
        /// 用于让「只能作为副产物获得」的材料也能查到本配方。</para>
        /// </summary>
        /// <returns>true 表示本配方允许以副产物身份展示。</returns>
        bool DisplaysAsRemains() => GetExtraValue(RecipeExtraKeys.DisplaysAsRemains, false);

        /// <summary>
        /// 是否与其他配方匹配
        /// </summary>
        /// <param name="actual">实际上的配方(玩家放入的)</param>
        /// <returns></returns>
        bool Match(IRecipe actual);
        
        /// <summary>
        /// 配方中应维护一个容器（强烈建议ValuesDictionary），用于储存额外的信息，可用于跨模组通信、拓展配方匹配机制等
        /// <para>这个方法用于读取容器中值</para>
        /// <para>约定键名见 <see cref="RecipeExtraKeys"/>。</para>
        /// </summary>
        /// <param name="key">值对应的键</param>
        /// <param name="defaultValue">默认值</param>
        /// <typeparam name="T">值的类型</typeparam>
        /// <returns>读取到的值</returns>
        T GetExtraValue<T>(string key, T defaultValue);
        
        /// <summary>
        /// 配方中应维护一个容器（强烈建议ValuesDictionary），用于储存额外的信息，可用于跨模组通信、拓展配方匹配机制等
        /// <para>这个方法用于设置容器中值</para>
        /// <para>约定键名见 <see cref="RecipeExtraKeys"/>。</para>
        /// </summary>
        /// <param name="key">值对应的键</param>
        /// <param name="value">值</param>
        /// <typeparam name="T">值的类型</typeparam>
        void SetExtraValue<T>(string key, T value);
    }
}