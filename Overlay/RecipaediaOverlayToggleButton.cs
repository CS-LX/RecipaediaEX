using Engine;
using Engine.Graphics;
using Game;

namespace RecipaediaEX.Overlay {
    /// <summary>合成 Modal 角标：扁平图标，打开/关闭合成助手。</summary>
    public class RecipaediaOverlayToggleButton : CanvasWidget {
        readonly IRecipaediaOverlayHost m_host;
        readonly ClickableWidget m_clickable;

        public RecipaediaOverlayToggleButton(IRecipaediaOverlayHost host) {
            m_host = host;
            Size = new Vector2(36, 36);
            HorizontalAlignment = WidgetAlignment.Far;
            VerticalAlignment = WidgetAlignment.Near;
            Margin = new Vector2(8, 8);

            Children.Add(new RectangleWidget {
                Size = new Vector2(20, 20),
                Subtexture = ContentManager.Get<Subtexture>("RecipaediaEX/Textures/CraftingOverlayToggle"),
                FillColor = new Color(255, 255, 255, 180),
                OutlineColor = Color.Transparent,
                HorizontalAlignment = WidgetAlignment.Center,
                VerticalAlignment = WidgetAlignment.Center,
                BlendState = BlendState.NonPremultiplied,
                TextureLinearFilter = true,
            });
            m_clickable = new ClickableWidget {
                HorizontalAlignment = WidgetAlignment.Stretch,
                VerticalAlignment = WidgetAlignment.Stretch,
                SoundName = "Audio/UI/ButtonClick",
            };
            Children.Add(m_clickable);
        }

        public override void Update() {
            IsVisible = m_host.GetCraftingContext() != null;
            if (!IsVisible) return;
            if (m_clickable.IsClicked) RecipaediaCraftingOverlayController.Toggle(m_host);
        }
    }
}
