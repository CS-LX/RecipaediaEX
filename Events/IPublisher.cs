namespace RecipaediaEX.Events {
    public interface IPublisher<in T> {
        void Publish(T message);
    }
}