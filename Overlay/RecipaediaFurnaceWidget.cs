using Game;
using GameEntitySystem;

namespace RecipaediaEX.Overlay {
    /// <summary>原版熔炉 Modal，接入合成助手 Host。</summary>
    public class RecipaediaFurnaceWidget : FurnaceWidget, IRecipaediaOverlayHost {
        public RecipaediaFurnaceWidget(IInventory inventory, ComponentFurnace componentFurnace)
            : base(inventory, componentFurnace) {
            RecipaediaOverlayHostUi.EnsureToggleButton(this, this);
        }

        public ContainerWidget OverlayParent => this;

        public RecipaediaCraftingContext? GetCraftingContext() {
            if (!m_componentFurnace.IsAddedToProject) return null;
            ComponentPlayer? player = m_componentFurnace.FindInteractingPlayer();
            return new RecipaediaCraftingContext {
                CrafterBlockValue = m_componentFurnace.Entity.FindComponent<ComponentBlockEntity>(false)?.BlockValue ?? 0,
                PlayerLevel = player?.PlayerData.Level ?? 1f,
                Project = m_componentFurnace.Project,
                Inventory = player?.ComponentMiner.Inventory,
            };
        }

        public IRecipePlacementTarget? GetPlacementTarget() =>
            m_componentFurnace.IsAddedToProject
                ? new FurnacePlacementTarget(m_componentFurnace)
                : null;
    }
}
