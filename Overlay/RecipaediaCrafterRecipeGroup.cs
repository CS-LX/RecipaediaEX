using System.Collections.Generic;
using Game;

namespace RecipaediaEX.Overlay {
    public sealed class RecipaediaCrafterRecipeGroup {
        /// <summary>方块 <see cref="Block.GetCraftingId"/>；工业设备各用独立 id，避免共用 BlockIndex 合并 Tab。</summary>
        public string CrafterId { get; init; } = string.Empty;

        public int RepresentativeBlockValue { get; init; }

        public List<IRecipe> Recipes { get; init; } = [];

        public string DisplayName =>
            BlocksManager.Blocks[Terrain.ExtractContents(RepresentativeBlockValue)]
                .GetDisplayName(null, RepresentativeBlockValue);
    }
}
