namespace RecipaediaEX.Overlay {
    /// <summary>
    /// 合成助手会话态（不写入世界存档）：toggle 关闭再开时恢复分类与列表滚动。
    /// </summary>
    public static class RecipaediaCraftingOverlaySessionState {
        public static string SelectedCategoryId { get; set; } = string.Empty;

        public static float BlocksListScrollPosition { get; set; }
    }
}
