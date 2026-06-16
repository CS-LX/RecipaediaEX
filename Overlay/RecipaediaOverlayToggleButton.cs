using System.Xml.Linq;
using Engine;
using Game;
using RecipaediaEX.Overlay;

namespace RecipaediaEX.UI {
    /// <summary>合成 Modal 角标：打开/关闭合成助手。</summary>
    public class RecipaediaOverlayToggleButton : BevelledButtonWidget {
        readonly IRecipaediaOverlayHost m_host;

        public RecipaediaOverlayToggleButton(IRecipaediaOverlayHost host) {
            m_host = host;
            Style = ContentManager.Get<XElement>("RecipaediaEX/Styles/ButtonStyle_Search");
            Size = new Vector2(48, 48);
            HorizontalAlignment = WidgetAlignment.Far;
            VerticalAlignment = WidgetAlignment.Near;
            Margin = new Vector2(8, 8);
        }

        public override void Update() {
            base.Update();
            if (IsClicked && m_host.GetCraftingContext() != null) {
                RecipaediaCraftingOverlayController.Toggle(m_host);
            }
        }
    }
}
