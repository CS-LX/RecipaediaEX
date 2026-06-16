using Engine;
using Game;
using RecipaediaEX.Implementation;
using System.Collections.Generic;
using ZLinq;
using ZLinq.Linq;

namespace RecipaediaEX.UI {
    public class CrafterButtonWidget : CanvasWidget {
        public IRecipaediaRecipeNavigator m_navigator;
        public AdvancedBevelledButtonWidget m_bevelledButtonWidget;

        /// <summary>
        /// 展示的Crafter(作为IRecipaediaItem展示)
        /// </summary>
        public IRecipaediaItem[] RecipaediaItems { get; set; } = [];

        /// <summary>
        /// 正在展示的内容的序号
        /// </summary>
        public int Index {
            get;
            set {
                if (value < 0) {
                    HideContent();
                    field = -1;
                }
                else {
                    if (field != value && field > 0)
                        HideContent();
                    field = value;
                    ShowContent(RecipaediaItems[value]);
                }
            }
        } = -1;

        public CrafterButtonWidget() {
            m_bevelledButtonWidget = new AdvancedBevelledButtonWidget();
            Children.Add(m_bevelledButtonWidget);
        }

        public override void Update() {
            base.Update();
            if (Index < 0) return;
            if (m_bevelledButtonWidget.IsClicked) {
                OnClicked();
            }

            if (m_bevelledButtonWidget.IsSpecialClicked) {
                OnSpecialClicked();
            }
        }

        public override void MeasureOverride(Vector2 parentAvailableSize) {
            base.MeasureOverride(parentAvailableSize);

            //没有内容则隐藏内容
            if (RecipaediaItems.Length == 0) {
                Index = -1;
            }
            else {
                //展示内容
                Index = (int)(1.0 * Time.RealTime) % RecipaediaItems.Length;
            }
        }

        /// <summary>
        /// 设置这个Crafter按钮的展示内容
        /// </summary>
        /// <param name="crafterItems"></param>
        /// <param name="belongingScreen">所属的Screen</param>
        /// <param name="additionalData"></param>
        public virtual void SetCrafters(IRecipaediaItem[] crafterItems, IRecipaediaRecipeNavigator navigator, params object[] additionalData) {
            RecipaediaItems = crafterItems;
            m_navigator = navigator;
        }

        /// <summary>
        /// 清除内容
        /// </summary>
        public virtual void ClearContents() {
            RecipaediaItems = [];
            Index = -1;
        }

        /// <summary>
        /// 展示内容的逻辑
        /// </summary>
        /// <param name="content"></param>
        public virtual void ShowContent(IRecipaediaItem content) {

        }

        /// <summary>
        /// 隐藏内容的逻辑
        /// </summary>
        public virtual void HideContent() {

        }

        /// <summary>
        /// 被点击时
        /// </summary>
        public virtual void OnClicked() {
            IRecipaediaItem recipaediaItem = RecipaediaItems[Index];
            if (recipaediaItem is IRecipaediaRecipeItem recipeItem) {
                ValueEnumerable<ListWhere<IRecipe>, IRecipe> newItems = RecipaediaEXManager.Recipes.AsValueEnumerable().Where(x => recipeItem.Match(x));
                if (newItems.Count() > 0) {
                    AudioManager.PlaySound("Audio/UI/ButtonClick", 1, 0, 0);
                    m_navigator.ShowRecipes(recipeItem, newItems.ToList(), 0);
                }
            }
        }

        /// <summary>
        /// 被长按/右键时执行
        /// </summary>
        public virtual void OnSpecialClicked() {
            IRecipaediaItem recipaediaItem = RecipaediaItems[Index];
            if (recipaediaItem is IRecipaediaRecipeItem recipeItem2) {
                ValueEnumerable<ListWhere<IRecipe>, IRecipe> newItems = RecipaediaEXManager.Recipes.AsValueEnumerable().Where(x => recipeItem2.IsIngredient(x));
                if (newItems.Count() > 0) {
                    AudioManager.PlaySound("Audio/UI/ButtonClick", 1, 0, 0);
                    m_navigator.ShowRecipes(recipeItem2, newItems.ToList(), 0);
                }
            }
        }

        public static CrafterButtonWidget GetDefaultWidget(IRecipe recipe, IRecipaediaRecipeNavigator navigator) {
            if (RecipesCrafterManager.Crafters.TryGetValue(recipe, out List<int> crafterIDs) && crafterIDs.Count > 0) {
                BlockCrafterButtonWidget blockCrafterButtonWidget = new();
                blockCrafterButtonWidget.SetCrafters(crafterIDs.AsValueEnumerable().Select(x => new BlockItem(BlocksManager.Blocks[Terrain.ExtractContents(x)], 0, x)).OfType<IRecipaediaItem>().ToArray(), navigator);
                return blockCrafterButtonWidget;
            }
            return null;
        }
    }
}