using System;
using System.Threading;
using AMO.Core.Infrastructure;

namespace DbImport2.Core.Tests.Messaging.Mocks
{
    internal class Subscriber : ISubscriber<EventMessage>
    {
        private static int _totalCalls;

        internal static int          TotalCalls  { get { return _totalCalls; } }
        internal static EventMessage LastMessage { get; private set; }


        private Action<EventMessage> _messageHandler;
        private int                  _instanceCalls;

        internal int          InstanceCalls       { get { return _instanceCalls; } }
        internal EventMessage LastInstanceMessage { get; private set; }



        internal Subscriber()
        {
            _messageHandler = s => { };
        }

        internal Subscriber(Action<EventMessage> messageHandler)
        {
            _messageHandler = messageHandler;
        }


        public void OnMessage(EventMessage message)
        {
            if (_messageHandler != null)
            {
                _messageHandler(message);
            }
            Interlocked.Increment(ref _instanceCalls);
            Interlocked.Increment(ref _totalCalls);
            LastMessage = LastInstanceMessage = message;
        }

        public void OnMessage2(EventMessage message)
        {
        }

        internal static void Callback(EventMessage message)
        {
            Interlocked.Increment(ref _totalCalls);
            LastMessage = message;
        }

        internal static void Callback2(EventMessage message)
        {
        }

        internal static Action<EventMessage> GetCallback()
        {
            return new Action<EventMessage>(Callback);
        }

        internal static void Reset()
        {
            LastMessage = null;
            Interlocked.Exchange(ref _totalCalls, 0);
        }
    }
}
