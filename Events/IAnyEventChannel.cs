using System;
using System.Collections.Generic;

namespace RecipaediaEX.Events {
    public interface IEventChannel {
        Type ChannelType { get; }
    }

    public interface IEventChannel<in T> : IEventChannel {
        IReadOnlyList<Action<T>> Handlers { get; }

        Type IEventChannel.ChannelType => typeof(T);
    }
}