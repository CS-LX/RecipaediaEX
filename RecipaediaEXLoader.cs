using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Game;
using RecipaediaEX.UI;

namespace RecipaediaEX
{
    public class RecipaediaEXLoader : ModLoader
    {
        public override void __ModInitialize()
        {
            base.__ModInitialize();
            ModsManager.RegisterHook("OnLoadingFinished", this);
            ModsManager.RegisterHook("BlocksInitalized", this);
        }

        public override void OnLoadingFinished(List<Action> actions)
        {
            base.OnLoadingFinished(actions);
            RecipesLoadManager.Initialize();
            RecipaediaEXManager.Initialize();
            RecipesCrafterManager.Initialize();
            ScreensManager.m_screens["Recipaedia"] = new RecipaediaEXScreen();
            ScreensManager.m_screens["RecipaediaDescription"] = new RecipaediaEXDescriptionScreen();
            ScreensManager.m_screens["RecipaediaRecipes"] = new RecipaediaEXRecipesScreen();
        }

        public override void BlocksInitalized() {
            base.BlocksInitalized();
            RecipesCrafterManager.Initialize();
        }

        public static XElement RequestScreenFile(string screenName) => ContentManager.Get<XElement>($"RecipaediaEX/Screens/{screenName}");
        public static XElement RequestStyleFile(string screenName) => ContentManager.Get<XElement>($"RecipaediaEX/Styles/{screenName}");
        public static XElement RequestWidgetFile(string screenName) => ContentManager.Get<XElement>($"RecipaediaEX/Widgets/{screenName}");
    }
}
