using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Engine;
using Engine.Serialization;
using Game;
using ZLinq;
using ZLinq.Linq;

namespace RecipaediaEX {
    public class RecipaediaEXScreen : Screen {
        //功能
        public List<Assembly> m_scannedAssemblies = [];
        public Dictionary<Type, IRecipaediaCategoryProvider> m_categoryProviderCache = [];//所有分类配置器提供器
        public List<string> m_categoriesName = [];//所有分类配置器的名称
        public Dictionary<string, IRecipaediaCategory> m_categories = [];//所有分类配置器（易失）
        public string m_selectedCategory;
        public string m_listCategory = string.Empty;
        public Func<object, Widget> m_currentItemWidgetFactory;

        //界面
        public LabelWidget m_categoryLabel;
        public ButtonWidget m_prevCategoryButton;
        public ButtonWidget m_nextCategoryButton;
        public ButtonWidget m_detailsButton;
        public ButtonWidget m_recipesButton;
        public ListPanelWidget m_blocksList;//（内容易失）
        public Screen m_previousScreen;


        public RecipaediaEXScreen() {
            XElement node = ContentManager.Get<XElement>("Screens/RecipaediaEXScreen");
            LoadContents(this, node);
            m_categoryLabel = Children.Find<LabelWidget>("Category");
            m_prevCategoryButton = Children.Find<ButtonWidget>("PreviousCategory");
            m_nextCategoryButton = Children.Find<ButtonWidget>("NextCategory");
            m_detailsButton = Children.Find<ButtonWidget>("DetailsButton");
            m_recipesButton = Children.Find<ButtonWidget>("RecipesButton");
            m_blocksList = Children.Find<ListPanelWidget>("BlocksList");
            m_blocksList.ItemClicked = OnBlocksListItemClicked;

            GetProviders();//获取类别提供器
        }

        public override void Enter(object[] parameters) {
            base.Enter(parameters);
            if (ScreensManager.PreviousScreen != ScreensManager.FindScreen<Screen>("RecipaediaRecipes") && ScreensManager.PreviousScreen != ScreensManager.FindScreen<Screen>("RecipaediaDescription")) {
                m_previousScreen = ScreensManager.PreviousScreen;
            }
            GetCategories();
            m_selectedCategory = m_categoriesName.Contains(m_selectedCategory) ? m_selectedCategory : m_categoriesName[0];
        }

        public override void Update() {
            base.Update();
            var recipes = RecipaediaEXManager.Recipes.AsValueEnumerable();
            //类别有改变，刷新列表
            if (m_selectedCategory != m_listCategory) {
                m_listCategory = m_selectedCategory;
                PopulateBlocksList();
            }

            //列表最上方的名称
            string arg = m_categories[m_selectedCategory].DisplayName;
            m_categoryLabel.Text = $"{arg} ({m_blocksList.Items.Count})";

            //切换类别按钮逻辑
            m_prevCategoryButton.IsEnabled = m_selectedCategory != m_categoriesName[0];
            m_nextCategoryButton.IsEnabled = m_selectedCategory != m_categoriesName[^1];
            if (m_prevCategoryButton.IsClicked || Input.Left) {
                m_selectedCategory = m_categoriesName[MathUtils.Max(m_categoriesName.IndexOf(m_selectedCategory) - 1, 0)];
            }
            if (m_nextCategoryButton.IsClicked || Input.Right) {
                m_selectedCategory = m_categoriesName[MathUtils.Min(m_categoriesName.IndexOf(m_selectedCategory) + 1, m_categoriesName.Count - 1)];
            }

            //选中后
            IRecipaediaItem selectedItem = null;
            int recipesCount = 0;
            if (m_blocksList.SelectedItem is IRecipaediaItem item) {
                selectedItem = item;
                recipesCount = recipes.Count(x => x.Match(item));
            }
            //配方按钮逻辑
            if (recipesCount > 0) {
                m_recipesButton.Text = $"{recipesCount} {((recipesCount == 1) ? LanguageControl.Get(nameof(RecipaediaScreen), 1) : LanguageControl.Get(nameof(RecipaediaScreen), 2))}";
                m_recipesButton.IsEnabled = true;
            }
            else {
                m_recipesButton.Text = LanguageControl.Get(nameof(RecipaediaScreen), 3);
                m_recipesButton.IsEnabled = false;
            }
            if (selectedItem != null && m_recipesButton.IsClicked) {
                ScreensManager.SwitchScreen(selectedItem.RecipeScreenName, selectedItem);
            }
            //描述按钮逻辑
            m_detailsButton.IsEnabled = selectedItem != null;
            if (selectedItem != null && m_detailsButton.IsClicked) {
                ScreensManager.SwitchScreen(selectedItem.DetailScreenName, selectedItem, m_blocksList.Items.Cast<IRecipaediaItem>().ToList());
            }

            //退出逻辑
            if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked) {
                m_categories.Clear();
                m_categoriesName.Clear();
                m_blocksList.ClearItems();
                m_listCategory = string.Empty;
                ScreensManager.SwitchScreen(m_previousScreen);
            }
        }

        public void GetProviders() {//获取类别提供器
            foreach (Assembly item in TypeCache.LoadedAssemblies.AsValueEnumerable().Where(a => !TypeCache.IsKnownSystemAssembly(a))) {
                if (!m_scannedAssemblies.Contains(item)) {
                    foreach (TypeInfo definedType in item.DefinedTypes) {
                        Type type = definedType.AsType();
                        if (typeof(IRecipaediaCategoryProvider).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface) {
                            // 确保类型没有被实例化过
                            if (!m_categoryProviderCache.ContainsKey(type)) {
                                var instance = (IRecipaediaCategoryProvider)Activator.CreateInstance(type);
                                m_categoryProviderCache.Add(type, instance);
                            }
                        }
                    }
                    m_scannedAssemblies.Add(item);
                }
            }
        }

        public void GetCategories() {//获取类别
            foreach (var provider in m_categoryProviderCache.Values) {
                foreach (var category in provider.GetCategories()) {
                    m_categories[category.Id] = category;
                    m_categoriesName.Add(category.Id);
                }
            }
            m_categoriesName = m_categoriesName.AsValueEnumerable().Distinct().ToList();
        }

        public void PopulateBlocksList() {
            m_blocksList.ScrollPosition = 0f;
            m_blocksList.ClearItems();

            IRecipaediaCategory selectedCategory = m_categories[m_selectedCategory];
            m_blocksList.Direction = selectedCategory is IAdvancedCategory adv ? adv.ListDirection : LayoutDirection.Vertical;
            m_blocksList.ItemSize = selectedCategory is IAdvancedCategory adv2 ? adv2.ListItemSize : 70;
            Widget CurrentFunc(object o) => selectedCategory.ItemWidgetFactory(o as IRecipaediaItem);
            m_blocksList.ItemWidgetFactory = CurrentFunc;

            foreach (var item in selectedCategory.GetItems()) {
                m_blocksList.AddItem(item);
            }
        }

        public void OnBlocksListItemClicked(object item) {//实现条目双击跳转详情页逻辑
            if (m_blocksList.SelectedItem == item && item is IRecipaediaItem selectedItem) {
                ScreensManager.SwitchScreen(selectedItem.DetailScreenName, selectedItem, m_blocksList.Items.Cast<IRecipaediaItem>().ToList());
            }
        }
    }
}