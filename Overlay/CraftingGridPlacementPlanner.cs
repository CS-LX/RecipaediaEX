using System;
using System.Collections.Generic;
using System.Globalization;
using Game;
using RecipaediaEX.Implementation;

namespace RecipaediaEX.Overlay {
    /// <summary>有形合成格自动摆放：Transform 布局选择、背包取料、部分填充。</summary>
    static class CraftingGridPlacementPlanner {
        sealed class PlacementAction {
            public int TargetSlot;
            public int SourceSlot = -1;
            public int BlockValue;
            public bool NeedsTransfer;
            public string? MissingLabel;
        }

        public static PlacementResult TryPlace(
            ComponentCraftingTable table,
            FormattedRecipe recipe,
            PlacementSources sources,
            PlacementOptions options,
            bool execute
        ) {
            if (sources.PlayerInventory == null) {
                return PlacementResult.None(["缺少玩家背包"]);
            }

            EnsureTransformedLayouts(recipe);

            int gridSize = table.m_craftingGridSize;
            List<PlacementAction>? bestPlan = null;
            int bestScore = int.MinValue;

            foreach (string[] layout in recipe.TransformedIngredients) {
                if (!LayoutFitsGrid(layout, gridSize)) continue;
                if (!TryBuildPlan(table, layout, gridSize, recipe, sources, options, out List<PlacementAction> plan)) continue;
                int score = ScorePlan(plan);
                if (score <= bestScore) continue;
                bestScore = score;
                bestPlan = plan;
            }

            if (bestPlan == null) {
                return PlacementResult.None(["配方无法放入当前合成格"]);
            }

            if (!bestPlan.Exists(static action => action.NeedsTransfer)) {
                return PlacementResult.AlreadySatisfied();
            }

            if (execute) {
                foreach (PlacementAction action in bestPlan) {
                    if (!action.NeedsTransfer) continue;
                    sources.PlayerInventory.RemoveSlotItems(action.SourceSlot, 1);
                    table.AddSlotItems(action.TargetSlot, action.BlockValue, 1);
                }
                table.UpdateCraftingResult(true);
            }

            return ToResult(bestPlan);
        }

        static void EnsureTransformedLayouts(FormattedRecipe recipe) {
            if (recipe.TransformedIngredients.Count > 0) return;
            recipe.PreTransformIngredients();
            if (recipe.TransformedIngredients.Count > 0) return;
            var identity = new string[recipe.Ingredients.Length];
            Array.Copy(recipe.Ingredients, identity, recipe.Ingredients.Length);
            recipe.TransformedIngredients.Add(identity);
        }

        static bool LayoutFitsGrid(string[] layout, int gridSize) {
            for (int gridIndex = 0; gridIndex < layout.Length; gridIndex++) {
                if (string.IsNullOrEmpty(layout[gridIndex])) continue;
                if (!TryGridIndexToSlot(gridIndex, gridSize, out _)) return false;
            }
            return true;
        }

        static int ScorePlan(List<PlacementAction> plan) {
            int transfers = 0;
            int satisfied = 0;
            int missing = 0;
            foreach (PlacementAction action in plan) {
                if (action.MissingLabel != null) {
                    missing++;
                    continue;
                }
                if (action.NeedsTransfer) transfers++;
                else satisfied++;
            }
            return satisfied * 100 + transfers * 10 - missing * 1000;
        }

        static PlacementResult ToResult(List<PlacementAction> plan) {
            if (plan.Count == 0) return PlacementResult.AlreadySatisfied();
            var missing = new List<string>();
            bool anyTransfer = false;
            bool anyMissing = false;
            foreach (PlacementAction action in plan) {
                if (action.MissingLabel != null) {
                    anyMissing = true;
                    if (!missing.Contains(action.MissingLabel)) missing.Add(action.MissingLabel);
                }
                else if (action.NeedsTransfer) anyTransfer = true;
            }

            if (!anyMissing) return PlacementResult.Complete(anyTransfer);
            if (anyTransfer) return PlacementResult.Partial(missing);
            return PlacementResult.None(missing);
        }

        static bool TryBuildPlan(
            ComponentCraftingTable table,
            string[] layout,
            int gridSize,
            FormattedRecipe recipe,
            PlacementSources sources,
            PlacementOptions options,
            out List<PlacementAction> plan
        ) {
            plan = [];
            var reservedCounts = new Dictionary<int, int>();

            for (int gridIndex = 0; gridIndex < layout.Length; gridIndex++) {
                string ingredient = layout[gridIndex];
                if (string.IsNullOrEmpty(ingredient)) continue;

                if (!TryGridIndexToSlot(gridIndex, gridSize, out int targetSlot)) return false;

                int slotCount = table.GetSlotCount(targetSlot);
                int slotValue = table.GetSlotValue(targetSlot);
                if (slotCount > 0) {
                    if (IngredientMatches(ingredient, slotValue, recipe)) {
                        plan.Add(new PlacementAction { TargetSlot = targetSlot, NeedsTransfer = false });
                        continue;
                    }
                    if (options.FillEmptyOnly) {
                        plan.Add(new PlacementAction { MissingLabel = FormatMissing(ingredient, recipe) });
                        if (!options.AllowPartial) return false;
                        continue;
                    }
                    plan.Add(new PlacementAction { MissingLabel = FormatMissing(ingredient, recipe) });
                    if (!options.AllowPartial) return false;
                    continue;
                }

                if (!TryFindPlayerSlot(sources.PlayerInventory, ingredient, recipe, reservedCounts, out int sourceSlot, out int blockValue)) {
                    plan.Add(new PlacementAction { MissingLabel = FormatMissing(ingredient, recipe) });
                    if (!options.AllowPartial) return false;
                    continue;
                }

                reservedCounts[sourceSlot] = reservedCounts.GetValueOrDefault(sourceSlot) + 1;
                plan.Add(new PlacementAction {
                    TargetSlot = targetSlot,
                    SourceSlot = sourceSlot,
                    BlockValue = blockValue,
                    NeedsTransfer = true,
                });
            }

            return true;
        }

        static bool TryGridIndexToSlot(int gridIndex, int gridSize, out int slotIndex) {
            int row = gridIndex / 6;
            int col = gridIndex % 6;
            if (row >= gridSize || col >= gridSize) {
                slotIndex = -1;
                return false;
            }
            slotIndex = col + row * gridSize;
            return true;
        }

        static bool IngredientMatches(string requiredIngredient, int blockValue, FormattedRecipe recipe) {
            if (blockValue == 0) return false;
            string actual = ToCraftingIngredient(blockValue);
            try {
                if (CraftingRecipesManager.CompareIngredients(requiredIngredient, actual)) return true;
            }
            catch (InvalidOperationException) { }
            return recipe.CompareIngredient(requiredIngredient, actual, throwOnNotSpecified: false);
        }

        static bool TryFindPlayerSlot(
            IInventory inventory,
            string requiredIngredient,
            FormattedRecipe recipe,
            Dictionary<int, int> reservedCounts,
            out int sourceSlot,
            out int blockValue
        ) {
            sourceSlot = -1;
            blockValue = 0;
            int bestAvailable = -1;
            int bestSlot = int.MaxValue;

            for (int i = 0; i < inventory.SlotsCount; i++) {
                int total = inventory.GetSlotCount(i);
                if (total <= 0) continue;
                int value = inventory.GetSlotValue(i);
                if (inventory.GetSlotCapacity(i, value) == 0) continue;
                reservedCounts.TryGetValue(i, out int reserved);
                int available = total - reserved;
                if (available <= 0) continue;
                if (!IngredientMatches(requiredIngredient, value, recipe)) continue;
                if (available > bestAvailable || (available == bestAvailable && i < bestSlot)) {
                    bestAvailable = available;
                    bestSlot = i;
                    sourceSlot = i;
                    blockValue = value;
                }
            }

            return sourceSlot >= 0;
        }

        static string ToCraftingIngredient(int blockValue) {
            int content = Terrain.ExtractContents(blockValue);
            int data = Terrain.ExtractData(blockValue);
            Block block = BlocksManager.Blocks[content];
            return block.GetCraftingId(blockValue) + ":" + data.ToString(CultureInfo.InvariantCulture);
        }

        static string FormatMissing(string ingredient, FormattedRecipe recipe) {
            int[] values = FormattedRecipe.ExpandIngredientToBlockValues(ingredient);
            if (values.Length > 0) {
                int value = values[0];
                Block block = BlocksManager.Blocks[Terrain.ExtractContents(value)];
                return $"缺少 {block.GetDisplayName(null, value)}";
            }
            CraftingRecipesManager.DecodeIngredient(ingredient, out string craftingId, out int? _);
            return $"缺少 {craftingId}";
        }
    }
}
