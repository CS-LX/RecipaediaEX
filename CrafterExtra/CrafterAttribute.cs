using System;

namespace RecipaediaEX {
    /// <summary>
    /// <para>给ICrafter的标签，表示这个Crafter适配的配方的类型</para>
    /// <para>仅在界面展示中有用！具体Crafter逻辑还得自己实现</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CrafterAttribute : Attribute {
        public Type[] m_types;
        public Type[] Types => m_types;

        public CrafterAttribute(Type[] types) {
            m_types = types;
        }
    }
}