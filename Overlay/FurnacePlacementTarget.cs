using Game;
using RecipaediaEX.Implementation;

namespace RecipaediaEX.Overlay {
    /// <summary>原版 / 扩展熔炉输入区自动摆放。</summary>
    public sealed class FurnacePlacementTarget : IRecipePlacementTarget {
        readonly ComponentFurnace m_furnace;

        public FurnacePlacementTarget(ComponentFurnace furnace) => m_furnace = furnace;

        public bool CanAccept(IRecipe recipe) => recipe is OriginalSmeltingRecipe;

        public PlacementResult TryPlaceRecipe(
            IRecipe recipe,
            PlacementSources sources,
            PlacementOptions options,
            bool execute
        ) {
            if (recipe is not OriginalSmeltingRecipe smelting) {
                return PlacementResult.None(["该配方不支持熔炼输入区摆放"]);
            }
            if (m_furnace.m_furnaceSize <= 0) {
                return PlacementResult.None(["当前熔炉无可用输入格"]);
            }
            return FormattedGridPlacementPlanner.TryPlace(
                FormattedGridPlacementContext.ForFurnace(m_furnace),
                smelting,
                sources,
                options,
                execute);
        }
    }
}
