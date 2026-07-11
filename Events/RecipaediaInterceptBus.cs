using System;
using System.Collections.Concurrent;

namespace RecipaediaEX.Events {
    /// <summary>
    /// RecipaediaEX 拦截总线：与 <see cref="RecipaediaEventBus"/> 的通知通道互补，
    /// 用于在操作执行前由依赖模组否决（返回 <see langword="false"/>）。
    /// </summary>
    public static class RecipaediaInterceptBus {
        static readonly ConcurrentDictionary<Type, IAnyInterceptChannel> m_channelsByType = new();

        static InterceptChannel<T> Channel<T>() => (InterceptChannel<T>)m_channelsByType.GetOrAdd(typeof(T), _ => new InterceptChannel<T>());

        /// <summary>获取指定上下文类型的拦截发布端。</summary>
        public static IInterceptPublisher<T> GetPublisher<T>() => Channel<T>();

        /// <summary>获取指定上下文类型的拦截订阅端。</summary>
        public static IInterceptSubscriber<T> GetSubscriber<T>() => Channel<T>();

        /// <summary>在唯一出口调用：任一订阅方否决则返回 <see langword="false"/>。</summary>
        public static bool TryProceed<T>(T context) => Channel<T>().TryProceed(context);
    }
}
