using Game;

namespace RecipaediaEX {
    public class BlockItem : IRecipaediaItem {
        public int m_blockValue;
        public int m_order;
        public Block m_block;

        public object Value => m_blockValue;
        public string DetailScreenName => "RecipaediaDescription";
        public string RecipeScreenName => "RecipaediaRecipes";

        public BlockItem(Block block, int order, int blockValue) {
            m_block = block;
            m_blockValue = blockValue;
            m_order = order;
        }
    }
}