using Game;
using GameEntitySystem;

namespace RecipaediaEX.Overlay {
    public static class RecipaediaCraftingTableContextHelper {
        public static RecipaediaCraftingContext BuildContext(ComponentCraftingTable table, int gridWidth) {
            ComponentPlayer? player = table.FindInteractingPlayer();
            return new RecipaediaCraftingContext {
                CrafterBlockValue = table.Entity.FindComponent<ComponentBlockEntity>(false)?.BlockValue ?? 0,
                GridWidth = gridWidth,
                PlayerLevel = player?.PlayerData.Level ?? 1f,
                Project = table.Project,
                Inventory = player?.ComponentMiner.Inventory,
            };
        }
    }
}
