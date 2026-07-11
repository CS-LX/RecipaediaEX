using System;

namespace RecipaediaEX.Events {
    /// <summary>
    /// 拦截通道订阅端：注册可在操作执行前否决的处理器。
    /// </summary>
    public interface IInterceptSubscriber<out T> {
        /// <summary>
        /// 订阅拦截器。<paramref name="handler"/> 返回 <see langword="true"/> 表示允许继续，
        /// <see langword="false"/> 表示否决当前操作。Dispose 即退订。
        /// </summary>
        /// <param name="handler">拦截逻辑。</param>
        /// <param name="priority">优先级，数值越小越先执行（与宿主 <c>ModsManager.RegisterHook</c> 一致）。</param>
        IDisposable Subscribe(Func<T, bool> handler, int priority = 0);
    }
}
