namespace RecipaediaEX.Overlay {
    public readonly struct PlacementOptions {
        public bool FillEmptyOnly { get; init; }
        public bool MaxSets { get; init; }
        public bool AllowPartial { get; init; }

        public static PlacementOptions Default => new() {
            FillEmptyOnly = true,
            MaxSets = false,
            AllowPartial = true,
        };
    }
}
