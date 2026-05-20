using Game;
using GameEntitySystem;
using RecipaediaEX.Implementation;

namespace RecipaediaEX.Events {
    /// <summary>
    /// 扩展熔炉当前激活的冶炼配方发生变化时发布（含变为无配方）。
    /// </summary>
    public readonly struct SmeltingRecipeChangedEvent {
        public SmeltingRecipeChangedEvent(
            Project project,
            IInventory inventory,
            ComponentPlayer? interactingPlayer,
            OriginalSmeltingRecipe? previousRecipe,
            OriginalSmeltingRecipe? newRecipe) {
            Project = project;
            Inventory = inventory;
            InteractingPlayer = interactingPlayer;
            PreviousRecipe = previousRecipe;
            NewRecipe = newRecipe;
        }

        public Project Project { get; }
        public IInventory Inventory { get; }
        public ComponentPlayer? InteractingPlayer { get; }
        public OriginalSmeltingRecipe? PreviousRecipe { get; }
        public OriginalSmeltingRecipe? NewRecipe { get; }
    }
}
