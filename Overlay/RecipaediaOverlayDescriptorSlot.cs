using Engine;
using Game;
using RecipaediaEX.UI;

namespace RecipaediaEX.Overlay {
    /// <summary>
    /// 悬浮预览专用：包裹 <see cref="RecipeDescriptor"/>，用 RenderTransform 整体缩小绘制，
    /// 布局尺寸按缩放后报告，不改动 Descriptor 内部 Measure/Arrange。
    /// </summary>
    public class RecipaediaOverlayDescriptorSlot : CanvasWidget {
        public const float CardPadding = 4f;
        public const float ActionBarReservedHeight = 28f;

        public RecipeDescriptor Descriptor { get; }
        public IRecipe Recipe { get; }
        public float Scale { get; }

        readonly RecipaediaOverlayDescriptorActionBar m_actionBar;
        Vector2 m_naturalSize;

        public RecipaediaOverlayDescriptorSlot(
            RecipeDescriptor descriptor,
            IRecipe recipe,
            float scale,
            IRecipaediaOverlayDescriptorHost host
        ) {
            Descriptor = descriptor;
            Recipe = recipe;
            Scale = scale;
            HorizontalAlignment = WidgetAlignment.Center;
            VerticalAlignment = WidgetAlignment.Near;

            Children.Add(descriptor);
            descriptor.HorizontalAlignment = WidgetAlignment.Near;
            descriptor.VerticalAlignment = WidgetAlignment.Near;

            m_actionBar = new RecipaediaOverlayDescriptorActionBar(recipe, host);
            Children.Add(m_actionBar);
        }

        public void Present(IRecipe recipe, string nameSuffix) {
            Descriptor.Show(recipe, nameSuffix);
            Descriptor.IsVisible = true;
            Descriptor.ColorTransform = Color.White;
        }

        public void RefreshActionBar() => m_actionBar.Refresh();

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
            DesiredSize = scaled + new Vector2(CardPadding * 2f, CardPadding * 2f + ActionBarReservedHeight);

            Descriptor.Measure(m_naturalSize);
            m_actionBar.Measure(RecipaediaOverlayDescriptorActionBar.BarSize);
        }

        public override void ArrangeOverride() {
            float barX = MathUtils.Max(ActualSize.X - RecipaediaOverlayDescriptorActionBar.BarSize.X - 2f, 0f);
            m_actionBar.Arrange(new Vector2(barX, 2f), RecipaediaOverlayDescriptorActionBar.BarSize);

            Vector2 scaled = m_naturalSize * Scale;
            float offsetX = (ActualSize.X - scaled.X) / 2f;
            Descriptor.Arrange(new Vector2(offsetX, CardPadding + ActionBarReservedHeight), m_naturalSize);
        }
    }
}
