using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using Engine.Serialization;
using Game;
using ZLinq;

namespace RecipaediaEX {
    public class RecipaediaEXScreen : Screen {
        //功能
        public List<Assembly> m_scannedAssemblies = [];
        public List<IRecipaediaCategoryProvider> m_categoryProviders = [];//所有分类配置器提供器
        public List<IRecipaediaCategory> m_categories = [];//所有分类配置器

        //类别

        //界面
        public LabelWidget m_categoryLabel;
        public ButtonWidget m_prevCategoryButton;
        public ButtonWidget m_nextCategoryButton;
        public ButtonWidget m_detailsButton;
        public ButtonWidget m_recipesButton;
        public ListPanelWidget m_blocksList;
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
        }

        public override void Enter(object[] parameters) {
            base.Enter(parameters);
            if (ScreensManager.PreviousScreen != ScreensManager.FindScreen<Screen>("RecipaediaRecipes") && ScreensManager.PreviousScreen != ScreensManager.FindScreen<Screen>("RecipaediaDescription")) {
                m_previousScreen = ScreensManager.PreviousScreen;
            }
        }

        public override void Update() {
            base.Update();
            if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked) {
                ScreensManager.SwitchScreen(m_previousScreen);
            }
        }
    }
}