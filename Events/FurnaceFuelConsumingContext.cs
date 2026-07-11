using Game;
using GameEntitySystem;

namespace RecipaediaEX.Events {
    /// <summary>
    /// 扩展熔炉消耗一格燃料之前询问是否允许（与事后 <see cref="FurnaceFuelUsedEvent"/> 成对）。
    /// </summary>
    public readonly struct FurnaceFuelConsumingContext {
        public FurnaceFuelConsumingContext(
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
