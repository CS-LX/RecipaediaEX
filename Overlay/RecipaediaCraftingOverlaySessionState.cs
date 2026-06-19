namespace RecipaediaEX.Overlay {
    /// <summary>
    /// 合成助手会话态（不写入世界存档）：toggle 关闭再开时恢复分类、列表滚动与搜索词（D23、D24）。
    /// </summary>
    public static class RecipaediaCraftingOverlaySessionState {
        public static string SelectedCategoryId { get; set; } = string.Empty;

        public static float BlocksListScrollPosition { get; set; }

        public static string SearchQuery { get; set; } = string.Empty;
    }
}
