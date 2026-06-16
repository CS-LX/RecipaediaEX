using System;
using System.Collections.Generic;
using Engine;

namespace RecipaediaEX.Events {
    /// <summary>
    /// 单类型事件的发布/订阅通道；<see cref="Subscribe"/> 返回 <see cref="IDisposable"/>，Dispose 即退订（语义类似 Rx 的 Subscribe 返回值）。
    /// </summary>
    public sealed class EventChannel<T> : IPublisher<T>, ISubscriber<T>, IEventChannel<T> {
        readonly List<Action<T>> m_handlers = new();

        public Type ChannelType => typeof(T);

        public IReadOnlyList<Action<T>> Handlers {
            get {
                lock (m_handlers) {
                    return m_handlers.AsReadOnly();
                }
            }
        }

        /// <summary>订阅事件；Dispose 后不再接收发布。</summary>
        public IDisposable Subscribe(Action<T> handler) {
            ArgumentNullException.ThrowIfNull(handler);
            lock (m_handlers) {
                m_handlers.Add(handler);
            }
            return new Subscription(this, handler);
        }

        /// <summary>向当前所有订阅者广播（对已 Dispose 的订阅无效）。</summary>
        public void Publish(T value) {
            Action<T>[] snapshot;
            lock (m_handlers) {
                if (m_handlers.Count == 0) return;
                snapshot = m_handlers.ToArray();
            }
            foreach (Action<T> h in snapshot) {
                try {
                    h(value);
                }
                catch (Exception ex) {
                    Log.Error(ex);
                }
            }
        }

        void Unsubscribe(Action<T> handler) {
            lock (m_handlers) {
                m_handlers.Remove(handler);
            }
        }

        sealed class Subscription : IDisposable {
            EventChannel<T>? m_owner;
            Action<T>? m_handler;

            public Subscription(EventChannel<T> owner, Action<T> handler) {
                m_owner = owner;
                m_handler = handler;
            }

            public void Dispose() {
                if (m_owner == null || m_handler == null) return;
                m_owner.Unsubscribe(m_handler);
                m_owner = null;
                m_handler = null;
            }
        }
    }
}
