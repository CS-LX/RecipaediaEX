using System;

namespace RecipaediaEX {
    /// <summary>
    /// 给IRecipeReader的标签，表示这个Reader读取的配方的类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RecipeReaderAttribute : Attribute {
        public Type[] m_types;
        public Type[] Types => m_types;

        public RecipeReaderAttribute(Type[] types) {
            m_types = types;
        }
    }
}