using System.Collections.Generic;
using RecipaediaEX.UI;

namespace RecipaediaEX.Search {
    /// <summary>图鉴条目与配方的关联索引；按需构建，避免简单文本搜索扫描全库配方。</summary>
    public sealed class RecipeSearchMetadata {
        public static readonly RecipeSearchMetadata Empty = new();

        public int RecipeCountAsResult { get; init; }
        public int RecipeCountAsIngredient { get; init; }
        public HashSet<int> CrafterBlockValues { get; init; } = [];
        public HashSet<string> RecipeTypeNames { get; init; } = [];
        public HashSet<int> ResultBlockValues { get; init; } = [];
        public HashSet<int> IngredientBlockValues { get; init; } = [];
    }
}
