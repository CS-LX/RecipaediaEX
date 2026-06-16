using Game;

namespace RecipaediaEX.Overlay {
    public interface IRecipaediaOverlayHost {
        /// <summary>当前合成上下文；无法提供时返回 null（不显示助手入口）。</summary>
        RecipaediaCraftingContext? GetCraftingContext();

        /// <summary>用于 Dialog 挂载的 GUI 根（通常为合成 Modal Widget）。</summary>
        ContainerWidget OverlayParent { get; }

        /// <summary>自动摆放目标；Phase 2a 起由 Host 提供，P0 可返回 null。</summary>
        IRecipePlacementTarget? GetPlacementTarget() => null;
    }
}
