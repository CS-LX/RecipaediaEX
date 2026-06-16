using System.Collections.Generic;

namespace RecipaediaEX.UI {
    /// <summary>配方页内导航：全屏 Screen 与合成助手预览各自实现。</summary>
    public interface IRecipaediaRecipeNavigator {
        /// <summary>展示一组配方（原料格点击、Crafter 切换等）。</summary>
        void ShowRecipes(IReadOnlyList<IRecipe> recipes, int startIndex = 0);
    }
}
