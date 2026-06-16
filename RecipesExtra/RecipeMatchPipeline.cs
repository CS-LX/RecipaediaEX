using GameEntitySystem;
using RecipaediaEX.Implementation;
using ZLinq;

namespace RecipaediaEX {
    public enum CrafterMatchMode {
        CraftingTable,
        Furnace,
    }

    public readonly struct RecipeMatchResult {
        public FormattedRecipe? Recipe { get; init; }
        public bool IsHint { get; init; }
    }

    /// <summary>
    /// 工作台 / 熔炉：动态配方 → 静态匹配 → 原料形状匹配 → 约束检查。
    /// </summary>
    public static class RecipeMatchPipeline {
        public static RecipeMatchResult Resolve(IRecipe actual, CrafterMatchMode mode, Project? project) {
            if (actual is not FormattedRecipe actualFormatted) return default;
            if (actualFormatted.Ingredients.AsValueEnumerable().All(string.IsNullOrEmpty)) return default;

            float heatLevel = actualFormatted.RequiredHeatLevel;
            float playerLevel = actualFormatted.RequiredPlayerLevel;
            FormattedRecipe? matched = null;
            bool fromDynamic = false;

            if (project != null) {
                matched = RecipaediaEXManager.FindDynamicRecipe(actual, project) as FormattedRecipe;
                if (matched != null) fromDynamic = true;
            }
            if (matched == null) matched = TryStaticMatch(actual, mode);
            if (matched == null) matched = TryIngredientOnlyMatch(actualFormatted);

            FormattedRecipe? result = RecipeConstraintChecker.Apply(matched, heatLevel, playerLevel);
            bool isHint = RecipeConstraintChecker.IsHintOnly(result);
            if (!isHint) result = FilterProductiveRecipe(result, mode);

            if (result != null) {
                RecipaediaEXManager.NotifyRecipeMatched(actual, result, fromDynamic);
            }
            return new RecipeMatchResult { Recipe = result, IsHint = isHint };
        }

        static FormattedRecipe? TryStaticMatch(IRecipe actual, CrafterMatchMode mode) {
            foreach (IRecipe recipe in RecipaediaEXManager.Recipes) {
                if (mode == CrafterMatchMode.CraftingTable && recipe is not OriginalCraftingRecipe) continue;
                if (mode == CrafterMatchMode.Furnace && recipe is not OriginalSmeltingRecipe) continue;
                if (recipe.Match(actual)) return recipe as FormattedRecipe;
            }
            return null;
        }

        static FormattedRecipe? TryIngredientOnlyMatch(FormattedRecipe actual) {
            return RecipaediaEXManager.Recipes
                .AsValueEnumerable()
                .OfType<FormattedRecipe>()
                .Where(recipe => recipe.MatchIngredientsOnly(actual))
                .OrderBy(recipe => recipe.MatchPriority)
                .FirstOrDefault();
        }

        static FormattedRecipe? FilterProductiveRecipe(FormattedRecipe? recipe, CrafterMatchMode mode) {
            if (recipe == null) return null;
            if (mode == CrafterMatchMode.CraftingTable) {
                return recipe is OriginalCraftingRecipe { ResultValue: not 0 } ? recipe : null;
            }
            return recipe is OriginalSmeltingRecipe { RequiredHeatLevel: > 0 } ? recipe : null;
        }
    }
}
