using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Engine;
using Engine.Input;
using Game;
using GameEntitySystem;
using RecipaediaEX.Implementation;
using RecipaediaEX.Search;
using RecipaediaEX.UI;


namespace RecipaediaEX.Overlay {
    public class RecipaediaCraftingOverlayDialog : Dialog, IRecipaediaRecipeNavigator, IRecipaediaOverlayDescriptorHost {
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

        public RecipaediaOverlayCategoryBar m_categoryBar;

        public RecipaediaOverlayRecipePreview m_recipePreview;


        public string m_searchQuery = string.Empty;

        public RecipaediaSearchFilterState m_filterState = new();

        object? m_lastPreviewItem;

        List<RecipaediaCrafterRecipeGroup> m_crafterGroups = [];

        readonly Stack<OverlayPreviewState> m_previewStack = new();

        readonly List<string> m_categoryIds = [];

        string m_selectedCategory = string.Empty;

        string m_listCategory = string.Empty;

        const float SearchDebounceSeconds = 0.25f;

        string m_debounceInputSnapshot = string.Empty;

        float m_searchDebounceRemaining;


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
            m_categoryBar = Children.Find<RecipaediaOverlayCategoryBar>("CategoryBar");
            m_recipePreview = Children.Find<RecipaediaOverlayRecipePreview>("RecipePreview");
            m_recipePreview.SetContext(context);
            m_recipePreview.SetNavigator(this);
            m_recipePreview.SetDescriptorHost(this);

            RecipaediaCategoryCatalog.EnsureLoaded();
            m_categoryIds.AddRange(RecipaediaCategoryCatalog.CategoryIds);
            if (!string.IsNullOrEmpty(RecipaediaCraftingOverlaySessionState.SelectedCategoryId)
                && m_categoryIds.Contains(RecipaediaCraftingOverlaySessionState.SelectedCategoryId)) {
                m_selectedCategory = RecipaediaCraftingOverlaySessionState.SelectedCategoryId;
            }
            else {
                m_selectedCategory = RecipaediaCategoryCatalog.DefaultOverlayCategoryId;
            }
            m_listCategory = m_selectedCategory;

            RestoreSessionSearch();
            m_debounceInputSnapshot = NormalizeInputText(m_inputKey.Text);

            m_blocksList.Direction = LayoutDirection.Vertical;
            m_blocksList.ItemSize = 64;
            m_blocksList.ItemClicked = OnBlocksListItemClicked;
            RecipeDescriptorRegistry.EnsureScanned();
            IsHitTestVisible = false;
            m_overlayRoot.IsHitTestVisible = true;
            PopulateBlocksList(resetScroll: false);
            m_blocksList.ScrollPosition = RecipaediaCraftingOverlaySessionState.BlocksListScrollPosition;
        }


        public void RefreshHostContext() {
            RecipaediaCraftingContext? context = m_host.GetCraftingContext();
            if (context == null) {
                RecipaediaCraftingOverlayController.Dismiss();
                return;
            }
            m_context = context;
            m_recipePreview.SetContext(context);
        }

        public void CaptureSessionState() {
            RecipaediaCraftingOverlaySessionState.SelectedCategoryId = m_selectedCategory;
            RecipaediaCraftingOverlaySessionState.BlocksListScrollPosition = m_blocksList.ScrollPosition;
            RecipaediaCraftingOverlaySessionState.SearchQuery = m_searchQuery;
        }

        void RestoreSessionSearch() {
            string saved = RecipaediaCraftingOverlaySessionState.SearchQuery;
            if (string.IsNullOrEmpty(saved)) return;
            m_searchQuery = saved;
            m_inputKey.Text = saved;
            m_filterState = RecipaediaSearchParser.ParseToFilterState(saved);
        }


        public override void Update() {
            if (!IsVisible) return;
            UpdateSearchBarVisibility();
            UpdateSearchTypeButtonState();
            UpdateSearchInputDebounce();

            UpdateCategoryNavigation();
            if (m_selectedCategory != m_listCategory) {
                m_listCategory = m_selectedCategory;
                ClearSearch();
                PopulateBlocksList(resetScroll: true);
            }
            RefreshCategoryBarCaption();

            if (m_closeButton.IsClicked) {
                RecipaediaCraftingOverlayController.Hide();
                return;
            }
            if (m_recipeDetailClose.IsClicked) HideRecipeDetail();
            if (m_recipeDetailBack.IsClicked) NavigateBack();
            if (m_searchButton.IsClicked) ApplySearchFromInput(commitHistory: true);
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


        void UpdateSearchInputDebounce() {
            string current = NormalizeInputText(m_inputKey.Text);
            if (Keyboard.IsKeyDownOnce(Key.Enter) && m_inputKey.HasFocus) {
                m_searchDebounceRemaining = 0f;
                m_debounceInputSnapshot = current;
                ApplySearchFromInput(commitHistory: true);
                return;
            }
            if (current != m_debounceInputSnapshot) {
                m_debounceInputSnapshot = current;
                m_searchDebounceRemaining = SearchDebounceSeconds;
                return;
            }
            if (m_searchDebounceRemaining <= 0f) return;
            m_searchDebounceRemaining -= Time.FrameDuration;
            if (m_searchDebounceRemaining > 0f) return;
            ApplySearchFromInput(commitHistory: false);
        }

        static string NormalizeInputText(string? text) => text?.Replace("\n", string.Empty) ?? string.Empty;

        void UpdateCategoryNavigation() {
            if (m_categoryIds.Count == 0) return;
            int index = m_categoryIds.IndexOf(m_selectedCategory);
            if (index < 0) {
                m_selectedCategory = m_categoryIds[0];
                index = 0;
            }
            if (m_categoryBar.m_prevButton.IsClicked && index > 0) m_selectedCategory = m_categoryIds[index - 1];
            if (m_categoryBar.m_nextButton.IsClicked && index < m_categoryIds.Count - 1) m_selectedCategory = m_categoryIds[index + 1];
        }

        void RefreshCategoryBarCaption() {
            if (m_categoryIds.Count == 0) {
                m_categoryBar.IsVisible = false;
                return;
            }
            m_categoryBar.IsVisible = true;
            int index = m_categoryIds.IndexOf(m_selectedCategory);
            if (index < 0) index = 0;
            IRecipaediaCategory category = RecipaediaCategoryCatalog.GetCategory(m_selectedCategory);
            m_categoryBar.SetCaption($"{category.DisplayName} ({m_blocksList.Items.Count})", index > 0, index < m_categoryIds.Count - 1);
        }


        void UpdateSearchBarVisibility() {
            m_placeHolder.IsVisible = string.IsNullOrEmpty(m_inputKey.Text);
            m_clearSearchLink.IsVisible = !string.IsNullOrEmpty(m_inputKey.Text) || m_inputKey.HasFocus;
            m_historyButton.ParentWidget.IsVisible = RecipaediaSearchHistory.Entries.Count > 0;
        }


        void UpdateSearchTypeButtonState() {
            m_filterIcon.FillColor = m_filterState.ActiveFilterCount > 0 ? new Color(120, 220, 120, 220) : new Color(255, 255, 255, 180);
        }


        void ApplySearchFromInput(bool commitHistory = true) {
            m_searchQuery = NormalizeInputText(m_inputKey.Text).Trim();
            m_filterState = RecipaediaSearchParser.ParseToFilterState(m_searchQuery);
            if (commitHistory && !string.IsNullOrEmpty(m_searchQuery)) RecipaediaSearchHistory.Add(m_searchQuery);
            m_debounceInputSnapshot = NormalizeInputText(m_inputKey.Text);
            m_searchDebounceRemaining = 0f;
            PopulateBlocksList();
        }


        void ClearSearch() {
            m_searchQuery = string.Empty;
            m_filterState = new RecipaediaSearchFilterState();
            m_inputKey.Text = string.Empty;
            m_debounceInputSnapshot = string.Empty;
            m_searchDebounceRemaining = 0f;
            RecipaediaCraftingOverlaySessionState.SearchQuery = string.Empty;
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
                        m_debounceInputSnapshot = query;
                        m_searchDebounceRemaining = 0f;
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
                        m_debounceInputSnapshot = m_searchQuery;
                        m_searchDebounceRemaining = 0f;
                        if (!string.IsNullOrEmpty(m_searchQuery)) RecipaediaSearchHistory.Add(m_searchQuery);
                        PopulateBlocksList();
                    }
                )
            );
        }


        void PopulateBlocksList(bool resetPreview = true, bool resetScroll = true) {
            if (resetScroll) m_blocksList.ScrollPosition = 0f;
            m_blocksList.ClearItems();
            if (resetPreview) {
                m_lastPreviewItem = null;
                HideRecipeDetail();
            }

            if (m_categoryIds.Count == 0) return;

            IRecipaediaCategory category = RecipaediaCategoryCatalog.GetCategory(m_selectedCategory);
            if (category is IAdvancedCategory advanced) {
                m_blocksList.Direction = advanced.ListDirection;
                m_blocksList.ItemSize = advanced.ListItemSize;
            }
            else {
                m_blocksList.Direction = LayoutDirection.Vertical;
                m_blocksList.ItemSize = 64;
            }
            m_blocksList.ItemWidgetFactory = o => category.ItemWidgetFactory(o as IRecipaediaItem);

            IEnumerable<IRecipaediaItem> items = category.GetItems();
            if (!string.IsNullOrWhiteSpace(m_searchQuery)) {
                List<SearchMatchResult> matches = RecipaediaSearchEngine.Filter(items, m_selectedCategory, m_searchQuery);
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
            UpdateBackButtonVisibility();
        }

        void OnCrafterTabSelected(int index) {
            if (index < 0
                || index >= m_crafterGroups.Count)
                return;
            m_recipePreview.DisplayRecipes(m_crafterGroups[index].Recipes);
        }


        public bool PassesPlacementGate(IRecipe recipe, out string disabledReason) {
            disabledReason = string.Empty;
            if (m_context.Inventory == null) {
                disabledReason = LanguageControl.GetContentWidgets(LanguageName, 5);
                return false;
            }
            int tabIndex = m_crafterTabBar.SelectedIndex;
            if (tabIndex < 0 || tabIndex >= m_crafterGroups.Count) {
                disabledReason = LanguageControl.GetContentWidgets(LanguageName, 2);
                return false;
            }
            RecipaediaCrafterRecipeGroup group = m_crafterGroups[tabIndex];
            string hostCrafterId = GetCrafterId(m_context.CrafterBlockValue);
            if (!string.IsNullOrEmpty(hostCrafterId) && group.CrafterId != hostCrafterId) {
                disabledReason = LanguageControl.GetContentWidgets(LanguageName, 3);
                return false;
            }
            IRecipePlacementTarget? target = m_host.GetPlacementTarget();
            if (target == null) {
                disabledReason = LanguageControl.GetContentWidgets(LanguageName, 4);
                return false;
            }
            if (!PlacableRecipeAdapter.TryAsPlacable(recipe, out _) || !target.CanAccept(recipe)) {
                disabledReason = LanguageControl.GetContentWidgets(LanguageName, 6);
                return false;
            }
            return true;
        }

        public void PlaceRecipe(IRecipe recipe) {
            if (!PassesPlacementGate(recipe, out _)) return;
            IRecipePlacementTarget target = m_host.GetPlacementTarget()!;
            var sources = new PlacementSources {
                PlayerInventory = m_context.Inventory!,
                ContainerInventory = null,
            };
            PlacementResult preview = target.TryPlaceRecipe(recipe, sources, PlacementOptions.Default, execute: false);
            if (preview.Success && !preview.HadTransfers) {
                ShowPlacementFeedback(PlacementResult.AlreadySatisfied());
                return;
            }
            if (!preview.Success && !preview.PartialSuccess) {
                ShowPlacementFeedback(preview);
                return;
            }
            PlacementResult result = target.TryPlaceRecipe(recipe, sources, PlacementOptions.Default, execute: true);
            ShowPlacementFeedback(result);
        }

        public bool IsRecipeBookmarked(IRecipe recipe) => RecipaediaRecipeBookmarks.IsBookmarked(recipe);

        public bool ToggleRecipeBookmark(IRecipe recipe) => RecipaediaRecipeBookmarks.Toggle(recipe);


        void ShowPlacementFeedback(PlacementResult result) {
            ComponentPlayer? player = FindContextPlayer();
            if (player == null) return;

            if (result.Success && !result.HadTransfers) {
                player.ComponentGui.DisplaySmallMessage(
                    LanguageControl.GetContentWidgets(LanguageName, 9),
                    new Color(180, 220, 180, 255),
                    false,
                    false);
                HideRecipeDetail();
                return;
            }
            if (result.Success) {
                player.ComponentGui.DisplaySmallMessage(
                    LanguageControl.GetContentWidgets(LanguageName, 7),
                    Color.White,
                    false,
                    false);
                HideRecipeDetail();
                return;
            }
            string message = result.Missing.Count > 0
                ? string.Join("；", result.Missing)
                : LanguageControl.GetContentWidgets(LanguageName, 8);
            player.ComponentGui.DisplaySmallMessage(message, new Color(255, 220, 120, 255), false, false);
        }


        ComponentPlayer? FindContextPlayer() {
            if (m_context.Project == null || m_context.Inventory == null) return null;
            foreach (ComponentPlayer player in m_context.Project.FindSubsystem<SubsystemPlayers>(true).ComponentPlayers) {
                if (ReferenceEquals(player.ComponentMiner.Inventory, m_context.Inventory)) return player;
            }
            return null;
        }


        static string GetCrafterId(int blockValue) {
            if (blockValue == 0) return string.Empty;
            Block block = BlocksManager.Blocks[Terrain.ExtractContents(blockValue)];
            return block.GetCraftingId(blockValue);
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