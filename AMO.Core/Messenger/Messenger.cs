using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using AMO.Core.Infrastructure;

namespace AMO.Core.Messenger
{
    using SubInvokerDictionary = ConcurrentDictionary<Subscription, SubscriptionInvoker>;
    using TypeSubsDictionary = ConcurrentDictionary<Type, ConcurrentDictionary<Subscription, SubscriptionInvoker>>;

    public class Messenger : IMessenger
    {
        private const MessageThread DEFAULT_RECEIVE_THREAD = MessageThread.AsInvoker;

        private readonly TypeSubsDictionary _registries;


        public Messenger()
        {
            _registries = new TypeSubsDictionary();
        }


        #region IMessenger

        public void Publish<T>(T message)
        {
            SubscriptionInvoker invoker;

            foreach (var registry in from kvp in _registries
                                     where typeof(T).IsAssignableFrom(kvp.Key)
                                     select kvp.Value)
            {
                foreach (var kvp in registry)
                {
                    if(!kvp.Value.Invoke(kvp.Key, message))
                    {
                        registry.TryRemove(kvp.Key, out invoker);
                    }
                }
            }
        }

        #region Subscribe<T>

        public void Subscribe<T>(ISubscriber<T> listener)
        {
            if (listener == null) { throw new ArgumentNullException("listener"); }

            subscribeInternal(typeof(T), new Action<T>(listener.OnMessage), DEFAULT_RECEIVE_THREAD);
        }

        public void Subscribe<T>(ISubscriber<T> listener, MessageThread thread)
        {
            if (listener == null) { throw new ArgumentNullException("listener"); }

            subscribeInternal(typeof(T), new Action<T>(listener.OnMessage), thread);
        }

        public void Subscribe<T>(Action<T> messageHandler)
        {
            subscribeInternal(typeof(T), messageHandler, DEFAULT_RECEIVE_THREAD);
        }

        public void Subscribe<T>(Action<T> messageHandler, MessageThread thread)
        {
            subscribeInternal(typeof(T), messageHandler, thread);
        }

        #endregion

        #region Unsubscribe<T>

        public void Unsubscribe<T>(ISubscriber<T> listener)
        {
            if (listener == null) { throw new ArgumentNullException("listener"); }

            unsubscribeInternal(typeof(T), new Action<T>(listener.OnMessage));
        }

        public void Unsubscribe<T>(Action<T> messageHandler)
        {
            unsubscribeInternal(typeof(T), messageHandler);
        }

        #endregion

        public void ClearDead()
        {
            SubscriptionInvoker invoker;

            foreach (var registry in _registries.Values)
            {
                foreach (var kvp in registry)
                {
                    if (!kvp.Key.IsAlive)
                    {
                        registry.TryRemove(kvp.Key, out invoker);
                    }
                }
            }
        }

        #endregion

        private void subscribeInternal(Type messageType, Delegate messageHandler, MessageThread thread)
        {
            Debug.Assert(messageType != null, "messageType not set");
            Debug.Assert(messageHandler != null, "messageHandler not set");

            _registries
                .GetOrAdd(messageType, _ => new SubInvokerDictionary())
                .AddOrUpdate(new Subscription(messageHandler),
                s => SubscriptionInvoker.GetInstance(thread),
                (sub, inv) => SubscriptionInvoker.GetInstance(thread));
        }

        private void unsubscribeInternal(Type messageType, Delegate messageHandler)
        {
            Debug.Assert(messageType != null, "messageType not set");
            Debug.Assert(messageHandler != null, "messageHandler not set");

            SubInvokerDictionary registry;
            SubscriptionInvoker invoker;

            if (messageType != null && _registries.TryGetValue(messageType, out registry))
            {
                registry.TryRemove(new Subscription(messageHandler), out invoker);
            }
        }
    }
}
