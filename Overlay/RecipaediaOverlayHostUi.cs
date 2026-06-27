using Game;

namespace RecipaediaEX.Overlay {
    public static class RecipaediaOverlayHostUi {
        public static void EnsureToggleButton(CanvasWidget parent, IRecipaediaOverlayHost host) {
            RecipaediaOverlayToggleButton? button = parent.Children.Find<RecipaediaOverlayToggleButton>("RecipaediaOverlayToggle", false);
            if (host.GetCraftingContext() == null) {
                if (button != null) parent.Children.Remove(button);
                return;
            }
            if (button != null) return;
            parent.Children.Add(new RecipaediaOverlayToggleButton(host) { Name = "RecipaediaOverlayToggle" });
        }
    }
}
