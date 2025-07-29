using System;
using Game;

namespace RecipaediaEX.Implementation {
    public class CraftingTableBlock : Game.CraftingTableBlock, ICrafter {
        public new static int Index = 27;
        public bool IsCrafter(int blockValue, Type recipeType) => recipeType.IsAssignableTo(typeof(OriginalCraftingRecipe));
    }
}