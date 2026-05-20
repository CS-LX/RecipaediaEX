using Game;
using GameEntitySystem;

namespace RecipaediaEX.Events {
    /// <summary>
    /// 扩展熔炉成功消耗一格燃料并开始燃烧时发布。
    /// </summary>
    public readonly struct FurnaceFuelUsedEvent {
        public FurnaceFuelUsedEvent(
            Project project,
            IInventory inventory,
            int fuelBlockValue,
            float heatLevel,
            float fireDuration) {
            Project = project;
            Inventory = inventory;
            FuelBlockValue = fuelBlockValue;
            HeatLevel = heatLevel;
            FireDuration = fireDuration;
        }

        public Project Project { get; }
        public IInventory Inventory { get; }
        public int FuelBlockValue { get; }
        public float HeatLevel { get; }
        public float FireDuration { get; }
    }
}
