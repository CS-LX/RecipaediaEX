using System;
using Game;
using RecipaediaEX.Implementation;

namespace RecipaediaEX.ComponentsExtra {
    public static class OriginalComponentsExtensions {
        /// <summary>
        /// 将原版的CraftingRecipe转为RecipaediaEX的FormattedRecipe的子类
        /// </summary>
        /// <param name="originalRecipe">原版配方</param>
        /// <typeparam name="T">FormattedRecipe的子类的类型</typeparam>
        /// <returns>FormattedRecipe的实例</returns>
        public static T ToFormattedRecipe<T>(this CraftingRecipe originalRecipe) where T : FormattedRecipe, new() {
             T recipe = new() {
                ResultValue = originalRecipe.ResultValue,
                ResultCount = originalRecipe.ResultCount,
                Description = originalRecipe.Description,
                Ingredients = ScalingIngredients(originalRecipe.Ingredients),
                DisplayOrder = originalRecipe.DisplayOrder,
                Message = originalRecipe.Message,
                RemainsCount = originalRecipe.RemainsCount,
                RemainsValue = originalRecipe.RemainsValue,
                RequiredHeatLevel = originalRecipe.RequiredHeatLevel,
                RequiredPlayerLevel = originalRecipe.RequiredPlayerLevel
            };
            recipe.PreTransformIngredients();
            return recipe;
        }

        /// <summary>
        /// 将小于6*6的配方缩放到6*6的
        /// 使用时需要注意缩放的配方大小必须是不大于36的完全平方数，以确认配方宽度
        /// </summary>
        /// <param name="ingredientsBeforeScaling"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string[] ScalingIngredients(string[] ingredientsBeforeScaling) {
            if (ingredientsBeforeScaling.Length == 36) return ingredientsBeforeScaling;
            int width = ingredientsBeforeScaling.Length switch {
                9 => 3,
                16 => 4,
                25 => 5,
                4 => 2,
                1 => 1,
                _ => throw new ArgumentOutOfRangeException(nameof(ingredientsBeforeScaling))
            };
            string[] ingredientsAfterScaling = new string[36];
            for (int i = 0; i < width; i++) {
                for(int j = 0; j < width; j++) {
                    ingredientsAfterScaling[i * 6 + j] = ingredientsBeforeScaling[i * width + j];
                }
            }
            return ingredientsAfterScaling;
        }
        
        /// <summary>
        /// 适用于6*6配方的，性能更好的配方transform方法
        /// </summary>
        /// <param name="transformedIngredients"></param>
        /// <param name="ingredients"></param>
        /// <param name="shiftX"></param>
        /// <param name="shiftY"></param>
        /// <param name="flip"></param>
        /// <returns></returns>
        public static bool TransformRecipe(string[] transformedIngredients, string[] ingredients, int shiftX, int shiftY, bool flip) {
            // 初始化 transformedIngredients 数组
            Array.Clear(transformedIngredients, 0, transformedIngredients.Length);

            int baseIndex = 0;  // 基本索引，用于计算位置

            for (int j = 0; j < 6; j++) {
                int num2 = j + shiftY;
                int destIndex = num2 * 6; // 每行的起始索引

                if (num2 < 0 || num2 >= 6) {
                    // num2 超出范围，跳过当前行
                    for (int k = 0; k < 6; k++) {
                        if (!string.IsNullOrEmpty(ingredients[baseIndex + k])) {
                            return false;
                        }
                    }
                    baseIndex += 6; // 移动到下一行
                    continue;
                }

                for (int k = 0; k < 6; k++) {
                    int num = (flip ? (5 - k) : k) + shiftX;
                    if (num >= 0 && num < 6) {
                        transformedIngredients[num + destIndex] = ingredients[baseIndex + k];
                    }
                    else if (!string.IsNullOrEmpty(ingredients[baseIndex + k])) {
                        return false;
                    }
                }
                baseIndex += 6; // 移动到下一行
            }
            return true;
        }
    }
}