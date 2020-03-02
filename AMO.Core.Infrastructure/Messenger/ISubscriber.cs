namespace AMO.Core.Infrastructure
{
    public interface ISubscriber<T>
    {
        void OnMessage(T message);
    }
}
