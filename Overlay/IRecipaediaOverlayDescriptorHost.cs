using Game;

namespace RecipaediaEX.Overlay {
    /// <summary>Descriptor 卡片操作条（+ / ★）与 Dialog 的桥接。</summary>
    public interface IRecipaediaOverlayDescriptorHost {
        /// <summary>轻量门控（Tab / Host / 可摆类型），不含 dry-run。</summary>
        bool PassesPlacementGate(IRecipe recipe, out string disabledReason);

        /// <summary>点击 + 时执行；内含 dry-run 与真实转移。</summary>
        void PlaceRecipe(IRecipe recipe);

        bool IsRecipeBookmarked(IRecipe recipe);

        bool ToggleRecipeBookmark(IRecipe recipe);
    }
}
