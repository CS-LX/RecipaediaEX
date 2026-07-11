using System;

namespace RecipaediaEX.Events {
    public interface IAnyInterceptChannel {
        Type ChannelType { get; }
    }

    public interface IInterceptChannel<in T> : IAnyInterceptChannel {
        Type IAnyInterceptChannel.ChannelType => typeof(T);
    }
}
