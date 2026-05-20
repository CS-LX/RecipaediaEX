using Game;
using GameEntitySystem;
using RecipaediaEX.Implementation;

namespace RecipaediaEX.Events {
    /// <summary>
    /// 扩展工作台在重新匹配后，当前预览配方发生变化时发布。
    /// </summary>
    public readonly struct CraftingRecipeChangedEvent {
        public CraftingRecipeChangedEvent(
            Project project,
            IInventory inventory,
            ComponentPlayer? interactingPlayer,
            OriginalCraftingRecipe? previousRecipe,
            OriginalCraftingRecipe? newRecipe) {
            Project = project;
            Inventory = inventory;
            InteractingPlayer = interactingPlayer;
            PreviousRecipe = previousRecipe;
            NewRecipe = newRecipe;
        }

        public Project Project { get; }
        public IInventory Inventory { get; }
        public ComponentPlayer? InteractingPlayer { get; }
        public OriginalCraftingRecipe? PreviousRecipe { get; }
        public OriginalCraftingRecipe? NewRecipe { get; }
    }
}
