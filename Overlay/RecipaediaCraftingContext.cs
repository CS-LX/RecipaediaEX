using Game;
using GameEntitySystem;

namespace RecipaediaEX.Overlay {
    public sealed class RecipaediaCraftingContext {
        /// <summary>预览配方列表过滤（等价 @crafter:，不拼进搜索 query）。</summary>
        public int CrafterBlockValue { get; init; }

        /// <summary>有形合成预览宽度过滤：3 / 4 / 5；非网格机器可为 0。</summary>
        public int GridWidth { get; init; }

        public float PlayerLevel { get; init; } = 1f;

        public float RequiredHeatLevel { get; init; }

        public Project? Project { get; init; }

        public IInventory? Inventory { get; init; }
    }
}
