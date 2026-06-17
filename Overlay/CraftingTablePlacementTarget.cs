using RecipaediaEX;
using Game;
using RecipaediaEX.Implementation;

namespace RecipaediaEX.Overlay {
    /// <summary>工作台 / 机床 / 车间有形合成格自动摆放。</summary>
    public sealed class CraftingTablePlacementTarget : IRecipePlacementTarget {
        readonly ComponentCraftingTable m_table;

        public CraftingTablePlacementTarget(ComponentCraftingTable table) => m_table = table;

        public bool CanAccept(IRecipe recipe) => PlacableRecipeAdapter.TryAsPlacable(recipe, out _);

        public PlacementResult TryPlaceRecipe(
            IRecipe recipe,
            PlacementSources sources,
            PlacementOptions options,
            bool execute
        ) {
            if (!PlacableRecipeAdapter.TryAsPlacable(recipe, out IPlacableRecipe placable)) {
                return PlacementResult.None(["该配方不支持自动摆放"]);
            }
            if (placable is not OriginalCraftingPlacableRecipe crafting) {
                return PlacementResult.None(["该配方不支持有形合成格摆放"]);
            }
            return CraftingGridPlacementPlanner.TryPlace(m_table, crafting.Recipe, sources, options, execute);
        }
    }
}
