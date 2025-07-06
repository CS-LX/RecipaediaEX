using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Game;
using ZLinq;

namespace RecipaediaEX {
    public class BlocksCategory : IRecipaediaCategory {
        public string m_id;
        public string m_displayName;
        public List<BlockItem> m_blocks;

        public string Id => m_id;
        public string DisplayName => m_displayName;
        public IEnumerable<IRecipaediaItem> GetItems() => m_blocks;
        public Func<IRecipaediaItem, Widget> ItemWidgetFactory => CreateItemWidget;

        public BlocksCategory(string category) {
            m_id = category;
            m_displayName = LanguageControl.Get("BlocksManager", category);
            m_blocks = new List<BlockItem>();
            foreach (Block item in BlocksManager.Blocks) {
                foreach (int creativeValue in item.GetCreativeValues()) {
                    if (category == "All Blocks" || item.GetCategory(creativeValue) == category)
                        m_blocks.Add(new BlockItem(item, item.GetDisplayOrder(creativeValue), creativeValue));
                }
            }
            m_blocks = m_blocks.AsValueEnumerable().OrderBy(o => o.m_order).ToList();
        }

        Widget CreateItemWidget(IRecipaediaItem item) {
            if (item is not BlockItem blockItem) return null;
            int value = (int)blockItem.Value;
            int num = Terrain.ExtractContents(value);
            Block block = BlocksManager.Blocks[num];
            XElement node2 = ContentManager.Get<XElement>("Widgets/RecipaediaItem");
            var obj = (ContainerWidget)Widget.LoadWidget(this, node2, null);
            obj.Children.Find<BlockIconWidget>("RecipaediaItem.Icon").Value = value;
            obj.Children.Find<LabelWidget>("RecipaediaItem.Text").Text = block.GetDisplayName(null, value);
            string description = block.GetDescription(value);
            description = description.Replace("\n", "  ");
            obj.Children.Find<LabelWidget>("RecipaediaItem.Details").Text = description;
            return obj;
        }
    }
}