using System.Xml.Linq;
using Engine;
using Game;
using ZLinq;

namespace RecipaediaEX.UI {
    /// <summary>
    /// 配方内容格子界面
    /// </summary>
    public class RecipeSlotWidget : CanvasWidget {
        public InteractableWidget m_interactableWidget;//可点击界面
        public RectangleWidget m_highlightedRect;
        public RecipaediaEXRecipesScreen m_belongingScreen;

        /// <summary>
        /// 此格子的模式
        /// </summary>
        public Mode SlotMode { get; protected set; }

        /// <summary>
        /// 展示的内容(可多选，例如原版存在一些合成ID相同的方块)
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

        public RecipeSlotWidget() {
            LoadContents(this, RecipaediaEXLoader.RequestWidgetFile("CraftingRecipeSlot"));
            m_interactableWidget = new InteractableWidget();
            Children.Add(m_interactableWidget);
            m_highlightedRect = new RectangleWidget { FillColor = new Engine.Color(30, 30, 30, 100), OutlineColor = new Engine.Color(48, 48, 48), OutlineThickness = 1f };
            Children.Add(m_highlightedRect);
        }

        public override void Update() {
            base.Update();
            if (Index < 0) return;
            if (m_interactableWidget.IsClicked) {
                OnClicked();
            }

            if (m_interactableWidget.IsSpecialClicked) {
                OnSpecialClicked();
            }
        }

        public override void MeasureOverride(Vector2 parentAvailableSize) {
            base.MeasureOverride(parentAvailableSize);
            m_highlightedRect.IsVisible = m_interactableWidget.IsMouseHover;

            //没有内容则隐藏内容
            if (RecipaediaItems.Length == 0) {
                Index = -1;
            }
            else {
                //展示内容
                if (SlotMode == Mode.Ingredient) {
                    Index = (int)(1.0 * Time.RealTime) % RecipaediaItems.Length;
                }
                else if (SlotMode == Mode.Result) {
                    Index = 0;
                }
            }
        }

        /// <summary>
        /// 设置这个格子展示的内容(以原料的模式展示)
        /// </summary>
        /// <param name="ingredients">展示的原料</param>
        /// <param name="belongingScreen">所属的Screen</param>
        /// <param name="additionalData">其余参数</param>
        public virtual void SetIngredients(IRecipaediaItem[] ingredients, RecipaediaEXRecipesScreen belongingScreen, params object[] additionalData) {
            RecipaediaItems = ingredients;
            SlotMode = Mode.Ingredient;
            m_belongingScreen = belongingScreen;
        }

        /// <summary>
        /// 设置这个格子的展示内容(以产物的模式展示)
        /// </summary>
        /// <param name="result"></param>
        /// <param name="belongingScreen">所属的Screen</param>
        /// <param name="additionalData"></param>
        public virtual void SetResult(IRecipaediaItem result, RecipaediaEXRecipesScreen belongingScreen, params object[] additionalData) {
            RecipaediaItems = [result];
            SlotMode = Mode.Result;
            m_belongingScreen = belongingScreen;
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
                if (SlotMode == Mode.Ingredient) {
                    var newItems = RecipaediaEXManager.Recipes.AsValueEnumerable().Where(x => recipeItem.Match(x));
                    if (newItems.Count() > 0) {
                        AudioManager.PlaySound("Audio/UI/ButtonClick", 1, 0, 0);
                        m_belongingScreen.SwitchToNewRecipe(newItems.ToList(), 0);
                    }
                }
            }
        }

        /// <summary>
        /// 被长按/右键时执行
        /// </summary>
        public virtual void OnSpecialClicked() {
            IRecipaediaItem recipaediaItem = RecipaediaItems[Index];
            if (recipaediaItem is IRecipaediaRecipeItem recipeItem2) {
                var newItems = RecipaediaEXManager.Recipes.AsValueEnumerable().Where(x => recipeItem2.IsIngredient(x));
                if (newItems.Count() > 0) {
                    AudioManager.PlaySound("Audio/UI/ButtonClick", 1, 0, 0);
                    m_belongingScreen.SwitchToNewRecipe(newItems.ToList(), 0);
                }
            }
        }

        /// <summary>
        /// 配方内容格子将自己的内容以何种方式展示?
        /// </summary>
        public enum Mode {

            /// <summary>
            /// 原料模式:
            /// <list type="bullet">
            ///     <item><description>点击格子可以跳转到它的合成配方</description></item>
            ///     <item><description>右键/长按格子可以跳转到需要它的配方</description></item>
            ///     <item><description>若格子存在多个内容，则会循环展示</description></item>
            /// </list>
            /// </summary>
            Ingredient,

            /// <summary>
            /// 产物模式
            /// <list type="bullet">
            ///     <item><description>只能右键/长按格子可以跳转到需要它的配方</description></item>
            ///     <item><description>若格子存在多个内容，只会展示第一个内容</description></item>
            /// </list>
            /// </summary>
            Result
        }
    }
}