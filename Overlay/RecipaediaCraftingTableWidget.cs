using Game;

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
            return RecipaediaCraftingTableContextHelper.BuildContext(m_componentCraftingTable, m_craftingGrid.ColumnsCount);
        }
    }
}
