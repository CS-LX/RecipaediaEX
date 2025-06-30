﻿using Game;

namespace RecipaediaEX {
    public class CraftingRecipe : IRecipe {
        public const int MaxSize = 3;

        public int ResultValue;

        public int ResultCount;

        public int RemainsValue;

        public int RemainsCount;

        public float RequiredHeatLevel;

        public float RequiredPlayerLevel;

        public string[] Ingredients = new string[9];

        public string Description;

        public string Message;

        /// <summary>
        /// 在配方表中的显示顺序，DisplayOrder越小，配方越靠前
        /// </summary>
        public int DisplayOrder = 0;

        string IRecipe.Description => Description;
        string IRecipe.Message => Message;
        int IRecipe.DisplayOrder => DisplayOrder;

        public bool Match(IRecipe other) {
            if (other == null || other is not CraftingRecipe craftingRecipe) return false;
            // TODO 要改
            return CraftingRecipesManager.MatchRecipe(Ingredients, craftingRecipe.Ingredients);
        }
    }
}