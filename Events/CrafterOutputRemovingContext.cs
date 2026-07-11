using Game;
using GameEntitySystem;

namespace RecipaediaEX.Events {
    /// <summary>
    /// 扩展 Crafter 从<strong>产物格</strong>取出物品之前询问是否允许（与事后 <see cref="CrafterOutputRemovedEvent"/> 成对）。
    /// </summary>
    public readonly struct CrafterOutputRemovingContext {
        public CrafterOutputRemovingContext(
            Project project,
            IInventory inventory,
            ComponentPlayer? interactingPlayer,
            IRecipe? recipe,
            int outputBlockValue,
            int requestedCount,
            string crafterKind) {
            Project = project;
            Inventory = inventory;
            InteractingPlayer = interactingPlayer;
            Recipe = recipe;
            OutputBlockValue = outputBlockValue;
            RequestedCount = requestedCount;
            CrafterKind = crafterKind ?? string.Empty;
        }

        public Project Project { get; }
        public IInventory Inventory { get; }
        public ComponentPlayer? InteractingPlayer { get; }
        public IRecipe? Recipe { get; }
        public int OutputBlockValue { get; }
        public int RequestedCount { get; }
        public string CrafterKind { get; }
    }
}
