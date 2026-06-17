using System;
using System.Collections.Generic;
using Engine;
using Game;
using RecipaediaEX.UI;

namespace RecipaediaEX.Overlay {
    /// <summary>
    /// 二级配方预览：JEI Category 式紧凑卡片网格 + 纵向滚动。
    /// Descriptor 内部布局不变；缩放仅由 <see cref="RecipaediaOverlayDescriptorSlot"/> 承担。
    /// </summary>
    public class RecipaediaOverlayRecipePreview : CanvasWidget {
        public const float MaxContentWidth = 460f;

        public const float NaturalWidthUpperBound = 480f;

        public ScrollPanelWidget m_scrollPanel;
        public GridPanelWidget m_gridPanel;
        public LabelWidget m_emptyLabel;
        public RecipaediaCraftingContext m_context;
        IRecipaediaRecipeNavigator m_navigator = null!;
        IRecipaediaOverlayDescriptorHost m_descriptorHost = null!;
        readonly List<RecipaediaOverlayDescriptorSlot> m_activeSlots = [];

        public RecipaediaOverlayRecipePreview() {
            m_scrollPanel = new ScrollPanelWidget {
                Direction = LayoutDirection.Vertical,
                HorizontalAlignment = WidgetAlignment.Stretch,
                VerticalAlignment = WidgetAlignment.Stretch,
                ClampToBounds = true,
            };
            Children.Add(m_scrollPanel);

            m_gridPanel = new GridPanelWidget {
                HorizontalAlignment = WidgetAlignment.Center,
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

        public void SetNavigator(IRecipaediaRecipeNavigator navigator) => m_navigator = navigator;

        public void SetDescriptorHost(IRecipaediaOverlayDescriptorHost host) => m_descriptorHost = host;

        public void DisplayRecipes(IReadOnlyList<IRecipe> recipes) {
            ClearDescriptors();
            if (recipes.Count == 0) {
                m_emptyLabel.IsVisible = true;
                return;
            }

            m_emptyLabel.IsVisible = false;
            var pending = new List<(Type descriptorType, IRecipe recipe, string nameSuffix)>();
            for (int i = 0; i < recipes.Count; i++) {
                IRecipe recipe = recipes[i];
                if (!RecipeDescriptorRegistry.TryGetDescriptorType(recipe.GetType(), out Type descriptorType)) continue;

                string nameSuffix = recipes.Count > 1
                    ? string.Format(LanguageControl.GetContentWidgets(nameof(RecipaediaRecipesScreen), 1), i + 1)
                    : string.Empty;
                pending.Add((descriptorType, recipe, nameSuffix));
            }

            if (pending.Count == 0) {
                m_emptyLabel.IsVisible = true;
                return;
            }

            float scale = SelectScale(pending.Count);
            float maxNaturalWidth = MeasureMaxNaturalWidth(pending, m_navigator);

            int columnCount = SelectColumnCount(pending.Count, scale, maxNaturalWidth);
            int rowCount = (pending.Count + columnCount - 1) / columnCount;
            m_gridPanel.ColumnsCount = columnCount;
            m_gridPanel.RowsCount = rowCount;

            for (int i = 0; i < pending.Count; i++) {
                (Type descriptorType, IRecipe recipe, string nameSuffix) = pending[i];
                RecipeDescriptor descriptor = RecipeDescriptorRegistry.CreateDescriptor(descriptorType, m_navigator)!;
                var slot = new RecipaediaOverlayDescriptorSlot(descriptor, recipe, scale, m_descriptorHost) {
                    Margin = new Vector2(4, 4),
                };
                slot.Present(recipe, nameSuffix);
                m_gridPanel.Children.Add(slot);
                m_gridPanel.SetWidgetCell(slot, new Point2(i % columnCount, i / columnCount));
                m_activeSlots.Add(slot);
            }

            m_scrollPanel.ScrollPosition = 0f;
            RefreshActionBars();
        }

        public void RefreshActionBars() {
            foreach (RecipaediaOverlayDescriptorSlot slot in m_activeSlots) slot.RefreshActionBar();
        }

        static float MeasureMaxNaturalWidth(
            List<(Type descriptorType, IRecipe recipe, string nameSuffix)> pending,
            IRecipaediaRecipeNavigator navigator
        ) {
            float maxNaturalWidth = 0f;
            var measuredTypes = new HashSet<Type>();
            foreach ((Type descriptorType, IRecipe recipe, string nameSuffix) in pending) {
                if (!measuredTypes.Add(descriptorType)) continue;
                RecipeDescriptor descriptor = RecipeDescriptorRegistry.CreateDescriptor(descriptorType, navigator)!;
                descriptor.Show(recipe, nameSuffix);
                descriptor.IsVisible = true;
                descriptor.RenderTransform = Matrix.Identity;
                descriptor.Measure(new Vector2(float.PositiveInfinity, float.PositiveInfinity));
                maxNaturalWidth = MathUtils.Max(maxNaturalWidth, descriptor.ParentDesiredSize.X);
            }
            return MathUtils.Min(MathUtils.Max(maxNaturalWidth, 400f), NaturalWidthUpperBound);
        }

        public void ClearDescriptors() {
            foreach (RecipaediaOverlayDescriptorSlot slot in m_activeSlots) {
                slot.Dismiss();
                m_gridPanel.Children.Remove(slot);
            }
            m_activeSlots.Clear();
            m_emptyLabel.IsVisible = false;
            m_scrollPanel.ScrollPosition = 0f;
        }

        static float SelectScale(int recipeCount) {
            if (recipeCount <= 1) return 0.62f;
            if (recipeCount <= 3) return 0.55f;
            if (recipeCount <= 6) return 0.50f;
            return 0.46f;
        }

        static int SelectColumnCount(int recipeCount, float scale, float maxNaturalWidth) {
            if (recipeCount <= 1) return 1;
            float slotWidth = maxNaturalWidth * scale + RecipaediaOverlayDescriptorSlot.CardPadding * 2f + 8f;
            int columns = MathUtils.Max(1, (int)(MaxContentWidth / slotWidth));
            return MathUtils.Min(columns, recipeCount);
        }
    }
}
