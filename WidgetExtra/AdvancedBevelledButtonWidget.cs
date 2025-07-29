using System.Xml.Linq;
using Engine;
using Engine.Media;
using Game;

namespace RecipaediaEX.UI {
    public class AdvancedBevelledButtonWidget : ButtonWidget {
        public BevelledRectangleWidget m_rectangleWidget;
        public RectangleWidget m_imageWidget;
        public LabelWidget m_labelWidget;
        public InteractableWidget m_interactableWidget;

        public float FontScale {
            get => this.m_labelWidget.FontScale;
            set => this.m_labelWidget.FontScale = value;
        }

        public override bool IsClicked => this.m_interactableWidget.IsClicked;

        public virtual bool IsSpecialClicked => m_interactableWidget.IsSpecialClicked;

        public virtual bool IsMouseHover => m_interactableWidget.IsMouseHover;

        public override bool IsChecked {
            get => this.m_interactableWidget.IsChecked;
            set => this.m_interactableWidget.IsChecked = value;
        }

        public override bool IsAutoCheckingEnabled {
            get => this.m_interactableWidget.IsAutoCheckingEnabled;
            set => this.m_interactableWidget.IsAutoCheckingEnabled = value;
        }

        public override string Text {
            get => this.m_labelWidget.Text;
            set => this.m_labelWidget.Text = value;
        }

        public override BitmapFont Font {
            get => this.m_labelWidget.Font;
            set => this.m_labelWidget.Font = value;
        }

        public Subtexture Subtexture {
            get => this.m_imageWidget.Subtexture;
            set => this.m_imageWidget.Subtexture = value;
        }

        public override Color Color { get; set; }

        public Color BevelColor {
            get => this.m_rectangleWidget.BevelColor;
            set => this.m_rectangleWidget.BevelColor = value;
        }

        public Color CenterColor {
            get => this.m_rectangleWidget.CenterColor;
            set => this.m_rectangleWidget.CenterColor = value;
        }

        public float AmbientLight {
            get => this.m_rectangleWidget.AmbientLight;
            set => this.m_rectangleWidget.AmbientLight = value;
        }

        public float DirectionalLight {
            get => this.m_rectangleWidget.DirectionalLight;
            set => this.m_rectangleWidget.DirectionalLight = value;
        }

        public float BevelSize { get; set; }

        public AdvancedBevelledButtonWidget() {
            this.Color = Color.White;
            this.BevelSize = 2f;
            XElement node = RecipaediaEXLoader.RequestWidgetFile("AdvancedBevelledButtonContents");
            this.LoadChildren((object)this, node);
            this.m_rectangleWidget = this.Children.Find<BevelledRectangleWidget>("BevelledButton.Rectangle");
            this.m_imageWidget = this.Children.Find<RectangleWidget>("BevelledButton.Image");
            this.m_labelWidget = this.Children.Find<LabelWidget>("BevelledButton.Label");
            this.m_interactableWidget = this.Children.Find<InteractableWidget>("BevelledButton.Interactable");
            this.m_labelWidget.VerticalAlignment = WidgetAlignment.Center;
            this.LoadProperties((object)this, node);
        }

        public override void MeasureOverride(Vector2 parentAvailableSize) {
            bool isEnabledGlobal = this.IsEnabledGlobal;
            this.m_labelWidget.Color = isEnabledGlobal ? this.Color : new Color(112, 112, 112);
            this.m_imageWidget.FillColor = isEnabledGlobal ? this.Color : new Color(112, 112, 112);
            this.m_rectangleWidget.BevelSize = this.m_interactableWidget.IsPressed || this.IsChecked ? -0.5f * this.BevelSize : this.BevelSize;
            base.MeasureOverride(parentAvailableSize);
        }
    }
}