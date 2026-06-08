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
        CanUse
    }

    public sealed class SearchClause {
        public SearchClauseKind Kind;
        public string Value = string.Empty;
        public bool Exclude;
    }

    public sealed class SearchQuery {
        public List<SearchClause> Clauses { get; } = [];
        public List<string> TextTerms { get; } = [];
    }
}
