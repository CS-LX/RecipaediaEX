namespace RecipaediaEX.Search {
    public sealed class RecipaediaSearchFilterState {
        public string NameText = string.Empty;
        public bool HasRecipe;
        public bool CanUse;
        public string ItemType = string.Empty;
        public string PackId = string.Empty;
        public string ModName = string.Empty;
        public string CrafterName = string.Empty;
        public string RecipeType = string.Empty;
        public string IngredientName = string.Empty;
        public string ProductName = string.Empty;
        public string ExcludeText = string.Empty;

        public int ActiveFilterCount {
            get {
                int count = 0;
                if (HasRecipe) count++;
                if (CanUse) count++;
                if (!string.IsNullOrWhiteSpace(ItemType)) count++;
                if (!string.IsNullOrWhiteSpace(PackId)) count++;
                if (!string.IsNullOrWhiteSpace(ModName)) count++;
                if (!string.IsNullOrWhiteSpace(CrafterName)) count++;
                if (!string.IsNullOrWhiteSpace(RecipeType)) count++;
                if (!string.IsNullOrWhiteSpace(IngredientName)) count++;
                if (!string.IsNullOrWhiteSpace(ProductName)) count++;
                if (!string.IsNullOrWhiteSpace(ExcludeText)) count++;
                return count;
            }
        }
    }
}
