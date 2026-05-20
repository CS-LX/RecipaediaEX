using GameEntitySystem;

namespace RecipaediaEX.Events {
    /// <summary>
    /// <see cref="RecipaediaEXManager.FindMatchingRecipe"/> / 动态配方链成功解析出配方时发布。
    /// </summary>
    public readonly struct RecipeMatchedEvent {
        public RecipeMatchedEvent(IRecipe actual, IRecipe matched, bool fromDynamicLoader, Project? project) {
            Actual = actual;
            Matched = matched;
            FromDynamicLoader = fromDynamicLoader;
            Project = project;
        }

        /// <summary>用于匹配的「实际放置」配方快照。</summary>
        public IRecipe Actual { get; }
        /// <summary>匹配到的完整配方定义。</summary>
        public IRecipe Matched { get; }
        /// <summary>是否来自 <see cref="IDynamicRecipeLoader"/> 链（否则为静态总表）。</summary>
        public bool FromDynamicLoader { get; }
        public Project? Project { get; }
    }
}
