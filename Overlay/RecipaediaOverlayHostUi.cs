using Game;

namespace RecipaediaEX.Overlay {
    public static class RecipaediaOverlayHostUi {
        public static void EnsureToggleButton(CanvasWidget parent, IRecipaediaOverlayHost host) {
            if (parent.Children.Find<RecipaediaOverlayToggleButton>("RecipaediaOverlayToggle", false) != null) return;
            var button = new RecipaediaOverlayToggleButton(host) { Name = "RecipaediaOverlayToggle" };
            parent.Children.Add(button);
        }
    }
}
