using Game;

namespace RecipaediaEX.Overlay {
    public interface IRecipePlacementTarget {
        /// <summary>当前容器是否接受该配方的自动摆放。</summary>
        bool CanAccept(IRecipe recipe);

        /// <summary>预检（execute=false）或执行（execute=true）放置。</summary>
        PlacementResult TryPlaceRecipe(
            IRecipe recipe,
            PlacementSources sources,
            PlacementOptions options,
            bool execute);
    }
}
