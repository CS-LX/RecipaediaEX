using System.Collections.Generic;
using RecipaediaEX.UI;

namespace RecipaediaEX.Search {
    public enum ItemSearchKind {
        Block,
        Custom
    }

    /// <summary>图鉴条目搜索索引文档。</summary>
    public sealed class ItemSearchDocument {
        public IRecipaediaItem Item { get; init; }
        public string CategoryId { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public string NormalizedName { get; init; } = string.Empty;
        public string PinyinFull { get; init; } = string.Empty;
        public string PinyinInitials { get; init; } = string.Empty;
        public string DescriptionSnippet { get; init; } = string.Empty;
        public ItemSearchKind Kind { get; init; }
        public string PackId { get; init; } = string.Empty;
        public string ModDisplayName { get; init; } = string.Empty;
        public string CraftingId { get; init; } = string.Empty;
        public int DisplayOrder { get; init; }
        public int RecipeCountAsResult { get; init; }
        public int RecipeCountAsIngredient { get; init; }
        public HashSet<int> CrafterBlockValues { get; init; } = [];
        public HashSet<string> RecipeTypeNames { get; init; } = [];
        public HashSet<int> ResultBlockValues { get; init; } = [];
        public HashSet<int> IngredientBlockValues { get; init; } = [];
        public List<string> Tags { get; init; } = [];
    }
}
