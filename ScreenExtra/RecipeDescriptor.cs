using System;
using Game;

namespace RecipaediaEX.UI {
    public abstract class RecipeDescriptor : CanvasWidget {
        public RecipaediaEXRecipesScreen m_belongingScreen;

        protected RecipeDescriptor(RecipaediaEXRecipesScreen belongingScreen) {
            m_belongingScreen = belongingScreen;
        }

        /// <summary>
        /// 当此配方呈现界面被展示时调用
        /// </summary>
        /// <param name="recipe">展示的配方</param>
        /// <param name="nameSuffix">名称后缀，一般是"# (配方序号)"</param>
        public abstract void Show(IRecipe recipe, string nameSuffix);

        /// <summary>
        /// 当此配方呈现界面被隐藏时调用
        /// </summary>
        public abstract void Hide();

        /// <summary>
        /// 查看Crafter的按钮的类型
        /// </summary>
        /// <returns></returns>
        public abstract CrafterButtonWidget GetCrafterButton(IRecipe recipe);
    }
}