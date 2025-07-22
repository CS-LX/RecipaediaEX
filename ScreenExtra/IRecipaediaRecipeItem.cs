namespace RecipaediaEX.UI {
    /// <summary>
    /// 用RecipaediaEXRecipesScreen展示的IRecipaediaItem就要接上这个接口
    /// </summary>
    public interface IRecipaediaRecipeItem {
        /// <summary>
        /// 查看配方按钮是否可按下
        /// </summary>
        public bool RecipesButtonEnabled { get; }
        /// <summary>
        /// 查看配方按钮显示的文本
        /// </summary>
        public string RecipesButtonText { get; }
        /// <summary>
        /// 是否是某配方的图鉴项
        /// </summary>
        /// <param name="recipe"></param>
        /// <returns></returns>
        public bool Match(IRecipe recipe);
    }
}