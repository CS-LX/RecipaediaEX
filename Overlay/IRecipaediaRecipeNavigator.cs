using System.Collections.Generic;

namespace RecipaediaEX.UI {
    /// <summary>配方页内导航：全屏 Screen 与合成助手预览各自实现。</summary>
    public interface IRecipaediaRecipeNavigator {
        /// <summary>跳转到条目的配方视图（原料格点击、Crafter 按钮等）。</summary>
        void ShowRecipes(IRecipaediaRecipeItem item, IReadOnlyList<IRecipe> recipes, int startIndex = 0);
    }
}
