using System;
using Game;

namespace RecipaediaEX.Overlay {
    public static class RecipaediaCraftingOverlayController {
        static RecipaediaCraftingOverlayDialog? s_dialog;
        static IRecipaediaOverlayHost? s_host;

        public static bool IsOpen => s_dialog != null && s_dialog.IsVisible;

        public static void Toggle(IRecipaediaOverlayHost host) {
            RecipaediaCraftingContext? context = host.GetCraftingContext();
            if (context == null) return;
            if (s_dialog != null && ReferenceEquals(s_host, host)) {
                if (s_dialog.IsVisible) Hide();
                else ShowExisting();
                return;
            }
            Dismiss();
            Open(host, context);
        }

        public static void Hide() {
            if (s_dialog == null || !s_dialog.IsVisible) return;
            s_dialog.CaptureSessionState();
            s_dialog.HideRecipeDetail();
            s_dialog.IsVisible = false;
        }

        public static void Dismiss() {
            if (s_dialog == null) return;
            s_dialog.CaptureSessionState();
            s_dialog.HideRecipeDetail();
            s_dialog.ParentWidget?.Children.Remove(s_dialog);
            s_dialog = null;
            s_host = null;
        }

        /// <summary>Host Modal 关闭或替换时销毁助手（D28）。toggle 关助手请用 <see cref="Hide"/>。</summary>
        public static void DismissForModalWidget(Widget? modalWidget) {
            if (s_dialog == null || s_host == null) return;
            if (modalWidget == null || IsHostOnModal(s_host, modalWidget)) Dismiss();
        }

        public static bool TryGetOverlayHost(ComponentGui gui, out IRecipaediaOverlayHost host) {
            host = null!;
            Widget? modal = gui.ModalPanelWidget;
            if (modal is IRecipaediaOverlayHost overlayHost && overlayHost.GetCraftingContext() != null) {
                host = overlayHost;
                return true;
            }
            return false;
        }

        static void ShowExisting() {
            if (s_dialog == null || s_host == null) return;
            s_dialog.RefreshHostContext();
            if (s_dialog == null) return;
            s_dialog.IsVisible = true;
        }

        static void Open(IRecipaediaOverlayHost host, RecipaediaCraftingContext context) {
            ContainerWidget guiRoot = ResolveGuiRoot(host.OverlayParent);
            s_host = host;
            s_dialog = new RecipaediaCraftingOverlayDialog(host, context);
            guiRoot.Children.Add(s_dialog);
        }

        static bool IsHostOnModal(IRecipaediaOverlayHost host, Widget modal) {
            if (ReferenceEquals(host, modal)) return true;
            Widget? widget = host.OverlayParent;
            while (widget != null) {
                if (ReferenceEquals(widget, modal)) return true;
                widget = widget.ParentWidget;
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
