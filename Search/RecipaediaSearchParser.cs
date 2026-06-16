using System;
using System.Collections.Generic;

namespace RecipaediaEX.Search {
    public static class RecipaediaSearchParser {
        static readonly Dictionary<string, SearchClauseKind> m_ingredientAliases = new(StringComparer.OrdinalIgnoreCase) {
            ["in"] = SearchClauseKind.Ingredient,
            ["uses"] = SearchClauseKind.Ingredient,
        };
        static readonly Dictionary<string, SearchClauseKind> m_productAliases = new(StringComparer.OrdinalIgnoreCase) {
            ["out"] = SearchClauseKind.Product,
            ["makes"] = SearchClauseKind.Product,
        };

        public static SearchNode ParseExpression(string query) {
            if (string.IsNullOrWhiteSpace(query)) return SearchNode.EmptyAnd();
            List<string> tokens = Tokenize(query);
            int pos = 0;
            SearchNode root = ParseOr(tokens, ref pos);
            return root ?? SearchNode.EmptyAnd();
        }

        /// <summary>扁平 AND 模型，供 Filter Dialog 状态互转。</summary>
        public static SearchQuery Parse(string query) {
            SearchQuery result = new();
            FlattenAnd(ParseExpression(query), result);
            return result;
        }

        static void FlattenAnd(SearchNode node, SearchQuery result) {
            if (node == null) return;
            switch (node.Kind) {
                case SearchNodeKind.And:
                    foreach (SearchNode child in node.Children) FlattenAnd(child, result);
                    break;
                case SearchNodeKind.Text:
                    result.TextTerms.Add(node.Text);
                    break;
                case SearchNodeKind.Clause:
                    result.Clauses.Add(CloneClause(node.Clause));
                    break;
                case SearchNodeKind.Not when node.Children.Count > 0:
                    if (node.Children[0].Kind == SearchNodeKind.Text) {
                        result.Clauses.Add(new SearchClause {
                            Kind = SearchClauseKind.Text,
                            Value = node.Children[0].Text,
                            Exclude = true,
                        });
                    }
                    else if (node.Children[0].Kind == SearchNodeKind.Clause) {
                        SearchClause clause = CloneClause(node.Children[0].Clause);
                        clause.Exclude = true;
                        result.Clauses.Add(clause);
                    }
                    break;
            }
        }

        static SearchNode ParseOr(List<string> tokens, ref int pos) {
            SearchNode left = ParseAnd(tokens, ref pos);
            while (pos < tokens.Count && IsOrToken(tokens[pos])) {
                pos++;
                SearchNode right = ParseAnd(tokens, ref pos);
                left = SearchNode.Or([left, right]);
            }
            return left;
        }

        static SearchNode ParseAnd(List<string> tokens, ref int pos) {
            List<SearchNode> factors = [];
            while (pos < tokens.Count && !IsOrToken(tokens[pos]) && tokens[pos] != ")") {
                factors.Add(ParseFactor(tokens, ref pos));
            }
            return SearchNode.And(factors);
        }

        static SearchNode ParseFactor(List<string> tokens, ref int pos) {
            if (pos >= tokens.Count) return SearchNode.EmptyAnd();
            string token = tokens[pos++];
            if (token == "(") {
                SearchNode inner = ParseOr(tokens, ref pos);
                if (pos < tokens.Count && tokens[pos] == ")") pos++;
                return inner;
            }
            bool exclude = token.StartsWith('-');
            if (exclude) token = token[1..];
            if (token.Length == 0) return SearchNode.EmptyAnd();
            SearchNode node = TokenToNode(token);
            return exclude ? SearchNode.Not(node) : node;
        }

        static SearchNode TokenToNode(string token) {
            if (token.StartsWith("name=", StringComparison.OrdinalIgnoreCase)) {
                return SearchNode.ClauseTerm(new SearchClause { Kind = SearchClauseKind.NameExact, Value = token[5..] });
            }
            if (token.StartsWith("name:", StringComparison.OrdinalIgnoreCase)) {
                return SearchNode.ClauseTerm(new SearchClause { Kind = SearchClauseKind.NameContains, Value = token[5..] });
            }
            if (token.StartsWith('@')) {
                SearchClause clause = ParseFilterToken(token[1..]);
                return SearchNode.ClauseTerm(clause);
            }
            return SearchNode.TextTerm(token);
        }

        static SearchClause ParseFilterToken(string token) {
            int sep = token.IndexOf(':');
            string key = sep >= 0 ? token[..sep] : token;
            string value = sep >= 0 ? token[(sep + 1)..] : string.Empty;

            int opSep = key.IndexOfAny(['>', '<', '=']);
            if (opSep > 0 && key.StartsWith("recipes", StringComparison.OrdinalIgnoreCase)) {
                value = key[opSep..] + value;
                key = key[..opSep];
            }

            if (key.Equals("recipes", StringComparison.OrdinalIgnoreCase)
                && TryParseComparison(value, out SearchCompareOp op, out int count)) {
                return new SearchClause { Kind = SearchClauseKind.RecipeCount, CompareOp = op, CompareValue = count };
            }
            if (key.Equals("has", StringComparison.OrdinalIgnoreCase)
                && value.Equals("recipe", StringComparison.OrdinalIgnoreCase)) {
                return new SearchClause { Kind = SearchClauseKind.HasRecipe };
            }
            if (key.Equals("can-use", StringComparison.OrdinalIgnoreCase)
                || key.Equals("canuse", StringComparison.OrdinalIgnoreCase)) {
                return new SearchClause { Kind = SearchClauseKind.CanUse };
            }
            if (key.Equals("t", StringComparison.OrdinalIgnoreCase)) {
                return new SearchClause { Kind = SearchClauseKind.ItemType, Value = value };
            }
            if (key.Equals("pack", StringComparison.OrdinalIgnoreCase)) {
                return new SearchClause { Kind = SearchClauseKind.Pack, Value = value };
            }
            if (key.Equals("mod", StringComparison.OrdinalIgnoreCase)) {
                return new SearchClause { Kind = SearchClauseKind.Mod, Value = value };
            }
            if (key.Equals("crafter", StringComparison.OrdinalIgnoreCase)) {
                return new SearchClause { Kind = SearchClauseKind.Crafter, Value = value };
            }
            if (key.Equals("rtype", StringComparison.OrdinalIgnoreCase)) {
                return new SearchClause { Kind = SearchClauseKind.RecipeType, Value = value };
            }
            if (m_ingredientAliases.TryGetValue(key, out SearchClauseKind ingredientKind)) {
                return new SearchClause { Kind = ingredientKind, Value = value };
            }
            if (m_productAliases.TryGetValue(key, out SearchClauseKind productKind)) {
                return new SearchClause { Kind = productKind, Value = value };
            }
            return new SearchClause { Kind = SearchClauseKind.Text, Value = token };
        }

        static bool TryParseComparison(string value, out SearchCompareOp op, out int count) {
            op = SearchCompareOp.GreaterOrEqual;
            count = 0;
            if (string.IsNullOrWhiteSpace(value)) return false;
            value = value.Trim();
            if (value.StartsWith(">=", StringComparison.Ordinal)) {
                op = SearchCompareOp.GreaterOrEqual;
                value = value[2..];
            }
            else if (value.StartsWith("<=", StringComparison.Ordinal)) {
                op = SearchCompareOp.LessOrEqual;
                value = value[2..];
            }
            else if (value.StartsWith("!=", StringComparison.Ordinal)) {
                op = SearchCompareOp.NotEqual;
                value = value[2..];
            }
            else if (value.StartsWith('>')) {
                op = SearchCompareOp.Greater;
                value = value[1..];
            }
            else if (value.StartsWith('<')) {
                op = SearchCompareOp.Less;
                value = value[1..];
            }
            else if (value.StartsWith('=')) {
                op = SearchCompareOp.Equal;
                value = value[1..];
            }
            return int.TryParse(value.Trim(), out count);
        }

        static bool IsOrToken(string token) => token.Equals("or", StringComparison.OrdinalIgnoreCase);

        static List<string> Tokenize(string query) {
            List<string> tokens = [];
            int i = 0;
            while (i < query.Length) {
                while (i < query.Length && char.IsWhiteSpace(query[i])) i++;
                if (i >= query.Length) break;

                char c = query[i];
                if (c is '(' or ')') {
                    tokens.Add(c.ToString());
                    i++;
                    continue;
                }

                if (c == '"') {
                    i++;
                    int start = i;
                    while (i < query.Length && query[i] != '"') i++;
                    tokens.Add(query[start..i]);
                    if (i < query.Length) i++;
                    continue;
                }

                int plainStart = i;
                while (i < query.Length && !char.IsWhiteSpace(query[i]) && query[i] is not '(' and not ')') i++;
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

        static SearchClause CloneClause(SearchClause clause) => new() {
            Kind = clause.Kind,
            Value = clause.Value,
            Exclude = clause.Exclude,
            CompareOp = clause.CompareOp,
            CompareValue = clause.CompareValue,
        };
    }
}
