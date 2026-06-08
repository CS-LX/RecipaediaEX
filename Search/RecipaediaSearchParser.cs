using System;
using System.Collections.Generic;

namespace RecipaediaEX.Search {
    public static class RecipaediaSearchParser {
        static readonly Dictionary<string, SearchClauseKind> s_ingredientAliases = new(StringComparer.OrdinalIgnoreCase) {
            ["in"] = SearchClauseKind.Ingredient,
            ["uses"] = SearchClauseKind.Ingredient,
        };
        static readonly Dictionary<string, SearchClauseKind> s_productAliases = new(StringComparer.OrdinalIgnoreCase) {
            ["out"] = SearchClauseKind.Product,
            ["makes"] = SearchClauseKind.Product,
        };

        public static SearchQuery Parse(string query) {
            SearchQuery result = new();
            if (string.IsNullOrWhiteSpace(query)) return result;

            foreach (string rawToken in Tokenize(query)) {
                if (string.IsNullOrWhiteSpace(rawToken)) continue;

                bool exclude = rawToken.StartsWith('-');
                string token = exclude ? rawToken[1..] : rawToken;

                if (token.StartsWith("name=", StringComparison.OrdinalIgnoreCase)) {
                    result.Clauses.Add(new SearchClause { Kind = SearchClauseKind.NameExact, Value = token[5..], Exclude = exclude });
                    continue;
                }
                if (token.StartsWith("name:", StringComparison.OrdinalIgnoreCase)) {
                    result.Clauses.Add(new SearchClause { Kind = SearchClauseKind.NameContains, Value = token[5..], Exclude = exclude });
                    continue;
                }
                if (token.StartsWith('@')) {
                    ParseFilterToken(token[1..], exclude, result);
                    continue;
                }

                if (exclude) {
                    result.Clauses.Add(new SearchClause { Kind = SearchClauseKind.Text, Value = token, Exclude = true });
                }
                else {
                    result.TextTerms.Add(token);
                }
            }

            return result;
        }

        static void ParseFilterToken(string token, bool exclude, SearchQuery result) {
            int sep = token.IndexOf(':');
            string key = sep >= 0 ? token[..sep] : token;
            string value = sep >= 0 ? token[(sep + 1)..] : string.Empty;

            if (key.Equals("has", StringComparison.OrdinalIgnoreCase)
                && value.Equals("recipe", StringComparison.OrdinalIgnoreCase)) {
                result.Clauses.Add(new SearchClause { Kind = SearchClauseKind.HasRecipe, Exclude = exclude });
                return;
            }
            if (key.Equals("can-use", StringComparison.OrdinalIgnoreCase)
                || key.Equals("canuse", StringComparison.OrdinalIgnoreCase)) {
                result.Clauses.Add(new SearchClause { Kind = SearchClauseKind.CanUse, Exclude = exclude });
                return;
            }
            if (key.Equals("t", StringComparison.OrdinalIgnoreCase)) {
                result.Clauses.Add(new SearchClause { Kind = SearchClauseKind.ItemType, Value = value, Exclude = exclude });
                return;
            }
            if (key.Equals("pack", StringComparison.OrdinalIgnoreCase)) {
                result.Clauses.Add(new SearchClause { Kind = SearchClauseKind.Pack, Value = value, Exclude = exclude });
                return;
            }
            if (key.Equals("mod", StringComparison.OrdinalIgnoreCase)) {
                result.Clauses.Add(new SearchClause { Kind = SearchClauseKind.Mod, Value = value, Exclude = exclude });
                return;
            }
            if (key.Equals("crafter", StringComparison.OrdinalIgnoreCase)) {
                result.Clauses.Add(new SearchClause { Kind = SearchClauseKind.Crafter, Value = value, Exclude = exclude });
                return;
            }
            if (key.Equals("rtype", StringComparison.OrdinalIgnoreCase)) {
                result.Clauses.Add(new SearchClause { Kind = SearchClauseKind.RecipeType, Value = value, Exclude = exclude });
                return;
            }
            if (s_ingredientAliases.TryGetValue(key, out SearchClauseKind ingredientKind)) {
                result.Clauses.Add(new SearchClause { Kind = ingredientKind, Value = value, Exclude = exclude });
                return;
            }
            if (s_productAliases.TryGetValue(key, out SearchClauseKind productKind)) {
                result.Clauses.Add(new SearchClause { Kind = productKind, Value = value, Exclude = exclude });
            }
        }

        static IEnumerable<string> Tokenize(string query) {
            List<string> tokens = [];
            int i = 0;
            while (i < query.Length) {
                while (i < query.Length && char.IsWhiteSpace(query[i])) i++;
                if (i >= query.Length) break;

                if (query[i] == '"') {
                    i++;
                    int start = i;
                    while (i < query.Length && query[i] != '"') i++;
                    tokens.Add(query[start..i]);
                    if (i < query.Length) i++;
                    continue;
                }

                int plainStart = i;
                while (i < query.Length && !char.IsWhiteSpace(query[i])) i++;
                tokens.Add(query[plainStart..i]);
            }
            return tokens;
        }

        public static string BuildQuery(RecipaediaSearchFilterState state) {
            List<string> parts = [];
            if (!string.IsNullOrWhiteSpace(state.NameText)) parts.Add(state.NameText.Trim());
            if (state.HasRecipe) parts.Add("@has:recipe");
            if (state.CanUse) parts.Add("@can-use");
            if (!string.IsNullOrWhiteSpace(state.ItemType)) parts.Add($"@t:{state.ItemType}");
            if (!string.IsNullOrWhiteSpace(state.PackId)) parts.Add($"@pack:{state.PackId}");
            if (!string.IsNullOrWhiteSpace(state.ModName)) parts.Add($"@mod:{state.ModName}");
            if (!string.IsNullOrWhiteSpace(state.CrafterName)) parts.Add($"@crafter:{state.CrafterName}");
            if (!string.IsNullOrWhiteSpace(state.RecipeType)) parts.Add($"@rtype:{state.RecipeType}");
            if (!string.IsNullOrWhiteSpace(state.IngredientName)) parts.Add($"@in:{state.IngredientName}");
            if (!string.IsNullOrWhiteSpace(state.ProductName)) parts.Add($"@out:{state.ProductName}");
            if (!string.IsNullOrWhiteSpace(state.ExcludeText)) parts.Add($"-{state.ExcludeText.Trim()}");
            return string.Join(' ', parts);
        }

        public static RecipaediaSearchFilterState ParseToFilterState(string query) {
            SearchQuery parsed = Parse(query);
            RecipaediaSearchFilterState state = new();
            foreach (string term in parsed.TextTerms) {
                if (string.IsNullOrEmpty(state.NameText)) state.NameText = term;
                else state.NameText += ' ' + term;
            }
            foreach (SearchClause clause in parsed.Clauses) {
                switch (clause.Kind) {
                    case SearchClauseKind.NameExact:
                    case SearchClauseKind.NameContains:
                        state.NameText = clause.Value;
                        break;
                    case SearchClauseKind.HasRecipe:
                        state.HasRecipe = !clause.Exclude;
                        break;
                    case SearchClauseKind.CanUse:
                        state.CanUse = !clause.Exclude;
                        break;
                    case SearchClauseKind.ItemType:
                        state.ItemType = clause.Value;
                        break;
                    case SearchClauseKind.Pack:
                        state.PackId = clause.Value;
                        break;
                    case SearchClauseKind.Mod:
                        state.ModName = clause.Value;
                        break;
                    case SearchClauseKind.Crafter:
                        state.CrafterName = clause.Value;
                        break;
                    case SearchClauseKind.RecipeType:
                        state.RecipeType = clause.Value;
                        break;
                    case SearchClauseKind.Ingredient:
                        state.IngredientName = clause.Value;
                        break;
                    case SearchClauseKind.Product:
                        state.ProductName = clause.Value;
                        break;
                    case SearchClauseKind.Text when clause.Exclude:
                        state.ExcludeText = clause.Value;
                        break;
                }
            }
            return state;
        }
    }
}
