namespace RecipaediaEX {
    /// <summary>
    /// <see cref="IRecipe"/> 的 <c>GetExtraValue</c> / <c>SetExtraValue</c> 约定键名集合。
    /// </summary>
    public static class RecipeExtraKeys {
        /// <summary>图鉴产物条目 <c>Match</c>：可产出该配方的方块 <c>blockValue</c> 列表（<c>int[]</c>）。</summary>
        public const string MatchedResultBlockValues = "MatchedResultBlockValues";

        /// <summary>图鉴原料条目 <c>IsIngredient</c>：可作为原料的方块 <c>blockValue</c> 列表（<c>int[]</c>）。</summary>
        public const string MatchedIngredientBlockValues = "MatchedIngredientBlockValues";

        /// <summary>当前世界 <see cref="GameEntitySystem.Project"/>；<see cref="RecipaediaEXManager.FindMatchingRecipe{T}"/> 据此走动态配方链。</summary>
        public const string Project = "Project";

        /// <summary>工作台/熔炉槽位快照：各格方块 <c>blockValue</c>（<c>int?[]</c>，空槽为 <c>null</c>）。</summary>
        public const string ActualIngredients = "ActualIngredients";

        /// <summary>发起匹配的工作台/熔炉库存（<see cref="Game.IInventory"/>）。</summary>
        public const string Inventory = "Inventory";
    }
}
