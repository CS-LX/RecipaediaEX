using Game;
using RecipaediaEX.ComponentsExtra;
using RecipaediaEX;
using System.Collections.Generic;
using System.Linq;
using TemplatesDatabase;
using ZLinq;
using System;

namespace RecipaediaEX.Implementation {
    public abstract class FormattedRecipe : IRecipe {
        public int ResultValue;

        public int ResultCount;

        public int RemainsValue;

        public int RemainsCount;

        public float RequiredHeatLevel;

        public float RequiredPlayerLevel;

        public string[] Ingredients = new string[36];

        public string Description;

        public string Message;

        public bool LogErrorOnMatchFail = false;

        public virtual int MatchPriority => 0;

        public HashSet<string[]> TransformedIngredients = new();
        
        public ValuesDictionary ExtraValues = new();

        /// <summary>
        /// 在配方表中的显示顺序，DisplayOrder越小，配方越靠前
        /// </summary>
        public int DisplayOrder = 0;
        int IRecipe.DisplayOrder => DisplayOrder;

        public virtual bool Match(IRecipe actual) {
            try {
                if (actual is not FormattedRecipe formattedRecipe) return false;
                return TransformedIngredients.AsValueEnumerable().Any(
                    ingredients => CompareIngredientsArray(ingredients, formattedRecipe.Ingredients)
                );
            }
            catch(Exception e) {
                if (LogErrorOnMatchFail) {
                    Engine.Log.Error("[RecipaediaEX]Formatted Recipe Match Error: " + e);
                }
                return false;
            }
        }
        public virtual bool CompareIngredientsArray(string[]? requiredIngredient, string[]? actualIngredient) {
            if (requiredIngredient == null || actualIngredient == null)
                return requiredIngredient == actualIngredient;

            // 如果长度不同，则直接返回 false
            if (requiredIngredient.Length != actualIngredient.Length)
                return false;

            // 使用 CompareIngredients 比较每个元素
            for (int i = 0; i < requiredIngredient.Length; i++) {
                if (!CompareIngredient(requiredIngredient[i], actualIngredient[i]))
                    return false;
            }

            return true;
        }
        public virtual bool CompareIngredient(string requiredIngredient, string actualIngredient, bool throwOnNotSpecified = false) {
            if (!throwOnNotSpecified && actualIngredient != null) {//如果不想CraftingRecipesManager.CompareIngredients报错，则预先检查报错case，如果符合报错case则直接返回false
                CraftingRecipesManager.DecodeIngredient(actualIngredient, out string craftingId2, out int? data2);
                if (!data2.HasValue) return false;
            }
            return CraftingRecipesManager.CompareIngredients(requiredIngredient, actualIngredient);
        }

        public virtual T GetExtraValue<T>(string key, T defaultValue) => ExtraValues.GetValue(key, defaultValue);

        public void SetExtraValue<T>(string key, T value) => ExtraValues.SetValue(key, value);

        public void PreTransformIngredients() {
            TransformedIngredients.Clear();
            for (int i = 0; i < 2; i++) {
                for (int j = -6; j <= 6; j++) {
                    for (int k = -6; k <= 6; k++) {
                        bool flip = i != 0;
                        string[] array = new string[36];
                        if (!OriginalComponentsExtensions.TransformRecipe(array, Ingredients, k, j, flip)) {
                            continue;
                        }
                        TransformedIngredients.Add(array);
                    }
                }
            }
            UpdateMatchedIngredientBlockValues();
        }

        public void UpdateMatchedIngredientBlockValues() {
            SetExtraValue(RecipeExtraKeys.MatchedIngredientBlockValues, ExpandIngredientsToBlockValues(Ingredients));
        }

        public static int[] ExpandIngredientsToBlockValues(IEnumerable<string> ingredients) {
            HashSet<int> values = new();
            foreach (string ingredient in ingredients) {
                if (!string.IsNullOrEmpty(ingredient)) {
                    AddIngredientBlockValues(values, ingredient);
                }
            }
            return values.ToArray();
        }

        public static int[] ExpandIngredientToBlockValues(string ingredient) {
            if (string.IsNullOrEmpty(ingredient)) return Array.Empty<int>();
            HashSet<int> values = new();
            AddIngredientBlockValues(values, ingredient);
            return values.ToArray();
        }

        static void AddIngredientBlockValues(HashSet<int> values, string ingredient) {
            CraftingRecipesManager.DecodeIngredient(ingredient, out string craftingId, out int? data);
            foreach (Block block in BlocksManager.FindBlocksByCraftingId(craftingId)) {
                values.Add(Terrain.MakeBlockValue(block.BlockIndex, 0, data ?? 0));
            }
        }
    }
}