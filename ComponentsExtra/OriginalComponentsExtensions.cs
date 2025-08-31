using System;
using System.IO;
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
        /// 将3*3配方缩放到6*6的
        /// </summary>
        /// <param name="recipes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string[] ScalingIngredients(string[] recipes) {
            switch (recipes.Length) {
                case 36: return recipes;
                case 9: {
                    var ingredients2 = new string[36];
                    ingredients2[0] = recipes[0];
                    ingredients2[1] = recipes[1];
                    ingredients2[2] = recipes[2];
                    ingredients2[6] = recipes[3];
                    ingredients2[7] = recipes[4];
                    ingredients2[8] = recipes[5];
                    ingredients2[12] = recipes[6];
                    ingredients2[13] = recipes[7];
                    ingredients2[14] = recipes[8];
                    return ingredients2;
                }
                default: throw new ArgumentOutOfRangeException(nameof(recipes));
            }
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
        
        /// <summary>
        /// 匹配原料序列
        /// </summary>
        /// <param name="requiredIngredient"></param>
        /// <param name="actualIngredient"></param>
        /// <returns></returns>
        public static bool CompareIngredientsArray(string[]? requiredIngredient, string[]? actualIngredient)
        {
            // 如果 x 和 y 其中一个为 null，只有在两个都为 null 时才返回 true
            if (requiredIngredient == null || actualIngredient == null)
                return requiredIngredient == actualIngredient;

            // 如果长度不同，则直接返回 false
            if (requiredIngredient.Length != actualIngredient.Length)
                return false;

            // 使用 CompareIngredients 比较每个元素
            for (int i = 0; i < requiredIngredient.Length; i++)
            {
                if (!CraftingRecipesManager.CompareIngredients(requiredIngredient[i], actualIngredient[i]))
                    return false;
            }

            return true;
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
                if (adHocCraftingRecipe != null) {
                    T formattedRecipe = adHocCraftingRecipe.ToFormattedRecipe<T>();
                    if (formattedRecipe.Match(actual)) return formattedRecipe;
                }
            }
            return RecipaediaEXManager.FindMatchingRecipe<T>(actual);
        }
    }
}