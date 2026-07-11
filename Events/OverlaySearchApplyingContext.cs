using RecipaediaEX.Overlay;

namespace RecipaediaEX.Events {
    /// <summary>
    /// 合成助手搜索框即将应用查询并刷新列表之前。附属模组可修改 <see cref="SearchQuery"/> 注入默认筛选。
    /// </summary>
    public sealed class OverlaySearchApplyingContext {
        public OverlaySearchApplyingContext(
            RecipaediaCraftingContext craftingContext,
            string searchQuery,
            bool commitHistory) {
            CraftingContext = craftingContext;
            SearchQuery = searchQuery;
            CommitHistory = commitHistory;
        }

        public RecipaediaCraftingContext CraftingContext { get; }
        public string SearchQuery { get; set; }
        public bool CommitHistory { get; }
    }
}
