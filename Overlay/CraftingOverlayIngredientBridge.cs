using System;
using System.Collections.Generic;
using Game;
using RecipaediaEX.Implementation;

namespace RecipaediaEX.Overlay {
    /// <summary>
    /// 内容模组可注入 IE 合成 ID 解析（如 <c>I-GoldIngot</c>），REX 默认真原版行为。
    /// </summary>
    public static class CraftingOverlayIngredientBridge {
        public static Func<int, string>? BlockValueToCraftingId { get; set; }
        public static Func<string, int[]>? ExpandIngredientBlockValues { get; set; }
        public static Func<string, int>? DecodeIngredientResult { get; set; }

        public static string ToCraftingId(int blockValue) {
            string? hooked = BlockValueToCraftingId?.Invoke(blockValue);
            if (!string.IsNullOrEmpty(hooked)) return hooked;
            int content = Terrain.ExtractContents(blockValue);
            int data = Terrain.ExtractData(blockValue);
            Block block = BlocksManager.Blocks[content];
            return block.GetCraftingId(blockValue) + ":" + data.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public static int[] ExpandBlockValues(string ingredient) {
            if (string.IsNullOrEmpty(ingredient)) return [];
            if (s_expandCache.TryGetValue(ingredient, out int[]? cached)) return cached;

            int[]? hooked = ExpandIngredientBlockValues?.Invoke(ingredient);
            int[] result = hooked is { Length: > 0 } ? hooked : FormattedRecipe.ExpandIngredientToBlockValues(ingredient);
            s_expandCache[ingredient] = result;
            return result;
        }

        static readonly Dictionary<string, int[]> s_expandCache = new();

        public static bool TryDecodeDisplayBlockValue(string ingredient, out int blockValue) {
            blockValue = 0;
            if (TryDecodeHookedIngredient(ingredient, out blockValue)) return true;
            foreach (int value in ExpandBlockValues(ingredient)) {
                if (!IsValidBlockValue(value)) continue;
                blockValue = value;
                return true;
            }
            return false;
        }

        static bool TryDecodeHookedIngredient(string ingredient, out int blockValue) {
            blockValue = 0;
            if (DecodeIngredientResult == null) return false;
            int hooked = DecodeIngredientResult.Invoke(ingredient);
            if (!IsValidBlockValue(hooked)) return false;
            blockValue = hooked;
            return true;
        }

        static bool IsValidBlockValue(int value) {
            if (value <= 0) return false;
            int contents = Terrain.ExtractContents(value);
            return contents >= 0 && contents < BlocksManager.Blocks.Length;
        }
    }
}
