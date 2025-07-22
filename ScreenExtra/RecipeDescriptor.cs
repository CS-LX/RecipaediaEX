using Game;

namespace RecipaediaEX.UI {
    public abstract class RecipeDescriptor : CanvasWidget {

        protected RecipeDescriptor(IRecipe recipe) {
            Initialize(recipe);
        }

        public abstract void Initialize(IRecipe recipe);
    }
}