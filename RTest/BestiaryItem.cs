using Game;
using RecipaediaEX;

namespace RTest {
    public class BestiaryItem : IRecipaediaItem {
        public BestiaryCreatureInfo m_value;

        public object Value => m_value;
        public string DetailScreenName => "BestiaryDescription";
        public string RecipeScreenName => "BestiaryDescription";

        public BestiaryItem(BestiaryCreatureInfo value) {
            m_value = value;
        }
    }
}