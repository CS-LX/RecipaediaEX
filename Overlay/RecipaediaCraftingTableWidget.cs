using Game;
using GameEntitySystem;

namespace RecipaediaEX.Overlay {
    /// <summary>原版 3×3 工作台 Modal，接入合成助手 Host。</summary>
    public class RecipaediaCraftingTableWidget : CraftingTableWidget, IRecipaediaOverlayHost {
        public RecipaediaCraftingTableWidget(IInventory inventory, ComponentCraftingTable componentCraftingTable)
            : base(inventory, componentCraftingTable) {
            RecipaediaOverlayHostUi.EnsureToggleButton(this, this);
        }

        public ContainerWidget OverlayParent => this;

        public RecipaediaCraftingContext? GetCraftingContext() {
            if (!m_componentCraftingTable.IsAddedToProject) return null;
            ComponentPlayer? player = m_componentCraftingTable.FindInteractingPlayer();
            return new RecipaediaCraftingContext {
                CrafterBlockValue = m_componentCraftingTable.Entity.FindComponent<ComponentBlockEntity>(false)?.BlockValue ?? 0,
                PlayerLevel = player?.PlayerData.Level ?? 1f,
                Project = m_componentCraftingTable.Project,
                Inventory = player?.ComponentMiner.Inventory,
            };
        }
    }
}
