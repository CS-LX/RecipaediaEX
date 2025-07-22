using System;

namespace RecipaediaEX.UI {
    /// <summary>
    /// 用在Widget上，表示一个配方的呈现界面
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RecipeDescriptorAttribute : Attribute {
        public Type[] RecipeTypes;
        public int Order;

        /// <summary>
        /// 创建一个RecipeDescriptorAttribute实例
        /// </summary>
        /// <param name="recipeTypes">这个呈现界面呈现的配方的类型(可多选)</param>
        /// <param name="order">此配方呈现界面的优先级。遇到一个配方有重复的呈现界面时，优先级高的覆盖优先级低的；若优先级相同，则类名字典序后的覆盖字典序前的</param>
        public RecipeDescriptorAttribute(Type[] recipeTypes, int order = 0) {
            RecipeTypes = recipeTypes;
            Order = order;
        }
    }
}