using RecipaediaEX;
using Game;
using RecipaediaEX.Implementation;

namespace RecipaediaEX.Overlay {
    /// <summary>工作台 / 机床 / 车间有形合成格自动摆放。</summary>
    public sealed class CraftingTablePlacementTarget : IRecipePlacementTarget {
        readonly ComponentCraftingTable m_table;

        public CraftingTablePlacementTarget(ComponentCraftingTable table) => m_table = table;

        public bool CanAccept(IRecipe recipe) => recipe is OriginalCraftingRecipe;

        public PlacementResult TryPlaceRecipe(
            IRecipe recipe,
            PlacementSources sources,
            PlacementOptions options,
            bool execute
        ) {
            if (recipe is not OriginalCraftingRecipe crafting) {
                return PlacementResult.None(["该配方不支持有形合成格摆放"]);
            }
            return FormattedGridPlacementPlanner.TryPlace(
                FormattedGridPlacementContext.ForCraftingTable(m_table),
                crafting,
                sources,
                options,
                execute);
        }
    }
}
