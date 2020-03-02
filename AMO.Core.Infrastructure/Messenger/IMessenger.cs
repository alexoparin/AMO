using System;

namespace AMO.Core.Infrastructure
{
    /// <summary>
    /// Event aggregator interface wich supports weak reference subscriptions
    /// </summary>
    public interface IMessenger
    {
        void Publish<T>(T message);
        void Subscribe<T>(ISubscriber<T> listener);
        void Subscribe<T>(ISubscriber<T> listener, MessageThread thread);
        void Subscribe<T>(Action<T> messageHandler);
        void Subscribe<T>(Action<T> messageHandler, MessageThread thread);
        void Unsubscribe<T>(ISubscriber<T> listener);
        void Unsubscribe<T>(Action<T> messageHandler);

        /// <summary> Clears garbage collected subscriptions </summary>
        void ClearDead();
    }
}
