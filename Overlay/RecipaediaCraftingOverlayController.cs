using System;
using Game;

namespace RecipaediaEX.Overlay {
    public static class RecipaediaCraftingOverlayController {
        static RecipaediaCraftingOverlayDialog? s_dialog;
        static IRecipaediaOverlayHost? s_host;

        public static bool IsOpen => s_dialog != null;

        public static void Toggle(IRecipaediaOverlayHost host) {
            if (s_dialog != null && ReferenceEquals(s_host, host)) {
                Close();
                return;
            }
            Close();
            RecipaediaCraftingContext? context = host.GetCraftingContext();
            if (context == null) return;

            ContainerWidget guiRoot = ResolveGuiRoot(host.OverlayParent);
            s_host = host;
            s_dialog = new RecipaediaCraftingOverlayDialog(host, context);
            guiRoot.Children.Add(s_dialog);
        }

        public static void Close() {
            if (s_dialog == null) return;
            s_dialog.CaptureSessionState();
            s_dialog.HideRecipeDetail();
            s_dialog.ParentWidget?.Children.Remove(s_dialog);
            s_dialog = null;
            s_host = null;
        }

        public static bool TryGetOverlayHost(ComponentGui gui, out IRecipaediaOverlayHost host) {
            host = null!;
            Widget? modal = gui.ModalPanelWidget;
            if (modal is IRecipaediaOverlayHost overlayHost) {
                host = overlayHost;
                return true;
            }
            return false;
        }

        static ContainerWidget ResolveGuiRoot(Widget from) {
            for (Widget? w = from; w != null; w = w.ParentWidget) {
                if (w is GameWidget gameWidget) return gameWidget;
            }
            return from as ContainerWidget ?? throw new InvalidOperationException("Cannot resolve GameWidget for crafting overlay.");
        }
    }
}
