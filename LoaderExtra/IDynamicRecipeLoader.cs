using GameEntitySystem;

namespace RecipaediaEX {
    /// <summary>
    /// 动态配方读取逻辑的封装，对标原版的AdHoc配方
    /// 用于动态性强，需要依赖输入实时生成输出的配方
    /// </summary>
    public interface IDynamicRecipeLoader {
        /// <summary>
        /// RecipaediaEXManager首次加载时调用
        /// </summary>
        void Initialize();

        /// <summary>
        /// 获取临时配方
        /// </summary>
        IRecipe GetDynamicRecipe(IRecipe actual, Project project);

        /// <summary>
        /// 此读取器的优先级
        /// <para>对于目标mod包名一致的读取器，优先级高的会覆盖优先级低的；若优先级相等，则被RecipaediaEX后发现的读取器会覆盖先前发现的那个</para>
        /// </summary>
        int Order { get; }
    }
}