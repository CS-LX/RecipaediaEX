using System;
using System.Collections.Generic;
using Engine;
using Game;
using RecipaediaEX.UI;

namespace RecipaediaEX.Overlay {
    public class RecipaediaOverlayRecipePreview : CanvasWidget, IRecipaediaRecipeNavigator {
        public ScrollPanelWidget m_scrollPanel;
        public GridPanelWidget m_gridPanel;
        public LabelWidget m_emptyLabel;
        public RecipaediaCraftingContext m_context;
        readonly Dictionary<Type, RecipeDescriptor> m_descriptorCache = [];
        readonly List<RecipeDescriptor> m_activeDescriptors = [];

        public RecipaediaOverlayRecipePreview() {
            m_scrollPanel = new ScrollPanelWidget {
                Direction = LayoutDirection.Vertical,
                HorizontalAlignment = WidgetAlignment.Stretch,
                VerticalAlignment = WidgetAlignment.Stretch,
                ClampToBounds = true,
            };
            Children.Add(m_scrollPanel);

            m_gridPanel = new GridPanelWidget {
                ColumnsCount = 1,
            };
            m_scrollPanel.Children.Add(m_gridPanel);

            m_emptyLabel = new LabelWidget {
                Text = LanguageControl.GetContentWidgets(RecipaediaCraftingOverlayDialog.LanguageName, 1),
                Color = new Color(180, 180, 180, 255),
                HorizontalAlignment = WidgetAlignment.Center,
                VerticalAlignment = WidgetAlignment.Center,
                WordWrap = true,
                IsVisible = false,
            };
            Children.Add(m_emptyLabel);
        }

        public void SetContext(RecipaediaCraftingContext context) => m_context = context;

        public void ShowRecipes(IReadOnlyList<IRecipe> recipes, int startIndex = 0) {
            ClearDescriptors();
            if (recipes.Count == 0) {
                m_emptyLabel.IsVisible = true;
                return;
            }

            m_emptyLabel.IsVisible = false;
            m_gridPanel.RowsCount = recipes.Count;
            int row = 0;
            for (int i = 0; i < recipes.Count; i++) {
                IRecipe recipe = recipes[i];
                if (!RecipeDescriptorRegistry.TryGetDescriptorType(recipe.GetType(), out Type descriptorType)) continue;
                if (!m_descriptorCache.TryGetValue(descriptorType, out RecipeDescriptor? descriptor)) {
                    descriptor = RecipeDescriptorRegistry.CreateDescriptor(descriptorType, this)!;
                    m_descriptorCache[descriptorType] = descriptor;
                }

                string nameSuffix = recipes.Count > 1
                    ? string.Format(LanguageControl.GetContentWidgets(nameof(RecipaediaRecipesScreen), 1), i + 1)
                    : string.Empty;
                descriptor.Show(recipe, nameSuffix);
                descriptor.IsVisible = true;
                descriptor.ColorTransform = Color.White;
                descriptor.HorizontalAlignment = WidgetAlignment.Center;
                m_gridPanel.Children.Add(descriptor);
                m_gridPanel.SetWidgetCell(descriptor, new Point2(0, row));
                m_activeDescriptors.Add(descriptor);
                row++;
            }

            if (m_activeDescriptors.Count == 0) {
                m_emptyLabel.IsVisible = true;
                m_gridPanel.RowsCount = 0;
            }
            else {
                m_gridPanel.RowsCount = m_activeDescriptors.Count;
            }
            m_scrollPanel.ScrollPosition = 0f;
        }

        public void ShowItemRecipes(IRecipaediaRecipeItem item) {
            List<IRecipe> recipes = RecipaediaOverlayRecipeResolver.ResolvePreviewRecipes(item, m_context);
            ShowRecipes(recipes);
        }

        public void ClearDescriptors() {
            foreach (RecipeDescriptor descriptor in m_activeDescriptors) {
                descriptor.Hide();
                descriptor.IsVisible = false;
                m_gridPanel.Children.Remove(descriptor);
            }
            m_activeDescriptors.Clear();
            m_emptyLabel.IsVisible = false;
            m_scrollPanel.ScrollPosition = 0f;
        }
    }
}
