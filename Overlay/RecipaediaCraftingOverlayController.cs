using System;
using Game;
using RecipaediaEX.Events;

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
                else if (TryOpen(host, context, isReopening: true)) ShowExisting();
                return;
            }
            DismissSilently();
            if (!TryOpen(host, context, isReopening: false)) return;
            Open(host, context);
        }

        static bool TryOpen(IRecipaediaOverlayHost host, RecipaediaCraftingContext context, bool isReopening) =>
            RecipaediaInterceptBus.TryProceed(new CraftingOverlayOpeningContext(host, context, isReopening));

        public static void Hide() {
            if (s_dialog == null || !s_dialog.IsVisible) return;
            if (!RecipaediaInterceptBus.TryProceed(new CraftingOverlayClosingContext(s_host, CraftingOverlayCloseReason.Hide))) return;
            s_dialog.CaptureSessionState();
            s_dialog.HideRecipeDetail();
            s_dialog.IsVisible = false;
        }

        /// <summary>销毁助手实例；附属模组可经 <see cref="CraftingOverlayClosing"/> 否决。</summary>
        public static void Dismiss() {
            Teardown(notifyClosingIntercept: true);
        }

        /// <summary>Host 切换、Modal 关闭、Context 失效等系统生命周期；不触发关闭拦截。</summary>
        public static void DismissSilently() {
            Teardown(notifyClosingIntercept: false);
        }

        static void Teardown(bool notifyClosingIntercept) {
            if (s_dialog == null) return;
            if (notifyClosingIntercept
                && !RecipaediaInterceptBus.TryProceed(new CraftingOverlayClosingContext(s_host, CraftingOverlayCloseReason.Dismiss))) {
                return;
            }
            s_dialog.CaptureSessionState();
            s_dialog.HideRecipeDetail();
            s_dialog.ParentWidget?.Children.Remove(s_dialog);
            s_dialog = null;
            s_host = null;
        }

        /// <summary>Host Modal 关闭或替换时销毁助手（D28）。toggle 关助手请用 <see cref="Hide"/>。</summary>
        public static void DismissForModalWidget(Widget? modalWidget) {
            if (s_dialog == null || s_host == null) return;
            if (modalWidget == null || IsHostOnModal(s_host, modalWidget)) DismissSilently();
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
