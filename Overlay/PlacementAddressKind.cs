namespace RecipaediaEX.Overlay {
    /// <summary>寻址方式：仅两类 REX 原生语义；不含 Fluid / Tank / 化工。</summary>
    public enum PlacementAddressKind {
        /// <summary>有形合成格，AddressIndex 为 0–35（6×6 语义）。</summary>
        GridCell,

        /// <summary>当前容器内逻辑槽位；AddressIndex 含义由 <see cref="IRecipePlacementTarget"/> 定义。</summary>
        ContainerSlot,
    }
}
