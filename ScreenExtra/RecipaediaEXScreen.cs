using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Engine;
using Engine.Serialization;
using Game;
using RecipaediaEX.Search;
using ZLinq;

namespace RecipaediaEX.UI {
    public class RecipaediaEXScreen : Screen {
        public const string SearchLanguageName = "RecipaediaSearch";

        public List<string> m_categoriesName = [];
        public string m_selectedCategory;
        public string m_listCategory = string.Empty;
        public Func<object, Widget> m_currentItemWidgetFactory;
        public bool m_categoriesInitialized;
        public string m_searchQuery = string.Empty;
        public RecipaediaSearchFilterState m_filterState = new();

        public LabelWidget m_categoryLabel;
        public ButtonWidget m_prevCategoryButton;
        public ButtonWidget m_nextCategoryButton;
        public ButtonWidget m_detailsButton;
        public ButtonWidget m_recipesButton;
        public ListPanelWidget m_blocksList;
        public Screen m_previousScreen;

        /// <summary>从子屏返回图鉴列表时不应覆盖 <see cref="m_previousScreen"/> 的 Screen 注册名。</summary>
        static readonly HashSet<string> s_childScreenNames = new(StringComparer.Ordinal) {
            "RecipaediaDescription",
            "RecipaediaRecipes",
        };

        public static void RegisterChildScreen(string screenName) => s_childScreenNames.Add(screenName);

        static bool IsChildScreen(Screen? screen) {
            if (screen == null) return false;
            foreach (KeyValuePair<string, Screen> entry in ScreensManager.m_screens) {
                if (entry.Value == screen && s_childScreenNames.Contains(entry.Key)) return true;
            }
            return false;
        }

        public TextBoxWidget m_inputKey;
        public LabelWidget m_placeHolder;
        public LinkWidget m_clearSearchLink;
        public ButtonWidget m_historyButton;
        public ButtonWidget m_searchButton;
        public ButtonWidget m_searchTypeButton;

        public RecipaediaEXScreen() {
            XElement node = RecipaediaEXLoader.RequestScreenFile("RecipaediaEXScreen");
            LoadContents(this, node);
            m_categoryLabel = Children.Find<LabelWidget>("Category");
            m_prevCategoryButton = Children.Find<ButtonWidget>("PreviousCategory");
            m_nextCategoryButton = Children.Find<ButtonWidget>("NextCategory");
            m_detailsButton = Children.Find<ButtonWidget>("DetailsButton");
            m_recipesButton = Children.Find<ButtonWidget>("RecipesButton");
            m_blocksList = Children.Find<ListPanelWidget>("BlocksList");
            m_blocksList.ItemClicked = OnBlocksListItemClicked;
            m_inputKey = Children.Find<TextBoxWidget>("key");
            m_placeHolder = Children.Find<LabelWidget>("placeholder");
            m_clearSearchLink = Children.Find<LinkWidget>("ClearSearchLink");
            m_historyButton = Children.Find<ButtonWidget>("History");
            m_searchButton = Children.Find<ButtonWidget>("Search");
            m_searchTypeButton = Children.Find<ButtonWidget>("SearchType");
        }

        public override void Enter(object[] parameters) {
            base.Enter(parameters);
            if (!IsChildScreen(ScreensManager.PreviousScreen)) {
                m_previousScreen = ScreensManager.PreviousScreen;
            }
            if (!m_categoriesInitialized || m_categoriesName.Count == 0) {
                RecipaediaCategoryCatalog.EnsureLoaded();
                m_categoriesName.Clear();
                m_categoriesName.AddRange(RecipaediaCategoryCatalog.CategoryIds);
                m_categoriesInitialized = true;
            }
            m_selectedCategory = m_categoriesName.Contains(m_selectedCategory) ? m_selectedCategory : m_categoriesName[0];
        }

        public override void Update() {
            base.Update();

            if (m_selectedCategory != m_listCategory) {
                m_listCategory = m_selectedCategory;
                ClearSearch();
                PopulateBlocksList();
            }

            UpdateSearchBarVisibility();
            UpdateSearchTypeButtonState();

            string arg = RecipaediaCategoryCatalog.GetCategory(m_selectedCategory).DisplayName;
            m_categoryLabel.Text = $"{arg} ({m_blocksList.Items.Count})";

            m_prevCategoryButton.IsEnabled = m_selectedCategory != m_categoriesName[0];
            m_nextCategoryButton.IsEnabled = m_selectedCategory != m_categoriesName[^1];
            if (m_prevCategoryButton.IsClicked || Input.Left) {
                m_selectedCategory = m_categoriesName[MathUtils.Max(m_categoriesName.IndexOf(m_selectedCategory) - 1, 0)];
            }
            if (m_nextCategoryButton.IsClicked || Input.Right) {
                m_selectedCategory = m_categoriesName[MathUtils.Min(m_categoriesName.IndexOf(m_selectedCategory) + 1, m_categoriesName.Count - 1)];
            }

            if (m_searchButton.IsClicked) {
                ApplySearchFromInput();
            }
            if (m_clearSearchLink.IsClicked) {
                ClearSearch();
                PopulateBlocksList();
            }
            if (m_historyButton.IsClicked) {
                OpenSearchHistoryDialog();
            }
            if (m_searchTypeButton.IsClicked) {
                OpenFilterDialog();
            }

            IRecipaediaItem selectedItem = m_blocksList.SelectedItem as IRecipaediaItem;
            if (m_blocksList.SelectedItem is IRecipaediaItem item) {
                selectedItem = item;
            }
            if (selectedItem != null) {
                m_recipesButton.IsEnabled = selectedItem.RecipesButtonEnabled;
                m_recipesButton.Text = selectedItem.RecipesButtonText;
            }
            else {
                m_recipesButton.Text = LanguageControl.Get(nameof(RecipaediaScreen), 3);
                m_recipesButton.IsEnabled = false;
            }
            if (selectedItem != null && m_recipesButton.IsClicked) {
                ScreensManager.SwitchScreen(selectedItem.RecipeScreenName, selectedItem);
            }
            if (selectedItem != null) {
                m_detailsButton.IsEnabled = selectedItem.DetailsButtonEnabled;
                m_detailsButton.Text = selectedItem.DetailsButtonText;
            }
            else {
                m_detailsButton.IsEnabled = false;
                m_detailsButton.Text = LanguageControl.Get("ContentWidgets", nameof(RecipaediaScreen), "1");
            }
            if (selectedItem != null && m_detailsButton.IsClicked) {
                ScreensManager.SwitchScreen(selectedItem.DetailScreenName, selectedItem, m_blocksList.Items.AsValueEnumerable().Cast<IRecipaediaItem>().ToList());
            }

            if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked) {
                if (!string.IsNullOrEmpty(m_searchQuery) || !string.IsNullOrEmpty(m_inputKey.Text)) {
                    ClearSearch();
                    PopulateBlocksList();
                }
                else {
                    m_categoriesName.Clear();
                    m_blocksList.ClearItems();
                    m_listCategory = string.Empty;
                    ScreensManager.SwitchScreen(m_previousScreen);
                }
            }
        }

        void UpdateSearchBarVisibility() {
            m_placeHolder.IsVisible = string.IsNullOrEmpty(m_inputKey.Text);
            m_clearSearchLink.IsVisible = !string.IsNullOrEmpty(m_inputKey.Text) || m_inputKey.HasFocus;
            m_historyButton.IsVisible = RecipaediaSearchHistory.Entries.Count > 0;
        }

        void UpdateSearchTypeButtonState() {
            m_searchTypeButton.Text = string.Empty;
            m_searchTypeButton.IsChecked = m_filterState.ActiveFilterCount > 0;
        }

        void ApplySearchFromInput() {
            m_searchQuery = m_inputKey.Text?.Replace("\n", string.Empty).Trim() ?? string.Empty;
            m_filterState = RecipaediaSearchParser.ParseToFilterState(m_searchQuery);
            if (!string.IsNullOrEmpty(m_searchQuery)) RecipaediaSearchHistory.Add(m_searchQuery);
            PopulateBlocksList();
        }

        void OpenSearchHistoryDialog() {
            IReadOnlyList<string> entries = RecipaediaSearchHistory.Entries;
            if (entries.Count == 0) return;
            DialogsManager.ShowDialog(
                this,
                new ListSelectionDialog(
                    LanguageControl.GetContentWidgets(SearchLanguageName, 3),
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

        void ClearSearch() {
            m_searchQuery = string.Empty;
            m_filterState = new RecipaediaSearchFilterState();
            m_inputKey.Text = string.Empty;
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

        public void PopulateBlocksList() {
            m_blocksList.ScrollPosition = 0f;
            m_blocksList.ClearItems();

            IRecipaediaCategory selectedCategory = RecipaediaCategoryCatalog.GetCategory(m_selectedCategory);
            m_blocksList.Direction = selectedCategory is IAdvancedCategory adv ? adv.ListDirection : LayoutDirection.Vertical;
            m_blocksList.ItemSize = selectedCategory is IAdvancedCategory adv2 ? adv2.ListItemSize : 70;
            Widget CurrentFunc(object o) => selectedCategory.ItemWidgetFactory(o as IRecipaediaItem);
            m_blocksList.ItemWidgetFactory = CurrentFunc;

            IEnumerable<IRecipaediaItem> items = selectedCategory.GetItems();
            if (!string.IsNullOrWhiteSpace(m_searchQuery)) {
                List<SearchMatchResult> matches = RecipaediaSearchEngine.Filter(items, m_selectedCategory, m_searchQuery);
                foreach (SearchMatchResult match in matches) {
                    m_blocksList.AddItem(match.Item);
                }
            }
            else {
                foreach (IRecipaediaItem item in items) {
                    m_blocksList.AddItem(item);
                }
            }
        }

        public void OnBlocksListItemClicked(object item) {
            if (m_blocksList.SelectedItem == item && item is IRecipaediaItem selectedItem) {
                ScreensManager.SwitchScreen(selectedItem.DetailScreenName, selectedItem, m_blocksList.Items.AsValueEnumerable().Cast<IRecipaediaItem>().ToList());
            }
        }
    }
}
