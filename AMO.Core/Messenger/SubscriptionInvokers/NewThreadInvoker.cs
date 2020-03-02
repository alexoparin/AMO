using System;
using System.Threading;
using System.Threading.Tasks;

namespace AMO.Core.Messenger
{
    internal class NewThreadInvoker : SubscriptionInvoker
    {
        private static readonly NewThreadInvoker _instance = new NewThreadInvoker();

        internal static NewThreadInvoker GetInstance()
        { 
            return _instance; 
        }


        private NewThreadInvoker()
        {
        }


        internal sealed override bool Invoke(Subscription subscription, object message)
        {
            try
            {
                return Task.Factory.StartNew(() => invokeInternal(subscription, message),
                    CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default)
                    .Result;
            }
            catch(AggregateException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}
