using Game;
using GameEntitySystem;

namespace RecipaediaEX.Events {
    /// <summary>
    /// 熔炉冶炼完成、<strong>写入产物格之前</strong>询问是否允许（与事后 <see cref="CrafterOutputProducedEvent"/> 成对）。
    /// </summary>
    public readonly struct CrafterOutputProducingContext {
        public CrafterOutputProducingContext(
            Project project,
            IInventory inventory,
            ComponentPlayer? interactingPlayer,
            IRecipe recipe,
            int outputBlockValue,
            int producedCount,
            string crafterKind) {
            Project = project;
            Inventory = inventory;
            InteractingPlayer = interactingPlayer;
            Recipe = recipe;
            OutputBlockValue = outputBlockValue;
            ProducedCount = producedCount;
            CrafterKind = crafterKind ?? string.Empty;
        }

        public Project Project { get; }
        public IInventory Inventory { get; }
        public ComponentPlayer? InteractingPlayer { get; }
        public IRecipe Recipe { get; }
        public int OutputBlockValue { get; }
        public int ProducedCount { get; }
        public string CrafterKind { get; }
    }
}
