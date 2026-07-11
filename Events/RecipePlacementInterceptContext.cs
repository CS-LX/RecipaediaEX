using Game;
using RecipaediaEX.Implementation;
using RecipaediaEX.Overlay;

namespace RecipaediaEX.Events {
    /// <summary>合成助手 <c>+</c> 已算出搬运方案、尚未执行背包→合成格转移时。</summary>
    public readonly struct RecipePlacementPlanBuildingContext {
        public RecipePlacementPlanBuildingContext(
            FormattedGridPlacementContext placementContext,
            FormattedRecipe recipe,
            PlacementSources sources,
            PlacementOptions options,
            string crafterKind,
            bool willExecute,
            int plannedTransferCount,
            int missingIngredientCount) {
            PlacementContext = placementContext;
            Recipe = recipe;
            Sources = sources;
            Options = options;
            CrafterKind = crafterKind ?? string.Empty;
            WillExecute = willExecute;
            PlannedTransferCount = plannedTransferCount;
            MissingIngredientCount = missingIngredientCount;
        }

        public FormattedGridPlacementContext PlacementContext { get; }
        public FormattedRecipe Recipe { get; }
        public PlacementSources Sources { get; }
        public PlacementOptions Options { get; }
        public string CrafterKind { get; }
        /// <summary>当前 <c>TryPlace</c> 调用是否处于执行阶段（<c>execute: true</c>）。</summary>
        public bool WillExecute { get; }
        public int PlannedTransferCount { get; }
        public int MissingIngredientCount { get; }
    }

    /// <summary>合成助手 <c>+</c> 即将从背包扣除物品并填入合成格/熔炉输入格时。</summary>
    public readonly struct RecipePlacementExecutingContext {
        public RecipePlacementExecutingContext(
            FormattedGridPlacementContext placementContext,
            FormattedRecipe recipe,
            PlacementSources sources,
            PlacementOptions options,
            string crafterKind,
            int plannedTransferCount) {
            PlacementContext = placementContext;
            Recipe = recipe;
            Sources = sources;
            Options = options;
            CrafterKind = crafterKind ?? string.Empty;
            PlannedTransferCount = plannedTransferCount;
        }

        public FormattedGridPlacementContext PlacementContext { get; }
        public FormattedRecipe Recipe { get; }
        public PlacementSources Sources { get; }
        public PlacementOptions Options { get; }
        public string CrafterKind { get; }
        public int PlannedTransferCount { get; }
    }
}
