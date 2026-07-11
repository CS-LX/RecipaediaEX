using Game;

namespace RecipaediaEX.Overlay {
    /// <summary>Descriptor 卡片操作条（+ / ★）与 Dialog 的桥接。</summary>
    public interface IRecipaediaOverlayDescriptorHost {
        /// <summary>轻量门控（Tab / Host / 可摆类型），不含 dry-run。</summary>
        bool PassesPlacementGate(IRecipe recipe, out string disabledReason);

        /// <summary>点击 / 长按 + 时执行；内含 dry-run 与真实转移。返回 false 时连放应停止。</summary>
        /// <param name="clearGridBeforePlace">连放后续次应传 false，避免清空已摆 pattern。</param>
        /// <param name="showFeedback">连放中建议 false，避免刷屏与收起详情。</param>
        bool PlaceRecipe(IRecipe recipe, bool clearGridBeforePlace = true, bool showFeedback = true);

        bool IsRecipeBookmarked(IRecipe recipe);

        bool ToggleRecipeBookmark(IRecipe recipe);
    }
}
