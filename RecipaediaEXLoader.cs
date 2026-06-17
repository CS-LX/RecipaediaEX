using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Engine;
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
            RecipesCrafterManager.Initialize();
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
            if (newWidget is CraftingTableWidget vanilla and not RecipaediaCraftingTableWidget) {
                // 原位替换，避免再次走 ModalPanelWidget setter 触发第二次入场动画。
                var replacement = new RecipaediaCraftingTableWidget(
                    gui.m_componentPlayer.ComponentMiner.Inventory,
                    vanilla.m_componentCraftingTable);
                replacement.HorizontalAlignment = WidgetAlignment.Center;
                gui.m_modalPanelContainerWidget.Children.Remove(vanilla);
                gui.m_modalPanelContainerWidget.Children.Insert(0, replacement);
                if (gui.m_modalPanelAnimationData != null) {
                    gui.m_modalPanelAnimationData.NewWidget = replacement;
                }
            }
        }

        public static XElement RequestScreenFile(string screenName) => ContentManager.Get<XElement>($"RecipaediaEX/Screens/{screenName}");
        public static XElement RequestStyleFile(string screenName) => ContentManager.Get<XElement>($"RecipaediaEX/Styles/{screenName}");
        public static XElement RequestWidgetFile(string screenName) => ContentManager.Get<XElement>($"RecipaediaEX/Widgets/{screenName}");
        public static XElement RequestDialogFile(string dialogName) => ContentManager.Get<XElement>($"RecipaediaEX/Dialogs/{dialogName}");
    }
}
