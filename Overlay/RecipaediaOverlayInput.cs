using Engine.Input;
using Game;
using RecipaediaEX.Overlay;
using RecipaediaEX.Events;

namespace RecipaediaEX {
    public static class RecipaediaOverlayInput {
        public static bool IsRecipaediaKeyPressed() {
            object mapping = SettingsManager.GetKeyboardMapping("Recipaedia");
            return mapping switch {
                Key key => key != Key.Null && Keyboard.IsKeyDownOnce(key),
                MouseButton button => Mouse.IsMouseButtonDown(button),
                _ => false,
            };
        }

        public static void HandleRecipaediaKey(ComponentGui gui) {
            if (!IsRecipaediaKeyPressed()) return;
            ContainerWidget guiWidget = gui.m_componentPlayer.GuiWidget;
            if (DialogsManager.HasDialogs(guiWidget) && !RecipaediaCraftingOverlayController.IsOpen) return;

            if (RecipaediaCraftingOverlayController.TryGetOverlayHost(gui, out IRecipaediaOverlayHost host)
                && host.GetCraftingContext() != null) {
                RecipaediaCraftingOverlayController.Toggle(host);
                return;
            }

            RecipaediaEventBus.GetPublisher<OpenFullRecipaediaRequestedEvent>().Publish(default);
        }
    }
}
