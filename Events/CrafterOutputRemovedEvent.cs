using Game;
using GameEntitySystem;

namespace RecipaediaEX.Events {
    /// <summary>RecipaediaEX 扩展台/炉「产物格」表面类型。</summary>
    public enum CrafterInventorySurfaceKind {
        CraftingTable,
        Furnace
    }

    /// <summary>
    /// 从 RecipaediaEX 工作台或熔炉的<strong>产物格</strong>成功取出物品时发布（与冶炼完成写入产物格区分）。
    /// </summary>
    public readonly struct CrafterOutputRemovedEvent {
        public CrafterOutputRemovedEvent(Project project, ComponentPlayer? interactingPlayer, int outputBlockValue, int removedCount, CrafterInventorySurfaceKind kind) {
            Project = project;
            InteractingPlayer = interactingPlayer;
            OutputBlockValue = outputBlockValue;
            RemovedCount = removedCount;
            Kind = kind;
        }

        public Project Project { get; }
        public ComponentPlayer? InteractingPlayer { get; }
        /// <summary>被取走的堆叠的方块值（含 data）。</summary>
        public int OutputBlockValue { get; }
        public int RemovedCount { get; }
        public CrafterInventorySurfaceKind Kind { get; }
    }
}
