using System;
using System.Collections.Generic;
using Engine;

namespace RecipaediaEX.Events {
    /// <summary>
    /// 单类型拦截通道：订阅方返回 <see langword="false"/> 即可否决，发布端通过 <see cref="TryProceed"/> 查询是否允许继续。
    /// </summary>
    public sealed class InterceptChannel<T> : IInterceptPublisher<T>, IInterceptSubscriber<T>, IInterceptChannel<T> {
        readonly List<HandlerEntry> m_handlers = new();
        int m_subscribeSequence;

        public int HandlerCount {
            get {
                lock (m_handlers) {
                    return m_handlers.Count;
                }
            }
        }

        public IDisposable Subscribe(Func<T, bool> handler, int priority = 0) {
            ArgumentNullException.ThrowIfNull(handler);
            HandlerEntry entry;
            lock (m_handlers) {
                entry = new HandlerEntry(handler, priority, m_subscribeSequence++);
                int index = m_handlers.BinarySearch(entry, HandlerEntryComparer.Instance);
                if (index < 0) {
                    index = ~index;
                }
                m_handlers.Insert(index, entry);
            }
            return new Subscription(this, entry);
        }

        public bool TryProceed(T context) {
            HandlerEntry[] snapshot;
            lock (m_handlers) {
                if (m_handlers.Count == 0) {
                    return true;
                }
                snapshot = m_handlers.ToArray();
            }
            foreach (HandlerEntry entry in snapshot) {
                if (!entry.IsActive) {
                    continue;
                }
                try {
                    if (!entry.Handler(context)) {
                        return false;
                    }
                }
                catch (Exception ex) {
                    Log.Error(ex);
                }
            }
            return true;
        }

        void Unsubscribe(HandlerEntry entry) {
            lock (m_handlers) {
                entry.Deactivate();
                m_handlers.Remove(entry);
            }
        }

        sealed class HandlerEntry(Func<T, bool> handler, int priority, int sequence) {
            public Func<T, bool> Handler { get; } = handler;
            public int Priority { get; } = priority;
            public int Sequence { get; } = sequence;
            public bool IsActive { get; private set; } = true;

            public void Deactivate() => IsActive = false;
        }

        sealed class HandlerEntryComparer : IComparer<HandlerEntry> {
            public static readonly HandlerEntryComparer Instance = new();

            public int Compare(HandlerEntry? x, HandlerEntry? y) {
                if (ReferenceEquals(x, y)) {
                    return 0;
                }
                if (x is null) {
                    return -1;
                }
                if (y is null) {
                    return 1;
                }
                int priorityCompare = x.Priority.CompareTo(y.Priority);
                if (priorityCompare != 0) {
                    return priorityCompare;
                }
                return x.Sequence.CompareTo(y.Sequence);
            }
        }

        sealed class Subscription(InterceptChannel<T> owner, HandlerEntry entry) : IDisposable {
            InterceptChannel<T>? m_owner = owner;

            public void Dispose() {
                if (m_owner == null) {
                    return;
                }
                m_owner.Unsubscribe(entry);
                m_owner = null;
            }
        }
    }
}
