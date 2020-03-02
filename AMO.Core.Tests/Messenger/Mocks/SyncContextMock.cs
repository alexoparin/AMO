using System;
using System.Threading;

namespace DbImport2.Core.Tests.Messaging.Mocks
{
    internal class SyncContextMock : SynchronizationContext
    {
        private int _callCount, _sendCount, _postCount;

        internal int CallCount     { get { return _callCount; } }
        internal int SendCallCount { get { return _sendCount; } }
        internal int PostCallCount { get { return _postCount; } }


        public override void Send(SendOrPostCallback d, object state)
        {
            if (d == null) { throw new ArgumentNullException("d"); }

            Interlocked.Increment(ref _callCount);
            Interlocked.Increment(ref _sendCount);
            d.DynamicInvoke(state);
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            if (d == null) { throw new ArgumentNullException("d"); }

            Interlocked.Increment(ref _callCount);
            Interlocked.Increment(ref _postCount);
            d.DynamicInvoke(state);
        }


        internal void Reset()
        {
            Interlocked.Exchange(ref _callCount, 0);
            Interlocked.Exchange(ref _sendCount, 0);
            Interlocked.Exchange(ref _postCount, 0);
        }
    }
}
