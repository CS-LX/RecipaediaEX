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
        bool Match(IRecipe recipe);

        /// <summary>
        /// 是否是某配方的某一原料的图鉴项
        /// </summary>
        /// <param name="recipe">配方</param>
        /// <returns></returns>
        bool IsIngredient(IRecipe recipe);

        /// <summary>
        /// 是否是某配方的主产物(Result)的图鉴项，即“只在产物方向”的匹配。
        /// <para>默认与 <see cref="Match"/> 同义，以兼容旧实现；</para>
        /// <para><c>BlockItem</c> 会把它收敛为仅主产物，从而与副产物(Remains)方向的匹配区分开——
        /// <see cref="Match"/> 在特定条件下会额外纳入副产物配方，而本方法始终只认主产物。</para>
        /// </summary>
        /// <param name="recipe">配方</param>
        /// <returns></returns>
        bool MatchesAsResult(IRecipe recipe) => Match(recipe);
    }
}