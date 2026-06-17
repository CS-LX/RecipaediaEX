using Game;

namespace RecipaediaEX.Overlay {
    public readonly struct PlacementSources {
        public IInventory PlayerInventory { get; init; }
        public IInventory? ContainerInventory { get; init; }
    }
}
