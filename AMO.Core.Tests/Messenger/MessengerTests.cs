using System;
using System.Threading;
using System.Threading.Tasks;
using AMO.Core.Infrastructure;
using AMO.Core.Messenger;
using DbImport2.Core.Tests.Messaging.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbImport2.Core.Tests.Messaging
{
    [TestClass]
    public class MessengerTests
    {
        private IMessenger      _messenger;
        private EventMessage    _testMsg;
        private Subscriber      _testSubscriber;
        private SyncContextMock _syncContextMock;

        [TestInitialize]
        public void Initialize()
        {
            Subscriber.Reset();

            //_syncContextMock = new Mock<SynchronizationContext>();
            //_syncContextMock
            //    .Setup(sc => sc.Send(It.IsAny<SendOrPostCallback>(), It.IsAny<object>()))
            //    .Callback<SendOrPostCallback, object>((cb, o) => cb.DynamicInvoke(o));

            _syncContextMock = new SyncContextMock();
            SynchronizationContext.SetSynchronizationContext(_syncContextMock);

            _testMsg         = new EventMessage("SampleValue");
            _testSubscriber  = new Subscriber();
            _messenger       = new Messenger();
        }

        [TestMethod]
        public void CanSubscribeAction()
        {
            _messenger.Subscribe(Subscriber.GetCallback());
            _messenger.Publish(_testMsg);

            Assert.AreEqual(_testMsg, Subscriber.LastMessage);
            Assert.AreEqual(1, Subscriber.TotalCalls);
        }

        [TestMethod]
        public void CanNotSubscribeActionTwice()
        {
            _messenger.Subscribe(Subscriber.GetCallback());
            _messenger.Subscribe(Subscriber.GetCallback());
            _messenger.Publish(_testMsg);

            Assert.AreEqual(_testMsg, Subscriber.LastMessage);
            Assert.AreEqual(1, Subscriber.TotalCalls);
        }

        [TestMethod]
        public void CanSubscribeMultipleActions()
        {
            int callCount = 0;
            var action1   = new Action<EventMessage>(e => { callCount++; });
            var action2   = new Action<EventMessage>(e => { callCount -= 2; });

            _messenger.Subscribe(action1);
            _messenger.Subscribe(action2);
            _messenger.Publish(_testMsg);

            Assert.AreEqual(-1, callCount);
        }

        [TestMethod]
        public void CanSubscribeISubscriber()
        {
            var subscriber = new Subscriber();

            _messenger.Subscribe(subscriber);
            _messenger.Publish(_testMsg);

            Assert.AreEqual(_testMsg, subscriber.LastInstanceMessage);
            Assert.AreEqual(1, subscriber.InstanceCalls);
        }

        [TestMethod]
        public void CanNotSubscribeISubcriberTwice()
        {
            _messenger.Subscribe(_testSubscriber);
            _messenger.Subscribe(_testSubscriber);

            _messenger.Publish(_testMsg);

            Assert.AreEqual(_testMsg, _testSubscriber.LastInstanceMessage);
            Assert.AreEqual(1, _testSubscriber.InstanceCalls);
        }

        [TestMethod]
        public void CanSubscribeMultipleISubscribers()
        {
            int numOfSubs = 5;
            var testMsg2 = new EventMessage("Hello2");

            var subs = new Subscriber[numOfSubs];

            for (int i = 0; i < subs.Length; ++i)
            {
                var mock = new Subscriber();
                _messenger.Subscribe(mock);
                subs[i] = mock;
            }

            _messenger.Publish(_testMsg);
            _messenger.Publish(testMsg2);

            Assert.AreEqual(testMsg2, Subscriber.LastMessage);
            Assert.AreEqual(numOfSubs * 2, Subscriber.TotalCalls);
        }

        [TestMethod]
        public void CanNotSubscribeISubscriberAndItsActionTwice()
        {
            _messenger.Subscribe(new Action<EventMessage>(_testSubscriber.OnMessage));
            _messenger.Subscribe(_testSubscriber);

            _messenger.Publish(_testMsg);
            Assert.AreEqual(_testMsg, _testSubscriber.LastInstanceMessage);
            Assert.AreEqual(1, _testSubscriber.InstanceCalls);
        }

        [TestMethod]
        public void UsesWeakReferencies()
        {
            int numOfSums = 10;
            for (int i = 0; i < numOfSums; ++i)
            {
                _messenger.Subscribe(new Subscriber());
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            _messenger.Publish(_testMsg);

            Assert.IsNull(Subscriber.LastMessage);
            Assert.AreEqual(0, Subscriber.TotalCalls);
        }
        
        [TestMethod]
        public void ForSynchronizedSubscribtions_UsesSyncContextIfRequired()
        {
            var sub2 = new Subscriber();

            _messenger.Subscribe(_testSubscriber, MessageThread.Synchronized);
            _messenger.Subscribe(sub2);

            Task.Factory.StartNew(() => _messenger.Publish(_testMsg)).Wait();

            Assert.AreEqual(1, _syncContextMock.CallCount);
        }

        [TestMethod]
        public void ForSynchronizedSubscribtions_DoesNotUseSyncContextIfNotRequired()
        {
            _messenger.Subscribe(_testSubscriber, MessageThread.Synchronized);
            _messenger.Publish(_testMsg);

            Assert.AreEqual(0, _syncContextMock.CallCount);
        }

        [TestMethod]
        public void ForSynchronizedSubscribtions_SyncContextsSendOrPoseCallbackInvokesHandlerWithArgumentOnce()
        {
            _messenger.Subscribe(_testSubscriber, MessageThread.Synchronized);

            Task.Factory.StartNew(() => _messenger.Publish(_testMsg)).Wait();

            Assert.AreEqual(1, _syncContextMock.CallCount);
            Assert.AreEqual(_testMsg, _testSubscriber.LastInstanceMessage);
            Assert.AreEqual(1, _testSubscriber.InstanceCalls);
        }

        [TestMethod]
        public void ForNewThreadSubscriptions_InvokesHandlerOnNewThreads()
        {
            Thread invokeThread = null;

            _messenger.Subscribe(new Action<EventMessage>(e =>
            {
                invokeThread = Thread.CurrentThread;
            }), MessageThread.NewThread);

            for (int i = 0; i < 100000; ++i)
            {
                _messenger.Publish(_testMsg);
                Assert.IsNotNull(invokeThread);
                Assert.AreNotEqual(Thread.CurrentThread, invokeThread);
                invokeThread = null;
            }
        }

        [TestMethod]
        public void RunFor_100000_Handlers()
        {
            var numOfSubs = 100000;
            var subs      = new Subscriber[numOfSubs];

            for (int i = 0; i < numOfSubs; ++i)
            {
                _messenger.Subscribe(subs[i] = new Subscriber());
            }
            
            _messenger.Publish(_testMsg);

            Assert.AreEqual(_testMsg, Subscriber.LastMessage);
            Assert.AreEqual(numOfSubs, Subscriber.TotalCalls);
        }

        [TestMethod]
        public void RunFor_100000_Messages()
        {
            var numOfMsgs = 100000;
            var subs = new EventMessage[numOfMsgs];

            _messenger.Subscribe(Subscriber.GetCallback());
            for (int i = 1; i <= numOfMsgs; ++i)
            {
                _messenger.Publish(new EventMessage(string.Format("EventMessage {0}", i)));
            }

            Assert.IsNotNull(Subscriber.LastMessage);
            Assert.AreEqual(string.Format("EventMessage {0}", numOfMsgs), Subscriber.LastMessage.SampleParam);
            Assert.AreEqual(numOfMsgs, Subscriber.TotalCalls);
        }

        [TestMethod]
        public void SubscribeCallUpdatesExistingSubscription()
        {
            _messenger.Subscribe(Subscriber.GetCallback(), MessageThread.AsInvoker);
            Task.Factory.StartNew(() => _messenger.Publish(_testMsg)).Wait();

            _messenger.Subscribe(Subscriber.GetCallback(), MessageThread.Synchronized);
            Task.Factory.StartNew(() => _messenger.Publish(_testMsg)).Wait();

            Assert.AreEqual(_testMsg, Subscriber.LastMessage);
            Assert.AreEqual(2, Subscriber.TotalCalls);
            Assert.AreEqual(1, _syncContextMock.CallCount);
        }

        [TestMethod]
        public void CanUnsubscribeDelegateRegistrations()
        {
            _messenger.Subscribe(Subscriber.GetCallback());
            _messenger.Publish(_testMsg);
            _messenger.Unsubscribe(Subscriber.GetCallback());
            _messenger.Publish(_testMsg);

            Assert.AreEqual(_testMsg, Subscriber.LastMessage);
            Assert.AreEqual(1, Subscriber.TotalCalls);
        }

        [TestMethod]
        public void CanUnsubscribeISubscriberRegistrations()
        {
            _messenger.Subscribe(_testSubscriber);
            _messenger.Unsubscribe(_testSubscriber);
            _messenger.Publish(_testMsg);

            Assert.AreEqual(null, _testSubscriber.LastInstanceMessage);
            Assert.AreEqual(0, _testSubscriber.InstanceCalls);
        }

        [TestMethod]
        public void CanUnsubscribeMixedRegistrations()
        {
            _messenger.Subscribe(_testSubscriber);
            _messenger.Unsubscribe(new Action<EventMessage>(_testSubscriber.OnMessage));
            _messenger.Publish(_testMsg);

            Assert.AreEqual(null, _testSubscriber.LastInstanceMessage);
            Assert.AreEqual(0, _testSubscriber.InstanceCalls);
        }
    }
}
