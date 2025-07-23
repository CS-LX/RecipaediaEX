namespace RecipaediaEX.UI {
    /// <summary>
    /// 用RecipaediaEXRecipesScreen展示的IRecipaediaItem就要接上这个接口
    /// </summary>
    public interface IRecipaediaRecipeItem {
        /// <summary>
        /// 是否是某配方的图鉴项
        /// </summary>
        /// <param name="recipe"></param>
        /// <returns></returns>
        public bool Match(IRecipe recipe);

        /// <summary>
        /// 是否是某配方的某一原料的图鉴项
        /// </summary>
        /// <param name="recipe">配方</param>
        /// <returns></returns>
        public bool IsIngredient(IRecipe recipe);
    }
}