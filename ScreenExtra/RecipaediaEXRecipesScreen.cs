using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Engine;
using Game;

namespace RecipaediaEX.UI {
    public class RecipaediaEXRecipesScreen : Screen
	{
		public ButtonWidget m_prevRecipeButton;

        public ButtonWidget m_prevInStackButton;

		public ButtonWidget m_nextRecipeButton;

        public CanvasWidget m_recipeDescriptorsCanvas;

		public RecipaediaEXRecipesScreen()
		{
			XElement node = RecipaediaEXLoader.RequestScreenFile("RecipaediaEXRecipesScreen");
			LoadContents(this, node);
            m_prevRecipeButton = Children.Find<ButtonWidget>("PreviousRecipe");
            m_prevInStackButton = Children.Find<ButtonWidget>("PreviousRecipeInStack");
			m_nextRecipeButton = Children.Find<ButtonWidget>("NextRecipe");
            m_recipeDescriptorsCanvas = Children.Find<CanvasWidget>("RecipeDescriptors");
		}

		public override void Enter(object[] parameters)
		{
            IRecipaediaRecipeItem recipeItem = (IRecipaediaRecipeItem)parameters[0];
		}

		public override void Update()
		{
			if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
			{
				ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
			}
		}
	}
}
