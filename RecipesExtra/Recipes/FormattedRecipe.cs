using System.Collections.Generic;
using RecipaediaEX.ComponentsExtra;
using ZLinq;

namespace RecipaediaEX.Implementation {
    public abstract class FormattedRecipe : IRecipe {
        public const int MaxSize = 3;

        public int ResultValue;

        public int ResultCount;

        public int RemainsValue;

        public int RemainsCount;

        public float RequiredHeatLevel;

        public float RequiredPlayerLevel;

        public string[] Ingredients = new string[36];

        public string Description;

        public string Message;
        
        public HashSet<string[]> TransformedIngredients = new();

        /// <summary>
        /// 在配方表中的显示顺序，DisplayOrder越小，配方越靠前
        /// </summary>
        public int DisplayOrder = 0;

        string IRecipe.Description => Description;
        string IRecipe.Message => Message;
        int IRecipe.DisplayOrder => DisplayOrder;

        public virtual bool Match(IRecipe actual) {
            if (actual is not OriginalCraftingRecipe craftingRecipe) return false;
            return TransformedIngredients.AsValueEnumerable().Any(ingredients => OriginalComponentsExtensions.CompareIngredientsArray(ingredients, craftingRecipe.Ingredients));
        }

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
        }
    }
}