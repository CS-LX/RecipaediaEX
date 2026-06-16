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
    public class RecipaediaCraftingOverlayDialog : Dialog, IRecipaediaRecipeNavigator {
        public const string LanguageName = "RecipaediaCraftingOverlay";


        readonly struct OverlayPreviewState {
            public readonly IRecipaediaRecipeItem Item;

            public readonly int TabIndex;


            public OverlayPreviewState(IRecipaediaRecipeItem item, int tabIndex) {
                Item = item;
                TabIndex = tabIndex;
            }
        }


        public IRecipaediaOverlayHost m_host;

        public RecipaediaCraftingContext m_context;


        public Widget m_overlayRoot;

        public Widget m_recipeDetailPopup;

        public Widget m_recipeDetailBackHost;

        public LabelWidget m_recipeDetailTitle;

        public ClickableWidget m_recipeDetailBack;

        public ClickableWidget m_recipeDetailClose;

        public RecipaediaOverlayCrafterTabBar m_crafterTabBar;

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

        List<RecipaediaCrafterRecipeGroup> m_crafterGroups = [];

        readonly Stack<OverlayPreviewState> m_previewStack = new();


        public RecipaediaCraftingOverlayDialog(IRecipaediaOverlayHost host, RecipaediaCraftingContext context) {
            m_host = host;
            m_context = context;
            XElement node = RecipaediaEXLoader.RequestDialogFile("RecipaediaCraftingOverlayDialog");
            LoadContents(this, node);
            m_overlayRoot = Children.Find<Widget>("OverlayRoot");
            m_recipeDetailPopup = Children.Find<Widget>("RecipeDetailPopup");
            m_recipeDetailBackHost = Children.Find<Widget>("RecipeDetailBackHost");
            m_recipeDetailTitle = Children.Find<LabelWidget>("RecipeDetailTitle");
            m_recipeDetailBack = Children.Find<ClickableWidget>("RecipeDetailBack");
            m_recipeDetailClose = Children.Find<ClickableWidget>("RecipeDetailClose");
            m_crafterTabBar = Children.Find<RecipaediaOverlayCrafterTabBar>("CrafterTabBar");
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
            m_recipePreview.SetNavigator(this);
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
            if (m_recipeDetailBack.IsClicked) NavigateBack();
            if (m_searchButton.IsClicked) ApplySearchFromInput();
            if (m_clearSearchLink.IsClicked) {
                ClearSearch();
                PopulateBlocksList();
            }
            if (m_historyButton.IsClicked) OpenSearchHistoryDialog();
            if (m_searchTypeButton.IsClicked) OpenFilterDialog();
        }

        public void ShowRecipes(IRecipaediaRecipeItem item, IReadOnlyList<IRecipe> recipes, int startIndex = 0) {
            if (recipes.Count == 0) return;
            PushCurrentPreviewState();
            ShowPreviewForItem(item);
        }


        void Dismiss() => RecipaediaCraftingOverlayController.Close();


        void UpdateSearchBarVisibility() {
            m_placeHolder.IsVisible = string.IsNullOrEmpty(m_inputKey.Text);
            m_clearSearchLink.IsVisible = !string.IsNullOrEmpty(m_inputKey.Text) || m_inputKey.HasFocus;
            m_historyButton.ParentWidget.IsVisible = RecipaediaSearchHistory.Entries.Count > 0;
        }


        void UpdateSearchTypeButtonState() {
            m_filterIcon.FillColor = m_filterState.ActiveFilterCount > 0 ? new Color(120, 220, 120, 220) : new Color(255, 255, 255, 180);
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
                new RecipaediaSearchFilterDialog(
                    m_filterState,
                    state => {
                        m_filterState = state;
                        m_searchQuery = RecipaediaSearchParser.BuildQuery(state);
                        m_inputKey.Text = m_searchQuery;
                        if (!string.IsNullOrEmpty(m_searchQuery)) RecipaediaSearchHistory.Add(m_searchQuery);
                        PopulateBlocksList();
                    }
                )
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
            m_previewStack.Clear();
            ShowPreviewForItem(recipeItem);
        }


        void ShowPreviewForItem(IRecipaediaRecipeItem recipeItem, int? tabIndex = null) {
            m_lastPreviewItem = recipeItem;
            m_recipeDetailTitle.Text = GetRecipeItemTitle(recipeItem);
            List<IRecipe> allRecipes = RecipaediaOverlayRecipeResolver.ResolveAllRecipes(recipeItem, m_context);
            m_crafterGroups = RecipaediaOverlayRecipeResolver.BuildCrafterGroups(allRecipes, m_context);
            int selectedIndex = tabIndex ?? RecipaediaOverlayRecipeResolver.SelectDefaultGroupIndex(m_crafterGroups, m_context);
            m_crafterTabBar.SetGroups(m_crafterGroups, selectedIndex, OnCrafterTabSelected);
            if (selectedIndex >= 0)
                m_recipePreview.DisplayRecipes(m_crafterGroups[selectedIndex].Recipes);
            else
                m_recipePreview.DisplayRecipes([]);
            m_recipeDetailPopup.IsVisible = true;
            SyncBlocksListSelection(recipeItem);
            UpdateBackButtonVisibility();
        }

        static bool SameRecipeItem(IRecipaediaRecipeItem a, IRecipaediaRecipeItem b) {
            if (a is BlockItem blockA
                && b is BlockItem blockB)
                return blockA.m_blockValue == blockB.m_blockValue;
            return ReferenceEquals(a, b);
        }

        void SyncBlocksListSelection(IRecipaediaRecipeItem recipeItem) {
            foreach (object item in m_blocksList.Items) {
                if (item is IRecipaediaRecipeItem listItem
                    && SameRecipeItem(listItem, recipeItem)) {
                    m_blocksList.SelectedItem = item;
                    return;
                }
            }
            m_blocksList.SelectedIndex = null;
        }

        void OnCrafterTabSelected(int index) {
            if (index < 0
                || index >= m_crafterGroups.Count)
                return;
            m_recipePreview.DisplayRecipes(m_crafterGroups[index].Recipes);
        }


        void PushCurrentPreviewState() {
            if (m_lastPreviewItem is not IRecipaediaRecipeItem current
                || !m_recipeDetailPopup.IsVisible)
                return;
            m_previewStack.Push(new OverlayPreviewState(current, m_crafterTabBar.SelectedIndex));
        }


        void NavigateBack() {
            if (m_previewStack.Count == 0) return;
            OverlayPreviewState state = m_previewStack.Pop();
            ShowPreviewForItem(state.Item, state.TabIndex);
            AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
        }


        void UpdateBackButtonVisibility() => m_recipeDetailBackHost.IsVisible = m_previewStack.Count > 0;


        static string GetRecipeItemTitle(IRecipaediaRecipeItem recipeItem) {
            if (recipeItem is IRecipaediaDescriptionItem descriptionItem) return descriptionItem.Name ?? string.Empty;
            if (recipeItem is BlockItem blockItem) return blockItem.m_block.GetDisplayName(null, blockItem.m_blockValue);
            return recipeItem.ToString() ?? string.Empty;
        }


        public void HideRecipeDetail() {
            m_recipeDetailPopup.IsVisible = false;
            m_previewStack.Clear();
            m_crafterTabBar.ClearTabs();
            m_crafterGroups.Clear();
            m_recipePreview.ClearDescriptors();
            UpdateBackButtonVisibility();
        }
    }
}