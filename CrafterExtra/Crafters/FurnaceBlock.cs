using System;

namespace RecipaediaEX.Implementation {
    public class FurnaceBlock : Game.FurnaceBlock, ICrafter {
        public new static int Index = 64;
        public bool IsCrafter(int blockValue, Type recipeType) => recipeType.IsAssignableTo(typeof(OriginalSmeltingRecipe));
    }
}