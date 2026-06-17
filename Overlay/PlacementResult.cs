using System.Collections.Generic;

namespace RecipaediaEX.Overlay {
    public readonly struct PlacementResult {
        public bool Success { get; init; }
        public bool PartialSuccess { get; init; }
        public bool HadTransfers { get; init; }
        public IReadOnlyList<string> Missing { get; init; }

        public static PlacementResult Complete(bool hadTransfers = true) => new() {
            Success = true,
            PartialSuccess = false,
            HadTransfers = hadTransfers,
            Missing = [],
        };

        public static PlacementResult AlreadySatisfied() => Complete(hadTransfers: false);

        public static PlacementResult None(IReadOnlyList<string> missing) => new() {
            Success = false,
            PartialSuccess = false,
            HadTransfers = false,
            Missing = missing,
        };

        public static PlacementResult Partial(IReadOnlyList<string> missing, bool hadTransfers = true) => new() {
            Success = false,
            PartialSuccess = true,
            HadTransfers = hadTransfers,
            Missing = missing,
        };
    }
}
