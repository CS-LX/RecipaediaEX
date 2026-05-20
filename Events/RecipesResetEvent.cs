namespace RecipaediaEX.Events {
    /// <summary>
    /// 静态配方总表重建完成时发布（<see cref="RecipaediaEXManager.ResetRecipes"/> 及初始化末尾）。
    /// </summary>
    public readonly struct RecipesResetEvent {
        public RecipesResetEvent(int recipeCount) {
            RecipeCount = recipeCount;
        }

        /// <summary>当前 <see cref="RecipaediaEXManager.Recipes"/> 条目数。</summary>
        public int RecipeCount { get; }
    }
}
