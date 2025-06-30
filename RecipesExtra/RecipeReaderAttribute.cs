using System;

namespace RecipaediaEX {
    /// <summary>
    /// 给IRecipeReader的标签，表示这个Reader读取的配方的类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RecipeReaderAttribute : Attribute {
        public Type m_type;
        public Type Type => m_type;

        public RecipeReaderAttribute(Type type) {
            m_type = type;
        }
    }
}