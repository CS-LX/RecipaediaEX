using Engine;
using Game;

namespace RecipaediaEX.Overlay {
    /// <summary>合成助手右侧条带：紧凑分类切换（Atlas 箭头 + 名称）。</summary>
    public class RecipaediaOverlayCategoryBar : CanvasWidget {
        public ClickableWidget m_prevButton;
        public ClickableWidget m_nextButton;
        public LabelWidget m_label;

        readonly CanvasWidget m_prevHost;
        readonly CanvasWidget m_nextHost;

        public RecipaediaOverlayCategoryBar() {
            var row = new StackPanelWidget {
                Direction = LayoutDirection.Horizontal,
                HorizontalAlignment = WidgetAlignment.Stretch,
                VerticalAlignment = WidgetAlignment.Center,
            };
            Children.Add(row);

            m_prevHost = CreateArrowHost("Textures/Atlas/ArrowLeft", out m_prevButton);
            m_nextHost = CreateArrowHost("Textures/Atlas/ArrowRight", out m_nextButton);
            m_label = new LabelWidget {
                Color = new Color(220, 220, 220, 255),
                HorizontalAlignment = WidgetAlignment.Center,
                VerticalAlignment = WidgetAlignment.Center,
                FontScale = 0.85f,
                WordWrap = false,
            };
            var labelHost = new CanvasWidget {
                Size = new Vector2(float.PositiveInfinity, 28),
                HorizontalAlignment = WidgetAlignment.Stretch,
                VerticalAlignment = WidgetAlignment.Center,
            };
            labelHost.Children.Add(m_label);

            row.Children.Add(m_prevHost);
            row.Children.Add(labelHost);
            row.Children.Add(m_nextHost);
        }

        static CanvasWidget CreateArrowHost(string subtexturePath, out ClickableWidget clickable) {
            var host = new CanvasWidget {
                Size = new Vector2(28, 28),
                VerticalAlignment = WidgetAlignment.Center,
                Margin = new Vector2(2, 0),
            };
            host.Children.Add(new RectangleWidget {
                Size = new Vector2(20, 20),
                Subtexture = ContentManager.Get<Subtexture>(subtexturePath),
                FillColor = new Color(255, 255, 255, 200),
                OutlineColor = Color.Transparent,
                HorizontalAlignment = WidgetAlignment.Center,
                VerticalAlignment = WidgetAlignment.Center,
                TextureLinearFilter = true,
            });
            clickable = new ClickableWidget {
                HorizontalAlignment = WidgetAlignment.Stretch,
                VerticalAlignment = WidgetAlignment.Stretch,
                SoundName = "Audio/UI/ButtonClick",
            };
            host.Children.Add(clickable);
            return host;
        }

        public void SetCaption(string text, bool canGoPrev, bool canGoNext) {
            m_label.Text = text;
            m_prevButton.IsHitTestVisible = canGoPrev;
            m_nextButton.IsHitTestVisible = canGoNext;
            m_prevHost.ColorTransform = canGoPrev ? Color.White : new Color(255, 255, 255, 80);
            m_nextHost.ColorTransform = canGoNext ? Color.White : new Color(255, 255, 255, 80);
        }
    }
}
