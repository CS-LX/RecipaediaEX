using Game;
using GameEntitySystem;

namespace RecipaediaEX.Events {
    /// <summary>
    /// 内置 Crafter 标识（约定字符串）。其它模组使用机器/方块名称或 <c>{ModId}.{Name}</c> 等形式。
    /// </summary>
    public static class CrafterKind {
        public const string CraftingTable = "RecipaediaEX.CraftingTable";
        public const string Furnace = "RecipaediaEX.Furnace";
    }

    /// <summary>
    /// 从 Crafter 的<strong>产物格</strong>成功取出物品时发布（与冶炼完成写入产物格区分）。
    /// </summary>
    public readonly struct CrafterOutputRemovedEvent {
        public CrafterOutputRemovedEvent(Project project, ComponentPlayer? interactingPlayer, int outputBlockValue, int removedCount, string crafterKind) {
            Project = project;
            InteractingPlayer = interactingPlayer;
            OutputBlockValue = outputBlockValue;
            RemovedCount = removedCount;
            CrafterKind = crafterKind ?? string.Empty;
        }

        public Project Project { get; }
        public ComponentPlayer? InteractingPlayer { get; }
        /// <summary>被取走的堆叠的方块值（含 data）。</summary>
        public int OutputBlockValue { get; }
        public int RemovedCount { get; }
        /// <summary>发布方 Crafter 名称或约定标识。</summary>
        public string CrafterKind { get; }
    }
}
