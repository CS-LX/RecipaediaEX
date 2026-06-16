using System.Globalization;
using Game;
using RecipaediaEX.Implementation;

namespace RecipaediaEX {
    /// <summary>
    /// 原料形状已匹配后，检查热等级 / 玩家等级；失败时返回仅含 <see cref="FormattedRecipe.Message"/> 的提示配方。
    /// </summary>
    public static class RecipeConstraintChecker {
        const string ManagerSection = "CraftingRecipesManager";

        public static FormattedRecipe? Apply(FormattedRecipe? matched, float heatLevel, float playerLevel) {
            if (matched == null) return null;
            if (heatLevel < matched.RequiredHeatLevel) {
                return CreateSyntheticHint(
                    heatLevel > 0f
                        ? LanguageControl.Get(ManagerSection, 1)
                        : LanguageControl.Get(ManagerSection, 0)
                );
            }
            if (RecipaediaEXManager.EnableLevelRestrictions
                && playerLevel < matched.RequiredPlayerLevel) {
                return CreateSyntheticHint(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        LanguageControl.Get(ManagerSection, matched.RequiredHeatLevel > 0f ? 3 : 2),
                        matched.RequiredPlayerLevel.ToString(CultureInfo.InvariantCulture)
                    )
                );
            }
            return matched;
        }

        public static bool IsHintOnly(FormattedRecipe? recipe) {
            return recipe is { ResultValue: 0 } && !string.IsNullOrEmpty(recipe.Message);
        }

        static OriginalCraftingRecipe CreateSyntheticHint(string message) {
            return new OriginalCraftingRecipe { Message = message, ResultValue = 0 };
        }
    }
}
