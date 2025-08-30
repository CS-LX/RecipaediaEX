using Game;
using RecipaediaEX.Implementation;

namespace RecipaediaEX.ComponentsExtra.Implementation {
    public static class OriginalComponentsExtensions {
        /// <summary>
        /// 将原版的CraftingRecipe转为RecipaediaEX的FormattedRecipe的子类
        /// </summary>
        /// <param name="originalRecipe">原版配方</param>
        /// <typeparam name="T">FormattedRecipe的子类的类型</typeparam>
        /// <returns>FormattedRecipe的实例</returns>
        public static T ToFormattedRecipe<T>(this CraftingRecipe originalRecipe) where T : FormattedRecipe, new() {
            return new T {
                ResultValue = originalRecipe.ResultValue,
                ResultCount = originalRecipe.ResultCount,
                Description = originalRecipe.Description,
                Ingredients = originalRecipe.Ingredients,
                DisplayOrder = originalRecipe.DisplayOrder,
                Message = originalRecipe.Message,
                RemainsCount = originalRecipe.RemainsCount,
                RemainsValue = originalRecipe.RemainsValue,
                RequiredHeatLevel = originalRecipe.RequiredHeatLevel,
                RequiredPlayerLevel = originalRecipe.RequiredPlayerLevel
            };
        }
        
        /// <summary>
        /// 对原版工作方块寻找配方的简单封装，允许寻找零时配方
        /// </summary>
        /// <param name="subsystemTerrain"></param>
        /// <param name="actual">玩家放入的实际配方</param>
        /// <typeparam name="T">配方的类型</typeparam>
        /// <returns>配方实例</returns>
        public static T FindCraftingRecipe<T>(SubsystemTerrain subsystemTerrain, T actual) where T : FormattedRecipe, new() {
            foreach (var block in BlocksManager.Blocks) {
                CraftingRecipe adHocCraftingRecipe = block.GetAdHocCraftingRecipe(subsystemTerrain, actual.Ingredients, actual.RequiredHeatLevel, actual.RequiredPlayerLevel);
                if (adHocCraftingRecipe != null && CraftingRecipesManager.MatchRecipe(adHocCraftingRecipe.Ingredients, actual.Ingredients)) {
                    return adHocCraftingRecipe.ToFormattedRecipe<T>();
                }
            }
            return RecipaediaEXManager.FindMatchingRecipe<T>(actual);
        }
    }
}