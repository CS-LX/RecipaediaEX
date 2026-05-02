using System;
using System.Collections.Concurrent;

namespace RecipaediaEX.Events {
    /// <summary>
    /// RecipaediaEX 侧可扩展事件总线：按事件类型维护独立 <see cref="EventChannel{T}"/>，
    /// 便于其它模组在不改 RX 源码的情况下订阅自定义载荷类型。
    /// </summary>
    public static class RecipaediaEventBus {
        static readonly ConcurrentDictionary<Type, object> s_channelsByType = new();

        /// <summary>内置：扩展工作台/熔炉产物格取出。</summary>
        public static EventChannel<CrafterOutputRemovedEvent> CrafterOutputRemoved { get; } = new();

        /// <summary>按类型获取或创建通道（用于自定义事件类型）。</summary>
        public static EventChannel<T> Channel<T>() =>
            (EventChannel<T>)s_channelsByType.GetOrAdd(typeof(T), _ => new EventChannel<T>());
    }
}
