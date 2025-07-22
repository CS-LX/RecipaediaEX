using Game;

namespace RecipaediaEX.UI {
    public abstract class RecipeDescriptor : CanvasWidget {
        public RecipaediaEXRecipesScreen m_belongingScreen;

        protected RecipeDescriptor(RecipaediaEXRecipesScreen belongingScreen) {
            m_belongingScreen = belongingScreen;
            Initialize();
        }

        public abstract void Initialize();

        public abstract void Show(IRecipe recipe);

        public abstract void Hide();
    }
}