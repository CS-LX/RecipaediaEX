using Engine;
using Engine.Graphics;
using Game;
using RecipaediaEX.UI;

namespace RecipaediaEX.Overlay {
    /// <summary>单条配方预览卡片旁的 + / ★ 操作条（Atlas 扁平图标）。</summary>
    public class RecipaediaOverlayDescriptorActionBar : StackPanelWidget {
        public static readonly Vector2 BarSize = new(60f, 28f);

        const float IconSize = 20f;
        const float HitSize = 28f;
        const float ButtonGap = 4f;
        const float TooltipFontScale = 0.58f;

        static readonly Subtexture s_plusIcon = ContentManager.Get<Subtexture>("Textures/Atlas/Plus");
        static readonly Subtexture s_starIcon = ContentManager.Get<Subtexture>("Textures/Gui/RatingStar");

        readonly IRecipe m_recipe;
        readonly IRecipaediaOverlayDescriptorHost m_host;
        readonly RectangleWidget m_bookmarkIcon;
        readonly RectangleWidget m_placeIcon;
        readonly ClickableWidget m_bookmarkButton;
        readonly InteractableWidget m_placeButton;
        readonly LabelWidget m_placeTooltip;
        readonly CanvasWidget m_placeHost;
        readonly PlacementLongPressRepeater m_placeRepeater = new();
        bool m_gateOpen;
        bool m_firstRepeatClearGrid = true;
        bool m_repeatExhausted;
        bool m_longPressGesture;
        double m_pressStartTime;
        string m_disabledReason = string.Empty;

        public RecipaediaOverlayDescriptorActionBar(IRecipe recipe, IRecipaediaOverlayDescriptorHost host) {
            m_recipe = recipe;
            m_host = host;
            Direction = LayoutDirection.Horizontal;
            IsHitTestVisible = true;

            (CanvasWidget bookmarkHost, m_bookmarkIcon, m_bookmarkButton) = CreateIconButton(s_starIcon);
            bookmarkHost.Margin = new Vector2(0, 0);
            Children.Add(bookmarkHost);

            (m_placeHost, m_placeIcon, m_placeButton, m_placeTooltip) = CreatePlaceButton();
            m_placeHost.Margin = new Vector2(ButtonGap, 0);
            Children.Add(m_placeHost);

            Refresh();
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

        (CanvasWidget Host, RectangleWidget Icon, InteractableWidget Clickable, LabelWidget Tooltip) CreatePlaceButton() {
            var host = new CanvasWidget {
                Size = new Vector2(HitSize, HitSize),
            };
            var iconWidget = new RectangleWidget {
                Size = new Vector2(IconSize, IconSize),
                Subtexture = s_plusIcon,
                FillColor = new Color(255, 255, 255, 200),
                OutlineColor = Color.Transparent,
                HorizontalAlignment = WidgetAlignment.Center,
                VerticalAlignment = WidgetAlignment.Center,
                BlendState = BlendState.NonPremultiplied,
                TextureLinearFilter = true,
            };
            var interactable = new InteractableWidget {
                HorizontalAlignment = WidgetAlignment.Stretch,
                VerticalAlignment = WidgetAlignment.Stretch,
                SoundName = string.Empty,
            };
            var tooltip = new LabelWidget {
                FontScale = TooltipFontScale,
                Color = new Color(255, 228, 160, 255),
                HorizontalAlignment = WidgetAlignment.Far,
                VerticalAlignment = WidgetAlignment.Near,
                TextAnchor = TextAnchor.HorizontalCenter | TextAnchor.VerticalCenter,
                IsHitTestVisible = false,
                IsVisible = false,
            };
            host.Children.Add(iconWidget);
            host.Children.Add(interactable);
            host.Children.Add(tooltip);
            return (host, iconWidget, interactable, tooltip);
        }

        public override void MeasureOverride(Vector2 parentAvailableSize) {
            base.MeasureOverride(BarSize);
            DesiredSize = BarSize;
        }

        public override void ArrangeOverride() {
            base.ArrangeOverride();
            if (!m_placeTooltip.IsVisible) return;
            Vector2 tooltipSize = m_placeTooltip.DesiredSize;
            if (tooltipSize.X <= 0f) return;
            float x = -tooltipSize.X - 4f;
            float y = (HitSize - tooltipSize.Y) * 0.5f;
            m_placeTooltip.Arrange(new Vector2(x, y), tooltipSize);
        }

        public void Refresh() {
            RefreshBookmarkVisual();
            RefreshGateState();
            RefreshTooltipVisibility();
        }

        public override void Update() {
            if (m_bookmarkButton.IsClicked) {
                m_host.ToggleRecipeBookmark(m_recipe);
                RefreshBookmarkVisual();
            }
            if (m_gateOpen) HandlePlaceInput();
            RefreshTooltipVisibility();
        }

        void HandlePlaceInput() {
            if (m_placeButton.IsTapped) {
                m_pressStartTime = Time.FrameStartTime;
                m_placeRepeater.OnPressStart();
                m_firstRepeatClearGrid = true;
                m_repeatExhausted = false;
                m_longPressGesture = false;
            }

            if (m_placeButton.IsPressed && !m_repeatExhausted) {
                if ((float)(Time.FrameStartTime - m_pressStartTime) >= SettingsManager.MinimumHoldDuration) {
                    m_longPressGesture = true;
                }
                if (!m_placeRepeater.UpdateWhilePressed(SettingsManager.MinimumHoldDuration, TryPlaceRepeatStep)) {
                    m_repeatExhausted = true;
                }
            }
            else if (m_placeRepeater.RepeatActive) {
                m_placeRepeater.Reset();
            }

            if (m_placeButton.IsClicked && !m_longPressGesture) {
                m_host.PlaceRecipe(m_recipe);
            }
        }

        bool TryPlaceRepeatStep() {
            bool clearGrid = m_firstRepeatClearGrid;
            m_firstRepeatClearGrid = false;
            bool placed = m_host.PlaceRecipe(m_recipe, clearGridBeforePlace: clearGrid, showFeedback: false);
            if (placed) m_longPressGesture = true;
            return placed;
        }

        void RefreshGateState() {
            m_gateOpen = m_host.PassesPlacementGate(m_recipe, out string disabledReason);
            m_disabledReason = disabledReason;
            m_placeIcon.FillColor = m_gateOpen
                ? new Color(140, 230, 140, 255)
                : new Color(96, 96, 96, 160);
            m_placeButton.ColorTransform = m_gateOpen ? Color.White : new Color(128, 128, 128, 160);
            m_placeButton.SoundName = m_gateOpen ? "Audio/UI/ButtonClick" : string.Empty;
        }

        void RefreshTooltipVisibility() {
            if (!m_placeButton.IsMouseHover) {
                if (m_placeTooltip.IsVisible) m_placeTooltip.IsVisible = false;
                return;
            }

            string text = m_gateOpen
                ? LanguageControl.GetContentWidgets(RecipaediaCraftingOverlayDialog.LanguageName, 14)
                : m_disabledReason;
            if (string.IsNullOrEmpty(text)) {
                if (m_placeTooltip.IsVisible) m_placeTooltip.IsVisible = false;
                return;
            }

            if (m_placeTooltip.Text != text) {
                m_placeTooltip.Text = text;
                if (m_placeTooltip.IsVisible) {
                    m_placeHost.ParentWidget?.Measure(m_placeHost.ParentWidget.ParentDesiredSize);
                }
            }
            if (m_placeTooltip.IsVisible) return;
            m_placeTooltip.IsVisible = true;
            m_placeHost.ParentWidget?.Measure(m_placeHost.ParentWidget.ParentDesiredSize);
        }

        void RefreshBookmarkVisual() {
            bool bookmarked = m_host.IsRecipeBookmarked(m_recipe);
            m_bookmarkIcon.FillColor = bookmarked
                ? new Color(255, 220, 80, 255)
                : new Color(255, 255, 255, 160);
        }
    }
}
