using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Engine;
using Game;
using RecipaediaEX.Implementation;
using RecipaediaEX.Search;
using RecipaediaEX.UI;
using ZLinq;

namespace RecipaediaEX.Overlay {
    public class RecipaediaCraftingOverlayDialog : Dialog {
        public const string LanguageName = "RecipaediaCraftingOverlay";

        public IRecipaediaOverlayHost m_host;
        public RecipaediaCraftingContext m_context;

        public Widget m_overlayRoot;
        public Widget m_recipeDetailPopup;
        public LabelWidget m_recipeDetailTitle;
        public ClickableWidget m_recipeDetailClose;
        public TextBoxWidget m_inputKey;
        public LabelWidget m_placeHolder;
        public LinkWidget m_clearSearchLink;
        public ClickableWidget m_historyButton;
        public ClickableWidget m_searchButton;
        public ClickableWidget m_searchTypeButton;
        public ClickableWidget m_closeButton;
        public RectangleWidget m_filterIcon;
        public ListPanelWidget m_blocksList;
        public RecipaediaOverlayRecipePreview m_recipePreview;

        public string m_searchQuery = string.Empty;
        public RecipaediaSearchFilterState m_filterState = new();
        object? m_lastPreviewItem;
        Func<object, Widget>? m_itemWidgetFactory;

        public RecipaediaCraftingOverlayDialog(IRecipaediaOverlayHost host, RecipaediaCraftingContext context) {
            m_host = host;
            m_context = context;
            XElement node = RecipaediaEXLoader.RequestDialogFile("RecipaediaCraftingOverlayDialog");
            LoadContents(this, node);

            m_overlayRoot = Children.Find<Widget>("OverlayRoot");
            m_recipeDetailPopup = Children.Find<Widget>("RecipeDetailPopup");
            m_recipeDetailTitle = Children.Find<LabelWidget>("RecipeDetailTitle");
            m_recipeDetailClose = Children.Find<ClickableWidget>("RecipeDetailClose");
            m_closeButton = Children.Find<ClickableWidget>("Close");
            m_inputKey = Children.Find<TextBoxWidget>("key");
            m_placeHolder = Children.Find<LabelWidget>("placeholder");
            m_clearSearchLink = Children.Find<LinkWidget>("ClearSearchLink");
            m_historyButton = Children.Find<ClickableWidget>("History");
            m_searchButton = Children.Find<ClickableWidget>("Search");
            m_searchTypeButton = Children.Find<ClickableWidget>("SearchType");
            m_filterIcon = Children.Find<RectangleWidget>("FilterIcon");
            m_blocksList = Children.Find<ListPanelWidget>("BlocksList");
            m_recipePreview = Children.Find<RecipaediaOverlayRecipePreview>("RecipePreview");
            m_recipePreview.SetContext(context);

            var category = new BlocksCategory("All Blocks");
            m_blocksList.Direction = LayoutDirection.Vertical;
            m_blocksList.ItemSize = 64;
            m_itemWidgetFactory = o => category.ItemWidgetFactory(o as IRecipaediaItem);
            m_blocksList.ItemWidgetFactory = m_itemWidgetFactory;
            m_blocksList.ItemClicked = OnBlocksListItemClicked;

            RecipeDescriptorRegistry.EnsureScanned();
            IsHitTestVisible = false;
            m_overlayRoot.IsHitTestVisible = true;

            PopulateBlocksList();
        }

        public override void Update() {
            UpdateSearchBarVisibility();
            UpdateSearchTypeButtonState();

            if (m_closeButton.IsClicked) {
                Dismiss();
                return;
            }

            if (m_recipeDetailClose.IsClicked) HideRecipeDetail();

            if (m_searchButton.IsClicked) ApplySearchFromInput();
            if (m_clearSearchLink.IsClicked) {
                ClearSearch();
                PopulateBlocksList();
            }
            if (m_historyButton.IsClicked) OpenSearchHistoryDialog();
            if (m_searchTypeButton.IsClicked) OpenFilterDialog();

            if (m_blocksList.SelectedItem is IRecipaediaRecipeItem selectedItem
                && !ReferenceEquals(m_lastPreviewItem, selectedItem)) {
                ShowPreviewForItem(selectedItem);
            }
        }

        void Dismiss() => RecipaediaCraftingOverlayController.Close();

        void UpdateSearchBarVisibility() {
            m_placeHolder.IsVisible = string.IsNullOrEmpty(m_inputKey.Text);
            m_clearSearchLink.IsVisible = !string.IsNullOrEmpty(m_inputKey.Text) || m_inputKey.HasFocus;
            m_historyButton.ParentWidget.IsVisible = RecipaediaSearchHistory.Entries.Count > 0;
        }

        void UpdateSearchTypeButtonState() {
            m_filterIcon.FillColor = m_filterState.ActiveFilterCount > 0
                ? new Color(120, 220, 120, 220)
                : new Color(255, 255, 255, 180);
        }

        void ApplySearchFromInput() {
            m_searchQuery = m_inputKey.Text?.Replace("\n", string.Empty).Trim() ?? string.Empty;
            m_filterState = RecipaediaSearchParser.ParseToFilterState(m_searchQuery);
            if (!string.IsNullOrEmpty(m_searchQuery)) RecipaediaSearchHistory.Add(m_searchQuery);
            PopulateBlocksList();
        }

        void ClearSearch() {
            m_searchQuery = string.Empty;
            m_filterState = new RecipaediaSearchFilterState();
            m_inputKey.Text = string.Empty;
            m_lastPreviewItem = null;
            HideRecipeDetail();
        }

        void OpenSearchHistoryDialog() {
            IReadOnlyList<string> entries = RecipaediaSearchHistory.Entries;
            if (entries.Count == 0) return;
            DialogsManager.ShowDialog(
                this,
                new ListSelectionDialog(
                    LanguageControl.GetContentWidgets(RecipaediaEXScreen.SearchLanguageName, 3),
                    entries,
                    48f,
                    item => item?.ToString() ?? string.Empty,
                    item => {
                        string query = item?.ToString() ?? string.Empty;
                        m_inputKey.Text = query;
                        m_searchQuery = query;
                        m_filterState = RecipaediaSearchParser.ParseToFilterState(query);
                        PopulateBlocksList();
                    }
                )
            );
        }

        void OpenFilterDialog() {
            DialogsManager.ShowDialog(
                this,
                new RecipaediaSearchFilterDialog(m_filterState, state => {
                    m_filterState = state;
                    m_searchQuery = RecipaediaSearchParser.BuildQuery(state);
                    m_inputKey.Text = m_searchQuery;
                    if (!string.IsNullOrEmpty(m_searchQuery)) RecipaediaSearchHistory.Add(m_searchQuery);
                    PopulateBlocksList();
                })
            );
        }

        void PopulateBlocksList() {
            m_blocksList.ScrollPosition = 0f;
            m_blocksList.ClearItems();
            m_lastPreviewItem = null;
            HideRecipeDetail();
            IEnumerable<IRecipaediaItem> items = RecipaediaOverlayRecipeResolver.GetAllBlockItems();
            if (!string.IsNullOrWhiteSpace(m_searchQuery)) {
                List<SearchMatchResult> matches = RecipaediaSearchEngine.Filter(items, "All Blocks", m_searchQuery);
                foreach (SearchMatchResult match in matches) m_blocksList.AddItem(match.Item);
            }
            else {
                foreach (IRecipaediaItem item in items) m_blocksList.AddItem(item);
            }
        }

        void OnBlocksListItemClicked(object item) {
            if (item is not IRecipaediaRecipeItem recipeItem) return;
            m_blocksList.SelectedItem = item;
            ShowPreviewForItem(recipeItem);
        }

        void ShowPreviewForItem(IRecipaediaRecipeItem recipeItem) {
            m_lastPreviewItem = recipeItem;
            m_recipeDetailTitle.Text = GetRecipeItemTitle(recipeItem);
            m_recipePreview.ShowItemRecipes(recipeItem);
            m_recipeDetailPopup.IsVisible = true;
        }

        static string GetRecipeItemTitle(IRecipaediaRecipeItem recipeItem) {
            if (recipeItem is IRecipaediaDescriptionItem descriptionItem) return descriptionItem.Name ?? string.Empty;
            if (recipeItem is BlockItem blockItem) return blockItem.m_block.GetDisplayName(null, blockItem.m_blockValue);
            return recipeItem.ToString() ?? string.Empty;
        }

        public void HideRecipeDetail() {
            m_recipeDetailPopup.IsVisible = false;
            m_recipePreview.ClearDescriptors();
        }
    }
}
