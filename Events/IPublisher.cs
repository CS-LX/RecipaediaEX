namespace RecipaediaEX.Events {
    public interface IPublisher<T> {
        void Publish(T message);
    }
}