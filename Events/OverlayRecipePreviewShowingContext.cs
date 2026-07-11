using RecipaediaEX.Overlay;
using RecipaediaEX.UI;

namespace RecipaediaEX.Events {
    /// <summary>合成助手即将展示某图鉴条目的配方预览弹层之前。</summary>
    public readonly struct OverlayRecipePreviewShowingContext {
        public OverlayRecipePreviewShowingContext(
            IRecipaediaOverlayHost host,
            RecipaediaCraftingContext craftingContext,
            IRecipaediaRecipeItem recipeItem) {
            Host = host;
            CraftingContext = craftingContext;
            RecipeItem = recipeItem;
        }

        public IRecipaediaOverlayHost Host { get; }
        public RecipaediaCraftingContext CraftingContext { get; }
        public IRecipaediaRecipeItem RecipeItem { get; }
    }
}
