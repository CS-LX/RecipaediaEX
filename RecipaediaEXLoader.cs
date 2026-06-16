using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Game;
using RecipaediaEX.Overlay;
using RecipaediaEX.Search;
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
            ModsManager.RegisterHook("CraftingRecipesManagerInitialized", this);
            ModsManager.RegisterHook("GuiUpdate", this);
            ModsManager.RegisterHook("OnModalPanelWidgetSet", this);
        }

        public override void OnLoadingFinished(List<Action> actions)
        {
            base.OnLoadingFinished(actions);
            RecipesLoadManager.Initialize();
            RecipaediaEXManager.Initialize();
            RecipesCrafterManager.Initialize();
            RecipaediaSearchIndex.Initialize();
            ScreensManager.m_screens["Recipaedia"] = new RecipaediaEXScreen();
            ScreensManager.m_screens["RecipaediaDescription"] = new RecipaediaEXDescriptionScreen();
            ScreensManager.m_screens["RecipaediaRecipes"] = new RecipaediaEXRecipesScreen();
        }
        public override void CraftingRecipesManagerInitialized() {
            RecipaediaEXManager.ResetRecipes();
        }

        public override void BlocksInitalized() {
            RecipesCrafterManager.Initialize();
        }

        public override void GuiUpdate(ComponentGui componentGui) {
            base.GuiUpdate(componentGui);
            RecipaediaOverlayInput.HandleRecipaediaKey(componentGui);
        }

        public override void OnModalPanelWidgetSet(ComponentGui gui, Widget oldWidget, Widget newWidget) {
            base.OnModalPanelWidgetSet(gui, oldWidget, newWidget);
            if (newWidget is CraftingTableWidget vanilla && vanilla is not RecipaediaCraftingTableWidget) {
                gui.ModalPanelWidget = new RecipaediaCraftingTableWidget(
                    gui.m_componentPlayer.ComponentMiner.Inventory,
                    vanilla.m_componentCraftingTable);
            }
        }

        public static XElement RequestScreenFile(string screenName) => ContentManager.Get<XElement>($"RecipaediaEX/Screens/{screenName}");
        public static XElement RequestStyleFile(string screenName) => ContentManager.Get<XElement>($"RecipaediaEX/Styles/{screenName}");
        public static XElement RequestWidgetFile(string screenName) => ContentManager.Get<XElement>($"RecipaediaEX/Widgets/{screenName}");
        public static XElement RequestDialogFile(string dialogName) => ContentManager.Get<XElement>($"RecipaediaEX/Dialogs/{dialogName}");
    }
}
