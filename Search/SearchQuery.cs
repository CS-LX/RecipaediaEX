using System.Collections.Generic;

namespace RecipaediaEX.Search {
    public enum SearchClauseKind {
        Text,
        NameExact,
        NameContains,
        ItemType,
        Pack,
        Mod,
        Crafter,
        Ingredient,
        Product,
        RecipeType,
        HasRecipe,
        CanUse,
        RecipeCount,
    }

    public enum SearchCompareOp {
        Equal,
        NotEqual,
        Greater,
        GreaterOrEqual,
        Less,
        LessOrEqual,
    }

    public sealed class SearchClause {
        public SearchClauseKind Kind;
        public string Value = string.Empty;
        public bool Exclude;
        public SearchCompareOp CompareOp = SearchCompareOp.GreaterOrEqual;
        public int CompareValue;
    }

    /// <summary>兼容 Filter Dialog 往返与旧调用方；执行过滤请用 <see cref="SearchNode"/>。</summary>
    public sealed class SearchQuery {
        public List<SearchClause> Clauses { get; } = [];
        public List<string> TextTerms { get; } = [];
    }
}
