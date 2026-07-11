using System;
using System.Collections.Generic;
using Game;
using RecipaediaEX.Events;
using RecipaediaEX.Implementation;

namespace RecipaediaEX.Overlay {
    public enum FormattedGridMappingMode {
        /// <summary>N×N 有形合成格（工作台 / 机床）。</summary>
        SquareCrafting,
        /// <summary>熔炼输入区第一行；<c>gridIndex</c> 平移对齐 <see cref="FormattedRecipe.Match"/>。</summary>
        FurnaceInputRow,
    }

    public readonly struct FormattedGridPlacementContext {
        public IInventory Inventory { get; init; }
        public FormattedGridMappingMode MappingMode { get; init; }
        /// <summary>工作台为 <c>m_craftingGridSize</c>；熔炉为 <c>m_furnaceSize</c>。</summary>
        public int GridSpan { get; init; }
        public int ClearSlotCount { get; init; }
        public Action RefreshRecipe { get; init; }
        public string ClearGridErrorMessage { get; init; }

        public static FormattedGridPlacementContext ForCraftingTable(ComponentCraftingTable table) => new() {
            Inventory = table,
            MappingMode = FormattedGridMappingMode.SquareCrafting,
            GridSpan = table.m_craftingGridSize,
            ClearSlotCount = table.SlotsCount - 2,
            RefreshRecipe = () => table.UpdateCraftingResult(true),
            ClearGridErrorMessage = "背包已满，无法清空合成格",
        };

        public static FormattedGridPlacementContext ForFurnace(ComponentFurnace furnace) => new() {
            Inventory = furnace,
            MappingMode = FormattedGridMappingMode.FurnaceInputRow,
            GridSpan = furnace.m_furnaceSize,
            ClearSlotCount = furnace.m_furnaceSize,
            RefreshRecipe = furnace.UpdateSmeltingRecipe,
            ClearGridErrorMessage = "背包已满，无法清空熔炉输入格",
        };
    }

    /// <summary><see cref="FormattedRecipe"/> 格位自动摆放：Transform 选布局、背包取料、部分填充。</summary>
    static class FormattedGridPlacementPlanner {
        sealed class PlacementAction {
            public int TargetSlot;
            public int SourceSlot = -1;
            public int BlockValue;
            public bool NeedsTransfer;
            public string? MissingLabel;
        }

        public static PlacementResult TryPlace(
            FormattedGridPlacementContext context,
            FormattedRecipe recipe,
            PlacementSources sources,
            PlacementOptions options,
            bool execute
        ) {
            if (sources.PlayerInventory == null) {
                return PlacementResult.None(["缺少玩家背包"]);
            }

            IInventory container = context.Inventory;
            if (execute && options.ClearGridBeforePlace) {
                if (!TryClearInputSlotsToInventory(context, sources.PlayerInventory, out string? clearError)) {
                    return PlacementResult.None([clearError ?? context.ClearGridErrorMessage]);
                }
            }

            bool treatGridAsEmpty = options.ClearGridBeforePlace && !execute;

            EnsureTransformedLayouts(recipe);

            List<PlacementAction>? bestPlan = null;
            int bestScore = int.MinValue;

            foreach (string[] layout in recipe.TransformedIngredients) {
                if (!LayoutFitsGrid(layout, context)) continue;
                if (!TryBuildPlan(container, layout, context, recipe, sources, options, treatGridAsEmpty, out List<PlacementAction> plan)) continue;
                int score = ScorePlan(plan);
                if (score <= bestScore) continue;
                bestScore = score;
                bestPlan = plan;
                if (!plan.Exists(static action => action.NeedsTransfer)
                    && plan.Exists(static action => action.MissingLabel != null)) {
                    break;
                }
            }

            if (bestPlan == null) {
                return PlacementResult.None([context.MappingMode == FormattedGridMappingMode.FurnaceInputRow
                    ? "配方无法放入当前熔炉输入格"
                    : "配方无法放入当前合成格"]);
            }

            if (!bestPlan.Exists(static action => action.NeedsTransfer)) {
                if (bestPlan.Exists(static action => action.MissingLabel != null)) {
                    return ToResult(bestPlan);
                }
                return PlacementResult.AlreadySatisfied();
            }

            CountPlan(bestPlan, out int plannedTransferCount, out int missingIngredientCount);
            string crafterKind = CrafterKindFor(context);
            if (!RecipaediaInterceptBus.TryProceed(new RecipePlacementPlanBuildingContext(
                    context,
                    recipe,
                    sources,
                    options,
                    crafterKind,
                    execute,
                    plannedTransferCount,
                    missingIngredientCount))) {
                return ToResult(bestPlan);
            }

            if (execute) {
                if (!RecipaediaInterceptBus.TryProceed(new RecipePlacementExecutingContext(
                        context,
                        recipe,
                        sources,
                        options,
                        crafterKind,
                        plannedTransferCount))) {
                    return ToResult(bestPlan);
                }
                foreach (PlacementAction action in bestPlan) {
                    if (!action.NeedsTransfer) continue;
                    int removed = sources.PlayerInventory.RemoveSlotItems(action.SourceSlot, 1);
                    if (removed <= 0) continue;
                    container.AddSlotItems(action.TargetSlot, action.BlockValue, removed);
                }
                context.RefreshRecipe();
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

        static bool LayoutFitsGrid(string[] layout, FormattedGridPlacementContext context) {
            for (int gridIndex = 0; gridIndex < layout.Length; gridIndex++) {
                if (string.IsNullOrEmpty(layout[gridIndex])) continue;
                if (!TryGridIndexToSlot(gridIndex, context, out _)) return false;
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
            IInventory container,
            string[] layout,
            FormattedGridPlacementContext context,
            FormattedRecipe recipe,
            PlacementSources sources,
            PlacementOptions options,
            bool treatGridAsEmpty,
            out List<PlacementAction> plan
        ) {
            plan = [];
            var reservedCounts = new Dictionary<int, int>();

            for (int gridIndex = 0; gridIndex < layout.Length; gridIndex++) {
                string ingredient = layout[gridIndex];
                if (string.IsNullOrEmpty(ingredient)) continue;

                if (!TryGridIndexToSlot(gridIndex, context, out int targetSlot)) return false;

                int slotCount = treatGridAsEmpty ? 0 : container.GetSlotCount(targetSlot);
                int slotValue = treatGridAsEmpty ? 0 : container.GetSlotValue(targetSlot);
                if (slotCount > 0) {
                    if (IngredientMatches(ingredient, slotValue, recipe)) {
                        int capacity = container.GetSlotCapacity(targetSlot, slotValue);
                        if (slotCount < capacity
                            && TryFindPlayerSlot(sources.PlayerInventory, ingredient, recipe, reservedCounts, out int stackSourceSlot, out int stackBlockValue)) {
                            reservedCounts[stackSourceSlot] = reservedCounts.GetValueOrDefault(stackSourceSlot) + 1;
                            plan.Add(new PlacementAction {
                                TargetSlot = targetSlot,
                                SourceSlot = stackSourceSlot,
                                BlockValue = stackBlockValue,
                                NeedsTransfer = true,
                            });
                        }
                        else {
                            plan.Add(new PlacementAction { TargetSlot = targetSlot, NeedsTransfer = false });
                        }
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

        static bool TryGridIndexToSlot(int gridIndex, FormattedGridPlacementContext context, out int slotIndex) {
            int row = gridIndex / 6;
            int col = gridIndex % 6;
            switch (context.MappingMode) {
                case FormattedGridMappingMode.FurnaceInputRow:
                    if (row != 0 || col >= context.GridSpan) {
                        slotIndex = -1;
                        return false;
                    }
                    slotIndex = col;
                    return true;
                default:
                    if (row >= context.GridSpan || col >= context.GridSpan) {
                        slotIndex = -1;
                        return false;
                    }
                    slotIndex = col + row * context.GridSpan;
                    return true;
            }
        }

        static bool IngredientMatches(string requiredIngredient, int blockValue, FormattedRecipe recipe) {
            if (blockValue == 0) return false;
            foreach (int accepted in CraftingOverlayIngredientBridge.ExpandBlockValues(requiredIngredient)) {
                if (accepted == blockValue) return true;
            }
            string actual = CraftingOverlayIngredientBridge.ToCraftingId(blockValue);
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

        static string FormatMissing(string ingredient, FormattedRecipe recipe) {
            if (s_missingLabelCache.TryGetValue(ingredient, out string? cached)) return cached;

            string label;
            if (CraftingOverlayIngredientBridge.TryDecodeDisplayBlockValue(ingredient, out int blockValue)) {
                Block block = BlocksManager.Blocks[Terrain.ExtractContents(blockValue)];
                label = $"缺少 {block.GetDisplayName(null, blockValue)}";
            }
            else {
                CraftingRecipesManager.DecodeIngredient(ingredient, out string craftingId, out int? _);
                label = string.IsNullOrEmpty(craftingId) ? "缺少原料" : $"缺少 {craftingId}";
            }

            s_missingLabelCache[ingredient] = label;
            return label;
        }

        static readonly Dictionary<string, string> s_missingLabelCache = new();

        static bool TryClearInputSlotsToInventory(FormattedGridPlacementContext context, IInventory playerInventory, out string? error) {
            error = null;
            IInventory container = context.Inventory;
            for (int slot = 0; slot < context.ClearSlotCount; slot++) {
                int remaining = container.GetSlotCount(slot);
                if (remaining <= 0) continue;
                int value = container.GetSlotValue(slot);
                while (remaining > 0) {
                    int moved = TryAddToInventory(playerInventory, value, remaining);
                    if (moved <= 0) {
                        error = context.ClearGridErrorMessage;
                        return false;
                    }
                    container.RemoveSlotItems(slot, moved);
                    remaining -= moved;
                }
            }
            context.RefreshRecipe();
            return true;
        }

        static int TryAddToInventory(IInventory inventory, int value, int count) {
            int moved = 0;
            Block block = BlocksManager.Blocks[Terrain.ExtractContents(value)];
            int maxStack = block.GetMaxStacking(value);
            for (int i = 0; i < inventory.SlotsCount; i++) {
                if (inventory.GetSlotCapacity(i, value) == 0) continue;
                int slotCount = inventory.GetSlotCount(i);
                if (slotCount > 0 && inventory.GetSlotValue(i) != value) continue;
                int canAdd = maxStack - slotCount;
                if (canAdd <= 0) continue;
                int add = Math.Min(canAdd, count - moved);
                inventory.AddSlotItems(i, value, add);
                moved += add;
                if (moved >= count) break;
            }
            return moved;
        }

        static string CrafterKindFor(FormattedGridPlacementContext context) =>
            context.MappingMode == FormattedGridMappingMode.FurnaceInputRow
                ? CrafterKind.Furnace
                : CrafterKind.CraftingTable;

        static void CountPlan(List<PlacementAction> plan, out int plannedTransferCount, out int missingIngredientCount) {
            plannedTransferCount = 0;
            missingIngredientCount = 0;
            foreach (PlacementAction action in plan) {
                if (action.MissingLabel != null) {
                    missingIngredientCount++;
                }
                else if (action.NeedsTransfer) {
                    plannedTransferCount++;
                }
            }
        }
    }
}
