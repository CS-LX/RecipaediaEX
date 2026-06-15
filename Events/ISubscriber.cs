using System;

namespace RecipaediaEX.Events {
    public interface ISubscriber<out T> {
        IDisposable Subscribe(Action<T> handler);
    }
}