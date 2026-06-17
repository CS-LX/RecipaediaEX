namespace RecipaediaEX.Overlay {
    /// <summary>单条摆放需求。Quantity / MatchKey 的物理含义由 Target + 配方适配器解释。</summary>
    public readonly struct PlacementRequirement {
        public PlacementAddressKind AddressKind { get; init; }
        public int AddressIndex { get; init; }
        public string MatchKey { get; init; }
        public float Quantity { get; init; }
        public int[] AcceptedBlockValues { get; init; }
    }
}
