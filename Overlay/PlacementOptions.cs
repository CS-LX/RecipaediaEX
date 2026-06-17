namespace RecipaediaEX.Overlay {
    public readonly struct PlacementOptions {
        public bool FillEmptyOnly { get; init; }
        public bool MaxSets { get; init; }
        public bool AllowPartial { get; init; }

        /// <summary>执行摆放前将合成格物品退回背包，便于切换同产物不同变体配方。</summary>
        public bool ClearGridBeforePlace { get; init; }

        public static PlacementOptions Default => new() {
            FillEmptyOnly = true,
            MaxSets = false,
            AllowPartial = true,
            ClearGridBeforePlace = true,
        };
    }
}
