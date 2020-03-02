using System.Collections.Concurrent;
using System.Threading;

namespace AMO.Core.Messenger
{
    using ContextInvokerDictionary = ConcurrentDictionary<SynchronizationContext, SynchronizedInvoker>;

    internal class SynchronizedInvoker : SubscriptionInvoker
    {
        private static readonly ContextInvokerDictionary _contextInvokers = new ContextInvokerDictionary();

        internal static SynchronizedInvoker GetInstance()
        {
            var syncContext = SynchronizationContext.Current;
            if (syncContext == null)
            {
                throw new LibraryException("Synchronization context not set");
            }
            return _contextInvokers.GetOrAdd(syncContext, c => new SynchronizedInvoker(c, Thread.CurrentThread));
        }


        private readonly SynchronizationContext _syncContext;
        private readonly Thread                 _referenceThread;


        private SynchronizedInvoker(SynchronizationContext syncContext, Thread referenceThread)
        {
            _syncContext     = syncContext;
            _referenceThread = referenceThread;
        }


        internal sealed override bool Invoke(Subscription subscription, object message)
        {
            if (Thread.CurrentThread == _referenceThread)
            {
                return invokeInternal(subscription, message);
            }
            else
            {
                bool result = false;
                _syncContext.Send(new SendOrPostCallback(delegate (object state)
                {
                    result = invokeInternal(subscription, state);
                }),
                message);
                return result;
            }
        }
    }
}
