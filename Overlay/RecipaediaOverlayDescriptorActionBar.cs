using Engine;
using Engine.Graphics;
using Game;

namespace RecipaediaEX.Overlay {
    /// <summary>单条配方预览卡片旁的 + / ★ 操作条（Atlas 扁平图标）。</summary>
    public class RecipaediaOverlayDescriptorActionBar : StackPanelWidget {
        public static readonly Vector2 BarSize = new(60f, 28f);

        const float IconSize = 20f;
        const float HitSize = 28f;
        const float ButtonGap = 4f;

        static readonly Subtexture s_plusIcon = ContentManager.Get<Subtexture>("Textures/Atlas/Plus");
        static readonly Subtexture s_starIcon = CreateRatingStarIcon();

        readonly IRecipe m_recipe;
        readonly IRecipaediaOverlayDescriptorHost m_host;
        readonly RectangleWidget m_bookmarkIcon;
        readonly RectangleWidget m_placeIcon;
        readonly ClickableWidget m_bookmarkButton;
        readonly ClickableWidget m_placeButton;
        bool m_gateOpen;

        public RecipaediaOverlayDescriptorActionBar(IRecipe recipe, IRecipaediaOverlayDescriptorHost host) {
            m_recipe = recipe;
            m_host = host;
            Direction = LayoutDirection.Horizontal;
            IsHitTestVisible = true;

            (CanvasWidget bookmarkHost, m_bookmarkIcon, m_bookmarkButton) = CreateIconButton(s_starIcon);
            bookmarkHost.Margin = new Vector2(0, 0);
            Children.Add(bookmarkHost);

            (CanvasWidget placeHost, m_placeIcon, m_placeButton) = CreateIconButton(s_plusIcon);
            placeHost.Margin = new Vector2(ButtonGap, 0);
            Children.Add(placeHost);

            Refresh();
        }

        static Subtexture CreateRatingStarIcon() {
            Texture2D texture = ContentManager.Get<Texture2D>("Textures/Gui/RatingStar");
            return new Subtexture(texture, Vector2.Zero, new Vector2(0.2f, 1f));
        }

        (CanvasWidget Host, RectangleWidget Icon, ClickableWidget Clickable) CreateIconButton(Subtexture icon) {
            var host = new CanvasWidget {
                Size = new Vector2(HitSize, HitSize),
            };
            var iconWidget = new RectangleWidget {
                Size = new Vector2(IconSize, IconSize),
                Subtexture = icon,
                FillColor = new Color(255, 255, 255, 200),
                OutlineColor = Color.Transparent,
                HorizontalAlignment = WidgetAlignment.Center,
                VerticalAlignment = WidgetAlignment.Center,
                BlendState = BlendState.NonPremultiplied,
                TextureLinearFilter = true,
            };
            var clickable = new ClickableWidget {
                HorizontalAlignment = WidgetAlignment.Stretch,
                VerticalAlignment = WidgetAlignment.Stretch,
                SoundName = "Audio/UI/ButtonClick",
            };
            host.Children.Add(iconWidget);
            host.Children.Add(clickable);
            return (host, iconWidget, clickable);
        }

        public override void MeasureOverride(Vector2 parentAvailableSize) {
            base.MeasureOverride(BarSize);
            DesiredSize = BarSize;
        }

        public void Refresh() {
            RefreshBookmarkVisual();
            RefreshGateState();
        }

        public override void Update() {
            if (m_bookmarkButton.IsClicked) {
                m_host.ToggleRecipeBookmark(m_recipe);
                RefreshBookmarkVisual();
            }
            if (m_placeButton.IsClicked && m_gateOpen) m_host.PlaceRecipe(m_recipe);
        }

        void RefreshGateState() {
            m_gateOpen = m_host.PassesPlacementGate(m_recipe, out _);
            m_placeButton.IsHitTestVisible = m_gateOpen;
            m_placeIcon.FillColor = m_gateOpen
                ? new Color(140, 230, 140, 255)
                : new Color(96, 96, 96, 160);
            m_placeButton.ColorTransform = m_gateOpen ? Color.White : new Color(128, 128, 128, 160);
        }

        void RefreshBookmarkVisual() {
            bool bookmarked = m_host.IsRecipeBookmarked(m_recipe);
            m_bookmarkIcon.FillColor = bookmarked
                ? new Color(255, 220, 80, 255)
                : new Color(255, 255, 255, 160);
        }
    }
}
