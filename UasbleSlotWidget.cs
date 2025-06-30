using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Engine;
using Game;

namespace RecipaediaEX
{
    public class UsableSlotWidget : CanvasWidget
    {
        public BlockIconWidget m_blockIconWidget;
        public LabelWidget m_labelWidget;
        public InteractableWidget clickableWidget;//可点击界面
        private RectangleWidget highlightedRect;
        private CraftingRecipeSlotWidget linkingSlot;//绑定的格子
        private LabelWidget blockNameLabel;

        public string m_ingredient;

        public int m_resultValue;

        public int m_resultCount;

        /// <summary>
        /// 对应仓库格子序号
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 是否被禁用
        /// </summary>
        public bool IsMouseHover { get => clickableWidget.IsMouseHover; }


        public UsableSlotWidget()
        {
            XElement node = ContentManager.Get<XElement>("Widgets/CraftingRecipeSlot");
            LoadContents(this, node);
            m_blockIconWidget = Children.Find<BlockIconWidget>("CraftingRecipeSlotWidget.Icon");
            m_labelWidget = Children.Find<LabelWidget>("CraftingRecipeSlotWidget.Count");

            clickableWidget = new InteractableWidget();
            Children.Add(clickableWidget);

            highlightedRect = new RectangleWidget { FillColor = new Engine.Color(30, 30, 30, 100), OutlineColor = new Engine.Color(48, 48, 48), OutlineThickness = 1f };
            Children.Add(highlightedRect);

            blockNameLabel = new LabelWidget { VerticalAlignment = WidgetAlignment.Far, HorizontalAlignment = WidgetAlignment.Center, FontScale = 0.5f, Margin = new Engine.Vector2(0, 5) };
            Children.Add(blockNameLabel);
        }

        public void SetIngredient(string ingredient)
        {
            m_ingredient = ingredient;
            m_resultValue = 0;
            m_resultCount = 0;
        }

        public void SetResult(int value, int count)
        {
            m_resultValue = value;
            m_resultCount = count;
            m_ingredient = null;
        }

        public void LinkCraftingSlot(CraftingRecipeSlotWidget slotWidget)
        {
            linkingSlot = slotWidget;
            m_ingredient = slotWidget.m_ingredient;
            SetResult(slotWidget.m_resultValue, slotWidget.m_resultCount);
        }

        public override void MeasureOverride(Vector2 parentAvailableSize)
        {

            m_resultCount = linkingSlot.m_resultCount;
            m_resultValue = linkingSlot.m_resultValue;
            m_ingredient = linkingSlot.m_ingredient;

            int value = 0;
            m_blockIconWidget.IsVisible = false;
            m_labelWidget.IsVisible = false;
            if (!string.IsNullOrEmpty(m_ingredient))
            {
                CraftingRecipesManager.DecodeIngredient(m_ingredient, out string craftingId, out int? data);
                Block[] array = BlocksManager.FindBlocksByCraftingId(craftingId);
                if (array.Length != 0)
                {
                    Block block = array[(int)(1.0 * Time.RealTime) % array.Length];
                    if (block != null)
                    {
                        value = Terrain.MakeBlockValue(block.BlockIndex, 0, data.HasValue ? data.Value : 4);
                        m_blockIconWidget.Value = value;
                        m_blockIconWidget.Light = 15;
                        m_blockIconWidget.IsVisible = true;
                    }
                }
            }
            else if (m_resultValue != 0)
            {
                value = m_resultValue;
                m_blockIconWidget.Value = value;
                m_blockIconWidget.Light = 15;
                m_labelWidget.Text = m_resultCount.ToString();
                m_blockIconWidget.IsVisible = true;
                m_labelWidget.IsVisible = true;
            }

            highlightedRect.IsVisible = IsMouseHover;
            blockNameLabel.IsVisible = IsMouseHover;

            //显示名称
            if (IsMouseHover)
            {
                Block displayBlock = BlocksManager.Blocks[Terrain.ExtractContents(value)];

                if (displayBlock is AirBlock)
                {
                    blockNameLabel.Text = "";
                }
                else
                {
                    try
                    {
                        blockNameLabel.Text = displayBlock.GetDisplayName(null, value);//由于自然子系统传入值是null，所以有些时候会引发空引用报错。有报错就采取最普通的获取名称办法
                    }
                    catch (Exception)
                    {
                        blockNameLabel.Text = GetDisplayName(displayBlock, value);
                    }
                }
            }

            //切换界面
            if (clickableWidget.IsClicked)
            {
                List<Game.CraftingRecipe> recipes = new List<Game.CraftingRecipe>(CraftingRecipesManager.Recipes).FindAll(x => x.ResultValue == Terrain.ExtractContents(value));
                int num = recipes.Count;
                RecipaediaRecipesScreen recipaediaRecipesScreen = ScreensManager.FindScreen<RecipaediaRecipesScreen>("RecipaediaRecipes");
                if (value != 0 && num != 0)
                {
                    recipaediaRecipesScreen.m_craftingRecipes = recipes;
                    recipaediaRecipesScreen.m_recipeIndex = 0;
                    AudioManager.PlaySound("Audio/UI/ButtonClick", 1, 0, 0);
                }
                clickableWidget.IsClicked = false;
            }

            base.MeasureOverride(parentAvailableSize);
        }

        public static string GetDisplayName(Block block, int value)
        {
            int data = Terrain.ExtractData(value);
            string bn = string.Format("{0}:{1}", block.GetType().Name, data);
            if (LanguageControl.TryGetBlock(bn, "DisplayName", out var result))
            {
                return result;
            }
            return "";
        }

        /*
        public InteractableWidget clickableWidget;//可点击界面
        private RectangleWidget highlightedRect;
        private CraftingRecipeSlotWidget linkingSlot;//绑定的格子
        private LabelWidget blockNameLabel;

        /// <summary>
        /// 对应仓库格子序号
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 是否被禁用
        /// </summary>
        public bool IsMouseHover { get => clickableWidget.IsMouseHover; }
        public UsableSlotWidget()
        {
            clickableWidget = new InteractableWidget();
            Children.Add(clickableWidget);

            highlightedRect = new RectangleWidget { FillColor = new Engine.Color(30, 30, 30, 100), OutlineColor = new Engine.Color(48, 48, 48), OutlineThickness = 1f };
            Children.Add(highlightedRect);

            blockNameLabel = new LabelWidget { VerticalAlignment = WidgetAlignment.Far, HorizontalAlignment = WidgetAlignment.Center, FontScale = 0.5f, Margin = new Engine.Vector2(0, 5) };
            Children.Add(blockNameLabel);
        }
        public void LinkCraftingSlot(CraftingRecipeSlotWidget slotWidget)
        {
            linkingSlot = slotWidget;
            m_ingredient = slotWidget.m_ingredient;
            SetResult(slotWidget.m_resultValue, slotWidget.m_resultCount);
        }
        public override void Update()
        {
            base.Update();
            //更新内容物
            if (m_resultCount != linkingSlot.m_resultCount || m_resultValue != linkingSlot.m_resultValue || m_ingredient != linkingSlot.m_ingredient)
            {
                m_resultCount = linkingSlot.m_resultCount;
                m_resultValue = linkingSlot.m_resultValue;
                m_ingredient = linkingSlot.m_ingredient;
                SetResult(linkingSlot.m_resultValue, linkingSlot.m_resultCount);
            }
            highlightedRect.IsVisible = IsMouseHover;
            blockNameLabel.IsVisible = IsMouseHover;

            int value = m_blockIconWidget.Value;

            //显示名称
            if (IsMouseHover)
            {
                Block displayBlock = BlocksManager.Blocks[Terrain.ExtractContents(value)];

                if (displayBlock is AirBlock)
                {
                    blockNameLabel.Text = "";
                }
                else
                {
                    try
                    {
                        blockNameLabel.Text = displayBlock.GetDisplayName(null, value);//由于自然子系统传入值是null，所以有些时候会引发空引用报错。有报错就采取最普通的获取名称办法
                    }
                    catch (Exception)
                    {
                        blockNameLabel.Text = GetDisplayName(displayBlock, value);
                    }
                }
            }
            //切换界面
            if (clickableWidget.IsClicked)
            {
                int num = CraftingRecipesManager.Recipes.Count((CraftingRecipe r) => r.ResultValue == Terrain.ExtractContents(value));
                if (value != 0 && num != 0) ScreensManager.SwitchScreen("RecipaediaRecipes", Terrain.ExtractContents(value));
            }
        }

        public static string GetDisplayName(Block block, int value)
        {
            int data = Terrain.ExtractData(value);
            string bn = string.Format("{0}:{1}", block.GetType().Name, data);
            if (LanguageControl.TryGetBlock(bn, "DisplayName", out var result))
            {
                return result;
            }
            return "";
        }
        */
    }
}
