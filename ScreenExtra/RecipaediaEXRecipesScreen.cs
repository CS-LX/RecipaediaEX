using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using Engine;
using Engine.Serialization;
using Game;
using RecipaediaEX.Overlay;
using ZLinq;

namespace RecipaediaEX.UI {
    public class RecipaediaEXRecipesScreen : Screen, IRecipaediaRecipeNavigator
    {
        public List<IRecipe> m_recipes = new();
        public int m_index = 0;
        public Stack<RecipesPresentation> m_presentations = new();

        public ButtonWidget m_prevRecipeButton;
        public ButtonWidget m_prevInStackButton;
        public ButtonWidget m_nextRecipeButton;
        public CanvasWidget m_recipeDescriptorsCanvas;
        public CanvasWidget m_crafterButtonsCanvas;
        public RecipeDescriptor m_currentRecipeDescriptor;
        public CrafterButtonWidget m_currentCrafterButton;

        public RecipaediaEXRecipesScreen() {
            XElement node = RecipaediaEXLoader.RequestScreenFile("RecipaediaEXRecipesScreen");
            LoadContents(this, node);
            m_prevRecipeButton = Children.Find<ButtonWidget>("PreviousRecipe");
            m_prevInStackButton = Children.Find<ButtonWidget>("PreviousRecipeInStack");
            m_nextRecipeButton = Children.Find<ButtonWidget>("NextRecipe");
            m_recipeDescriptorsCanvas = Children.Find<CanvasWidget>("RecipeDescriptors");
            m_crafterButtonsCanvas = Children.Find<CanvasWidget>("Crafters");
        }

        public override void Enter(object[] parameters) {
            IRecipaediaRecipeItem recipeItem = (IRecipaediaRecipeItem)parameters[0];
            m_recipes.AddRange(RecipaediaEXManager.Recipes.AsValueEnumerable().Where(x => recipeItem.Match(x)).OrderBy(x => x.DisplayOrder).ToArray());
            RecipeDescriptorRegistry.EnsureScanned();
        }

        public void ShowRecipes(IReadOnlyList<IRecipe> recipes, int startIndex = 0) {
            SwitchToNewRecipe([.. recipes], startIndex);
        }

        public override void Update() {
            IRecipe currentRecipe = m_recipes[m_index];
            string nameSuffix = string.Format(LanguageControl.GetContentWidgets(nameof(RecipaediaRecipesScreen), 1), m_index + 1);
            if (m_currentRecipeDescriptor == null) ShowCurrentDescriptor(RecipeDescriptorRegistry.TryGetDescriptorType(currentRecipe.GetType(), out Type descriptorType) ? descriptorType : null, currentRecipe, nameSuffix);

            m_prevRecipeButton.IsEnabled = m_index > 0;
            m_nextRecipeButton.IsEnabled = m_index < m_recipes.Count - 1;
            if (m_prevRecipeButton.IsClicked) {
                m_index = MathUtils.Max(m_index - 1, 0);
                HideCurrentDescriptor();
            }
            if (m_nextRecipeButton.IsClicked) {
                m_index = MathUtils.Min(m_index + 1, m_recipes.Count - 1);
                HideCurrentDescriptor();
            }

            m_prevInStackButton.IsEnabled = m_presentations.Count > 0;
            if (Input.Back || Input.Cancel || m_prevInStackButton.IsClicked) {
                if (m_presentations.Count > 0) {
                    SwitchToPreviousRecipe();
                }
                else {
                    Exit();
                }
            }
            if (Children.Find<ButtonWidget>("TopBar.Back").IsClicked) {
                Exit();
            }
        }

        public void SwitchToNewRecipe(List<IRecipe> recipes, int index) {
            if (m_currentRecipeDescriptor != null) HideCurrentDescriptor();
            m_presentations.Push(new RecipesPresentation(m_recipes, m_index));
            m_recipes = recipes;
            m_index = index;
        }

        public void SwitchToPreviousRecipe() {
            if (m_currentRecipeDescriptor != null) HideCurrentDescriptor();
            RecipesPresentation previousPresentation = m_presentations.Pop();
            m_recipes = previousPresentation.m_recipes;
            m_index = previousPresentation.m_index;
        }

        public void ShowCurrentDescriptor(Type descriptorType, IRecipe recipe, string nameSuffix = null) {
            if (descriptorType == null) return;
            if (m_currentRecipeDescriptor != null) HideCurrentDescriptor();
            if (m_currentCrafterButton != null) m_crafterButtonsCanvas.Children.Remove(m_currentCrafterButton);

            RecipeDescriptor recipeDescriptor = RecipeDescriptorRegistry.CreateDescriptor(descriptorType, this)!;
            recipeDescriptor.Show(recipe, nameSuffix);
            recipeDescriptor.IsVisible = true;
            recipeDescriptor.HorizontalAlignment = WidgetAlignment.Center;
            recipeDescriptor.VerticalAlignment = WidgetAlignment.Center;
            m_recipeDescriptorsCanvas.Children.Clear();
            m_recipeDescriptorsCanvas.Children.Add(recipeDescriptor);
            m_currentRecipeDescriptor = recipeDescriptor;

            m_currentCrafterButton = recipeDescriptor.GetCrafterButton(recipe);
            if (m_currentCrafterButton != null)
                m_crafterButtonsCanvas.Children.Add(m_currentCrafterButton);
        }

        public void HideCurrentDescriptor() {
            if (m_currentRecipeDescriptor == null) return;

            m_currentRecipeDescriptor.Hide();
            m_currentRecipeDescriptor.IsVisible = false;
            m_currentRecipeDescriptor = null;
        }

        public void Exit() {
            m_presentations.Clear();
            m_recipes.Clear();
            m_index = 0;
            HideCurrentDescriptor();
            ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
        }

        public struct RecipesPresentation(List<IRecipe> mRecipes, int mIndex) {
            public List<IRecipe> m_recipes = mRecipes;
            public int m_index = mIndex;
        }
    }
}
