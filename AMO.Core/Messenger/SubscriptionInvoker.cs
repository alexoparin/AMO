using System.Runtime.CompilerServices;
using AMO.Core.Infrastructure;

namespace AMO.Core.Messenger
{
    internal abstract class SubscriptionInvoker
    {
        internal abstract bool Invoke(Subscription subscription, object message);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool invokeInternal(Subscription subscription, object message)
        {
            object target;
            if (subscription.TryGetTarget(out target))
            {
                subscription.Method.Invoke(target, new[] { message });
                return true;
            }
            return false;
        }

        internal static SubscriptionInvoker GetInstance(MessageThread thread)
        {
            switch (thread)
            {
                case MessageThread.AsInvoker:    return CurrentThreadInvoker.GetInstance();
                case MessageThread.NewThread:    return NewThreadInvoker.GetInstance();
                case MessageThread.Synchronized: return SynchronizedInvoker.GetInstance();
                default:
                    throw new LibraryException("Unexpected MessageThread option");
            }
        }
    }
}
