namespace AMO.Core.Messenger
{
    internal class CurrentThreadInvoker : SubscriptionInvoker
    {
        private static readonly CurrentThreadInvoker _instance = new CurrentThreadInvoker();

        internal static CurrentThreadInvoker GetInstance()
        {
            return _instance;
        }

        private CurrentThreadInvoker()
        {
        }


        internal sealed override bool Invoke(Subscription subscription, object message)
        {
            return invokeInternal(subscription, message);
        }
    }
}
