using Game;
using GameEntitySystem;

namespace RecipaediaEX.Events {
    /// <summary>
    /// 熔炉冶炼完成并将产物写入<strong>产物格</strong>时发布（与玩家从产物格取出时的 <see cref="CrafterOutputRemovedEvent"/> 区分）。
    /// </summary>
    public readonly struct CrafterOutputProducedEvent {
        public CrafterOutputProducedEvent(
            Project project,
            IInventory inventory,
            ComponentPlayer? interactingPlayer,
            IRecipe? recipe,
            int outputBlockValue,
            int producedCount,
            CrafterInventorySurfaceKind kind) {
            Project = project;
            Inventory = inventory;
            InteractingPlayer = interactingPlayer;
            Recipe = recipe;
            OutputBlockValue = outputBlockValue;
            ProducedCount = producedCount;
            Kind = kind;
        }

        public Project Project { get; }
        public IInventory Inventory { get; }
        public ComponentPlayer? InteractingPlayer { get; }
        public IRecipe? Recipe { get; }
        public int OutputBlockValue { get; }
        public int ProducedCount { get; }
        public CrafterInventorySurfaceKind Kind { get; }
    }
}
