namespace RecipaediaEX.Implementation {
    public class CraftingTableBlock : Game.CraftingTableBlock, ICrafter {
        public new static int Index = 27;
        public bool IsCrafter(int blockValue, IRecipe recipe) => recipe.GetType().IsAssignableTo(typeof(OriginalCraftingRecipe));
    }
}