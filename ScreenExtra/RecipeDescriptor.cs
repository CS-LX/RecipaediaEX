using Game;

namespace RecipaediaEX.UI {
    public abstract class RecipeDescriptor(IRecipaediaRecipeNavigator navigator) : CanvasWidget {
        public IRecipaediaRecipeNavigator m_navigator = navigator;

        public abstract void Show(IRecipe recipe, string nameSuffix);

        public abstract void Hide();

        public abstract CrafterButtonWidget GetCrafterButton(IRecipe recipe);
    }
}
