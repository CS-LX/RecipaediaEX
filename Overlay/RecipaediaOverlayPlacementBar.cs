using Engine;
using Game;

namespace RecipaediaEX.Overlay {
    /// <summary>二级配方弹窗底部：+ 填充按钮与状态提示。</summary>
    public class RecipaediaOverlayPlacementBar : CanvasWidget {
        public ClickableWidget m_placeButton;
        public LabelWidget m_placeLabel;
        public LabelWidget m_statusLabel;

        readonly CanvasWidget m_buttonHost;

        public RecipaediaOverlayPlacementBar() {
            Size = new Vector2(float.PositiveInfinity, 44);
            HorizontalAlignment = WidgetAlignment.Stretch;

            m_statusLabel = new LabelWidget {
                Color = new Color(180, 180, 180, 255),
                VerticalAlignment = WidgetAlignment.Center,
                HorizontalAlignment = WidgetAlignment.Near,
                FontScale = 0.85f,
                WordWrap = true,
                Margin = new Vector2(4, 0),
            };
            Children.Add(m_statusLabel);

            m_buttonHost = new CanvasWidget {
                Size = new Vector2(72, 36),
                HorizontalAlignment = WidgetAlignment.Far,
                VerticalAlignment = WidgetAlignment.Center,
                Margin = new Vector2(4, 0),
            };
            m_buttonHost.Children.Add(new RectangleWidget {
                OutlineColor = Color.Transparent,
                FillColor = new Color(40, 120, 40, 180),
                HorizontalAlignment = WidgetAlignment.Stretch,
                VerticalAlignment = WidgetAlignment.Stretch,
                Size = new Vector2(float.PositiveInfinity, float.PositiveInfinity),
            });
            m_placeLabel = new LabelWidget {
                Text = "+",
                Color = new Color(255, 255, 255, 255),
                HorizontalAlignment = WidgetAlignment.Center,
                VerticalAlignment = WidgetAlignment.Center,
                FontScale = 1.2f,
            };
            m_buttonHost.Children.Add(m_placeLabel);
            m_placeButton = new ClickableWidget {
                HorizontalAlignment = WidgetAlignment.Stretch,
                VerticalAlignment = WidgetAlignment.Stretch,
                SoundName = "Audio/UI/ButtonClick",
            };
            m_buttonHost.Children.Add(m_placeButton);
            Children.Add(m_buttonHost);
        }

        public void SetEnabled(bool enabled, string statusText) {
            m_placeButton.IsHitTestVisible = enabled;
            m_placeLabel.Color = enabled ? new Color(255, 255, 255, 255) : new Color(255, 255, 255, 96);
            m_buttonHost.ColorTransform = enabled ? Color.White : new Color(255, 255, 255, 140);
            m_statusLabel.Text = statusText;
        }
    }
}
