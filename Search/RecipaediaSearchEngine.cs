using System;
using System.Collections.Generic;
using RecipaediaEX.Implementation;
using RecipaediaEX.UI;
using ZLinq;

namespace RecipaediaEX.Search {
    public readonly struct SearchMatchResult {
        public IRecipaediaItem Item { get; init; }
        public int Score { get; init; }
    }

    public static class RecipaediaSearchEngine {
        public static List<SearchMatchResult> Filter(IEnumerable<IRecipaediaItem> candidates, string categoryId, string query) {
            SearchQuery parsed = RecipaediaSearchParser.Parse(query);
            if (parsed.TextTerms.Count == 0 && parsed.Clauses.Count == 0) {
                return candidates.AsValueEnumerable().Select(item => new SearchMatchResult { Item = item, Score = 0 }).ToList();
            }

            List<SearchMatchResult> results = [];
            foreach (IRecipaediaItem item in candidates) {
                ItemSearchDocument doc = RecipaediaSearchIndex.GetDocument(item, categoryId);
                if (!Matches(doc, parsed, out int score)) continue;
                results.Add(new SearchMatchResult { Item = item, Score = score });
            }

            results.Sort((a, b) => {
                int scoreCompare = b.Score.CompareTo(a.Score);
                if (scoreCompare != 0) return scoreCompare;
                ItemSearchDocument docA = RecipaediaSearchIndex.GetDocument(a.Item, categoryId);
                ItemSearchDocument docB = RecipaediaSearchIndex.GetDocument(b.Item, categoryId);
                int orderCompare = docA.DisplayOrder.CompareTo(docB.DisplayOrder);
                if (orderCompare != 0) return orderCompare;
                return string.Compare(docA.DisplayName, docB.DisplayName, StringComparison.OrdinalIgnoreCase);
            });
            return results;
        }

        static bool Matches(ItemSearchDocument doc, SearchQuery query, out int score) {
            score = 0;
            foreach (string term in query.TextTerms) {
                if (!MatchesText(doc, term, false, ref score)) return false;
            }
            foreach (SearchClause clause in query.Clauses) {
                bool matched = MatchesClause(doc, clause, ref score);
                if (clause.Exclude) {
                    if (matched) return false;
                }
                else if (!matched) return false;
            }
            return true;
        }

        static bool MatchesClause(ItemSearchDocument doc, SearchClause clause, ref int score) {
            return clause.Kind switch {
                SearchClauseKind.Text => MatchesText(doc, clause.Value, clause.Exclude, ref score),
                SearchClauseKind.NameExact => StringEquals(doc.DisplayName, clause.Value),
                SearchClauseKind.NameContains => Contains(doc.NormalizedName, RecipaediaSearchIndex.Normalize(clause.Value)),
                SearchClauseKind.ItemType => MatchesItemType(doc, clause.Value),
                SearchClauseKind.Pack => MatchesPack(doc, clause.Value),
                SearchClauseKind.Mod => Contains(doc.ModDisplayName, clause.Value),
                SearchClauseKind.Crafter => MatchesCrafter(doc, clause.Value),
                SearchClauseKind.Ingredient => MatchesIngredientEntry(doc, clause.Value),
                SearchClauseKind.Product => MatchesProductEntry(doc, clause.Value),
                SearchClauseKind.RecipeType => MatchesRecipeType(doc, clause.Value),
                SearchClauseKind.HasRecipe => doc.RecipeCountAsResult > 0,
                SearchClauseKind.CanUse => doc.RecipeCountAsIngredient > 0,
                _ => true,
            };
        }

        static bool MatchesText(ItemSearchDocument doc, string term, bool exclude, ref int score) {
            if (string.IsNullOrWhiteSpace(term)) return true;
            string normalized = RecipaediaSearchIndex.Normalize(term);
            bool matched = false;
            if (doc.NormalizedName.StartsWith(normalized, StringComparison.Ordinal)) {
                score += 100;
                matched = true;
            }
            else if (Contains(doc.NormalizedName, normalized)) {
                score += 50;
                matched = true;
            }
            if (!string.IsNullOrEmpty(doc.CraftingId)
                && doc.CraftingId.Contains(term, StringComparison.OrdinalIgnoreCase)) {
                score += 40;
                matched = true;
            }
            if (Contains(RecipaediaSearchIndex.Normalize(doc.DescriptionSnippet), normalized)) {
                score += 10;
                matched = true;
            }
            foreach (string tag in doc.Tags) {
                if (Contains(tag, term)) {
                    score += 20;
                    matched = true;
                }
            }
            return exclude ? !matched : matched;
        }

        static bool MatchesItemType(ItemSearchDocument doc, string value) {
            if (string.IsNullOrWhiteSpace(value)) return true;
            return value.ToLowerInvariant() switch {
                "block" => doc.Kind == ItemSearchKind.Block,
                "custom" => doc.Kind == ItemSearchKind.Custom,
                _ => doc.Kind.ToString().Contains(value, StringComparison.OrdinalIgnoreCase),
            };
        }

        static bool MatchesPack(ItemSearchDocument doc, string value) {
            if (string.IsNullOrWhiteSpace(value)) return true;
            return doc.PackId.StartsWith(value, StringComparison.OrdinalIgnoreCase)
                || StringEquals(doc.PackId, value);
        }

        static bool MatchesCrafter(ItemSearchDocument doc, string value) {
            int[] crafters = RecipaediaSearchIndex.ResolveCrafterBlockValuesByName(value);
            if (crafters.Length == 0) return false;
            foreach (int crafter in crafters) {
                if (doc.CrafterBlockValues.Contains(crafter)) return true;
            }
            return false;
        }

        static bool MatchesIngredientEntry(ItemSearchDocument doc, string value) {
            if (!RepresentsNamedItem(doc, value)) return false;
            return doc.RecipeCountAsIngredient > 0 || doc.IngredientBlockValues.Count > 0;
        }

        static bool MatchesProductEntry(ItemSearchDocument doc, string value) {
            if (!RepresentsNamedItem(doc, value)) return false;
            return doc.RecipeCountAsResult > 0 || doc.ResultBlockValues.Count > 0;
        }

        static bool RepresentsNamedItem(ItemSearchDocument doc, string value) {
            if (string.IsNullOrWhiteSpace(value)) return true;
            if (Contains(doc.DisplayName, value)) return true;
            int[] values = RecipaediaSearchIndex.ResolveBlockValuesByName(value);
            if (values.Length == 0) return false;
            if (doc.Item is BlockItem blockItem) {
                return values.AsValueEnumerable().Contains(blockItem.m_blockValue);
            }
            if (doc.Item.Value is int intValue) {
                return values.AsValueEnumerable().Contains(intValue);
            }
            return false;
        }

        static bool MatchesRecipeType(ItemSearchDocument doc, string value) {
            if (string.IsNullOrWhiteSpace(value)) return true;
            foreach (string typeName in doc.RecipeTypeNames) {
                if (typeName.Contains(value, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        static bool Contains(string source, string value) =>
            !string.IsNullOrEmpty(source)
            && !string.IsNullOrEmpty(value)
            && source.Contains(value, StringComparison.OrdinalIgnoreCase);

        static bool StringEquals(string source, string value) =>
            string.Equals(source, value, StringComparison.OrdinalIgnoreCase);
    }
}
