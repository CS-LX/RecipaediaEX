using Engine;
using Game;
using RecipaediaEX.UI;

namespace RecipaediaEX.Overlay {
    /// <summary>
    /// 悬浮预览专用：包裹 <see cref="RecipeDescriptor"/>，用 RenderTransform 整体缩小绘制，
    /// 布局尺寸按缩放后报告，不改动 Descriptor 内部 Measure/Arrange。
    /// </summary>
    public class RecipaediaOverlayDescriptorSlot : CanvasWidget {
        public const float CardPadding = 6f;

        public RecipeDescriptor Descriptor { get; }
        public float Scale { get; }

        readonly RectangleWidget m_card;
        Vector2 m_naturalSize;

        public RecipaediaOverlayDescriptorSlot(RecipeDescriptor descriptor, float scale) {
            Descriptor = descriptor;
            Scale = scale;
            HorizontalAlignment = WidgetAlignment.Center;
            VerticalAlignment = WidgetAlignment.Near;

            m_card = new RectangleWidget {
                OutlineColor = new Color(255, 255, 255, 36),
                FillColor = new Color(0, 0, 0, 40),
                HorizontalAlignment = WidgetAlignment.Stretch,
                VerticalAlignment = WidgetAlignment.Stretch,
                Size = new Vector2(float.PositiveInfinity, float.PositiveInfinity),
            };
            Children.Add(m_card);
            Children.Add(descriptor);
            descriptor.HorizontalAlignment = WidgetAlignment.Near;
            descriptor.VerticalAlignment = WidgetAlignment.Near;
        }

        public void Present(IRecipe recipe, string nameSuffix) {
            Descriptor.Show(recipe, nameSuffix);
            Descriptor.IsVisible = true;
            Descriptor.ColorTransform = Color.White;
        }

        public void Dismiss() {
            Descriptor.Hide();
            Descriptor.IsVisible = false;
            Descriptor.RenderTransform = Matrix.Identity;
        }

        public Vector2 MeasureNaturalSize() {
            Descriptor.RenderTransform = Matrix.Identity;
            Descriptor.Measure(new Vector2(float.PositiveInfinity, float.PositiveInfinity));
            return Descriptor.ParentDesiredSize;
        }

        public override void MeasureOverride(Vector2 parentAvailableSize) {
            m_naturalSize = MeasureNaturalSize();
            Descriptor.RenderTransform = Matrix.CreateScale(Scale);
            Vector2 scaled = m_naturalSize * Scale;
            DesiredSize = scaled + new Vector2(CardPadding * 2f, CardPadding * 2f);
        }

        public override void ArrangeOverride() {
            m_card.Arrange(Vector2.Zero, ActualSize);
            Vector2 scaled = m_naturalSize * Scale;
            float offsetX = (ActualSize.X - scaled.X) / 2f;
            Descriptor.Arrange(new Vector2(offsetX, CardPadding), m_naturalSize);
        }
    }
}
