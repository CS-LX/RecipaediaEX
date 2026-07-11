using RecipaediaEX.Overlay;

namespace RecipaediaEX.Events {
    public enum CraftingOverlayCloseReason {
        /// <summary>玩家 toggle 或等价逻辑隐藏助手，Host Modal 仍在。</summary>
        Hide,
        /// <summary>销毁助手实例（Host 关闭、切换 Host 等）。</summary>
        Dismiss,
    }

    /// <summary>合成助手即将打开或重新显示之前。</summary>
    public readonly struct CraftingOverlayOpeningContext {
        public CraftingOverlayOpeningContext(IRecipaediaOverlayHost host, RecipaediaCraftingContext context, bool isReopening) {
            Host = host;
            Context = context;
            IsReopening = isReopening;
        }

        public IRecipaediaOverlayHost Host { get; }
        public RecipaediaCraftingContext Context { get; }
        /// <summary><see langword="true"/> 表示已有实例仅重新设为可见；<see langword="false"/> 表示新建 Dialog。</summary>
        public bool IsReopening { get; }
    }

    /// <summary>合成助手即将隐藏或销毁之前。</summary>
    public readonly struct CraftingOverlayClosingContext {
        public CraftingOverlayClosingContext(IRecipaediaOverlayHost? host, CraftingOverlayCloseReason reason) {
            Host = host;
            Reason = reason;
        }

        public IRecipaediaOverlayHost? Host { get; }
        public CraftingOverlayCloseReason Reason { get; }
    }
}
