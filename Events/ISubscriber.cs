using System;

namespace RecipaediaEX.Events {
    public interface ISubscriber<T> {
        IDisposable Subscribe(Action<T> handler);
    }
}