using System;
using Engine;
using Game;
using RecipaediaEX.Implementation;
using RecipaediaEX.UI;

namespace RecipaediaEX.Implementation {
    public class BlockRecipeSlotWidget : RecipeSlotWidget {
        public BlockIconWidget m_iconWidget;
        public LabelWidget m_countLabel;
        public LabelWidget m_blockNameLabel;
        public int m_count;

        public BlockRecipeSlotWidget() : base() {
            m_iconWidget = new BlockIconWidget { Size = new Vector2(64, 64), HorizontalAlignment = WidgetAlignment.Center, VerticalAlignment = WidgetAlignment.Center };
            Children.Add(m_iconWidget);
            m_countLabel = new LabelWidget { HorizontalAlignment = WidgetAlignment.Far, VerticalAlignment = WidgetAlignment.Far, Margin = new Vector2(6, 2) };
            Children.Add(m_countLabel);
            m_blockNameLabel = new LabelWidget { VerticalAlignment = WidgetAlignment.Near, HorizontalAlignment = WidgetAlignment.Center, FontScale = 0.5f, Margin = new Engine.Vector2(0, 5) };
            Children.Add(m_blockNameLabel);
        }

        public override void MeasureOverride(Vector2 parentAvailableSize) {
            base.MeasureOverride(parentAvailableSize);
            m_blockNameLabel.IsVisible = m_interactableWidget.IsMouseHover;
        }

        public override void SetResult(IRecipaediaItem result, RecipaediaEXRecipesScreen belongingScreen, params object[] additionalData) {
            base.SetResult(result, belongingScreen, additionalData);
            m_count = (int)additionalData[0];
        }

        public override void ShowContent(IRecipaediaItem content) {
            base.ShowContent(content);
            if (content is not BlockItem blockItem) return;
            m_iconWidget.Value = blockItem.m_blockValue;
            m_iconWidget.Light = 15;
            m_iconWidget.IsVisible = true;

            m_countLabel.IsVisible = SlotMode == Mode.Result;
            m_countLabel.Text = m_count.ToString();

            if (blockItem.m_block is AirBlock) {
                m_blockNameLabel.Text = "";
            }
            else {
                try {
                    m_blockNameLabel.Text = blockItem.m_block.GetDisplayName(null, blockItem.m_blockValue); //由于自然子系统传入值是null，所以有些时候会引发空引用报错。有报错就采取最普通的获取名称办法
                }
                catch (Exception) {
                    m_blockNameLabel.Text = GetDisplayName(blockItem.m_block, blockItem.m_blockValue);
                }
            }
        }

        public override void HideContent() {
            base.HideContent();
            Clear();
        }

        public override void ClearContents() {
            base.ClearContents();
            Clear();
        }

        public static string GetDisplayName(Block block, int value) {
            int data = Terrain.ExtractData(value);
            string bn = $"{block.GetType().Name}:{data}";
            if (LanguageControl.TryGetBlock(bn, "DisplayName", out var result)) {
                return result;
            }
            return "";
        }

        public void Clear() {
            m_iconWidget.IsVisible = false;
            m_countLabel.Text = string.Empty;
            m_countLabel.IsVisible = false;
            m_blockNameLabel.Text = string.Empty;
            m_blockNameLabel.IsVisible = false;
        }
    }
}