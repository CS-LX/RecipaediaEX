using System.Collections.Generic;

namespace RecipaediaEX.Search {
    public enum SearchNodeKind {
        And,
        Or,
        Not,
        Text,
        Clause,
    }

    /// <summary>图鉴搜索 AST 节点（Phase 2：<c>or</c> / <c>()</c>）。</summary>
    public sealed class SearchNode {
        public SearchNodeKind Kind;
        public string Text = string.Empty;
        public SearchClause Clause = new();
        public List<SearchNode> Children { get; } = [];

        public static SearchNode And(IEnumerable<SearchNode> children) => Combine(SearchNodeKind.And, children);
        public static SearchNode Or(IEnumerable<SearchNode> children) => Combine(SearchNodeKind.Or, children);

        public static SearchNode Not(SearchNode child) => new() { Kind = SearchNodeKind.Not, Children = { child } };

        public static SearchNode TextTerm(string text) => new() { Kind = SearchNodeKind.Text, Text = text ?? string.Empty };

        public static SearchNode ClauseTerm(SearchClause clause) => new() { Kind = SearchNodeKind.Clause, Clause = clause };

        public static SearchNode EmptyAnd() => new() { Kind = SearchNodeKind.And };

        static SearchNode Combine(SearchNodeKind kind, IEnumerable<SearchNode> children) {
            List<SearchNode> list = [];
            foreach (SearchNode child in children) {
                if (child == null) continue;
                if (child.Kind == kind) list.AddRange(child.Children);
                else list.Add(child);
            }
            if (list.Count == 0) return EmptyAnd();
            if (list.Count == 1) return list[0];
            SearchNode node = new() { Kind = kind };
            node.Children.AddRange(list);
            return node;
        }
    }
}
