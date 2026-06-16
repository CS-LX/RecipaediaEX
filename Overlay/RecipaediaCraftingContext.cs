using Game;
using GameEntitySystem;

namespace RecipaediaEX.Overlay {
    public sealed class RecipaediaCraftingContext {
        /// <summary>当前 Host 方块值：默认 Crafter Tab、Phase 2a「+」是否可用。</summary>
        public int CrafterBlockValue { get; init; }

        public float PlayerLevel { get; init; } = 1f;

        public float RequiredHeatLevel { get; init; }

        public Project? Project { get; init; }

        public IInventory? Inventory { get; init; }
    }
}
