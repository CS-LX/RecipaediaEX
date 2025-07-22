namespace RecipaediaEX.UI {
    /// <summary>
    /// 在方块图鉴中展示的条目。模组可以通过实现这个接口自定义列表条目内容，也就可以突破“只能展示方块”的限制。
    /// <para>【例】原版展示的是方块，条目内部展示的也就是int</para>
    /// </summary>
    public interface IRecipaediaItem {
        /// <summary>
        /// 条目的内容
        /// <para>【例】原版条目展示的方块，这个Value属性就是方块的ID</para>
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// 条目的描述界面的字符串名称，用于打开界面
        /// <para>【例】对于原版，此属性为 RecipaediaDescription</para>
        /// </summary>
        public string DetailScreenName { get; }

        /// <summary>
        /// 条目的配方界面的字符串名称，用于打开界面
        /// <para>【例】对于原版，此属性为 RecipaediaRecipes</para>
        /// </summary>
        public string RecipeScreenName { get; }

        /// <summary>
        /// 查看配方按钮是否可按下
        /// </summary>
        public bool RecipesButtonEnabled { get; }

        /// <summary>
        /// 查看配方按钮显示的文本
        /// </summary>
        public string RecipesButtonText { get; }

        /// <summary>
        /// 查看详细信息按钮是否可按下
        /// </summary>
        public bool DetailsButtonEnabled { get; }

        /// <summary>
        /// 查看详细信息按钮显示的文本
        /// </summary>
        public string DetailsButtonText { get; }
    }
}