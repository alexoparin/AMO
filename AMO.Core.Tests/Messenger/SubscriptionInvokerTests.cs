using System;
using System.Threading;
using System.Threading.Tasks;
using AMO.Core;
using AMO.Core.Infrastructure;
using AMO.Core.Messenger;
using DbImport2.Core.Tests.Messaging.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbImport2.Core.Tests.Messaging
{
    [TestClass]
    public class SubscriptionInvokerTests
    {
        private SyncContextMock _syncContextMock;

        [TestInitialize]
        public void InitSynchronizationContext()
        {
            _syncContextMock = new SyncContextMock();
            SynchronizationContext.SetSynchronizationContext(_syncContextMock);
        }

        [TestMethod]
        public void ThereExistAnInvokerForEachMessageThreadValue()
        {
            foreach (MessageThread messageThread in Enum.GetValues(typeof(MessageThread)))
            {
                var invoker = SubscriptionInvoker.GetInstance(messageThread);
                Assert.IsNotNull(invoker);
                switch (messageThread)
                {
                    case MessageThread.AsInvoker:    Assert.IsInstanceOfType(invoker, typeof(CurrentThreadInvoker)); break;
                    case MessageThread.NewThread:    Assert.IsInstanceOfType(invoker, typeof(NewThreadInvoker));     break;
                    case MessageThread.Synchronized: Assert.IsInstanceOfType(invoker, typeof(SynchronizedInvoker));  break;
                    default:
                        throw new Exception(string.Format("Untested message thread option {0}", messageThread));
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(LibraryException))]
        public void SynchronizedMessageThreadRequiresSynchronizationContextToBeSet()
        {
            SynchronizationContext.SetSynchronizationContext(null);
            SubscriptionInvoker.GetInstance(MessageThread.Synchronized);
        }

        [TestMethod]
        [ExpectedException(typeof(LibraryException))]
        public void InvalidMessageThreadValueCausesLibraryException()
        {
            MessageThread messageThread = MessageThread.AsInvoker;
            while (Enum.IsDefined(typeof(MessageThread), ++messageThread))
            {
            }
            SubscriptionInvoker.GetInstance(messageThread);
        }

        [TestMethod]
        public void CurrentThreadInvokerIsASingleton()
        {
            Assert.IsTrue(ReferenceEquals(CurrentThreadInvoker.GetInstance(), CurrentThreadInvoker.GetInstance()));
        }

        [TestMethod]
        public void NewThreadInvokerIsASingleton()
        {
            Assert.IsTrue(ReferenceEquals(NewThreadInvoker.GetInstance(), NewThreadInvoker.GetInstance()));
        }

        [TestMethod]
        public void SynchronizedInvokerIsASingletonPerSynchronizationContext()
        {
            var syncContext1Invoker1 = SynchronizedInvoker.GetInstance();
            var syncContext1Invoker2 = SynchronizedInvoker.GetInstance();
            InitSynchronizationContext();
            var syncContext2Invoker1 = SynchronizedInvoker.GetInstance();

            Assert.IsTrue(ReferenceEquals(syncContext1Invoker1, syncContext1Invoker2));
            Assert.IsFalse(ReferenceEquals(syncContext1Invoker1, syncContext2Invoker1));
        }

        [TestMethod]
        public void CurrentThreadInvokerInvokesOnACurrentThread()
        {
            Thread invokeThread = null;
            string invokedWith  = null;
            var action = new Action<string>(arg =>
            {
                invokedWith = arg;
                invokeThread = Thread.CurrentThread;
            });
            var sub = new Subscription(action);
            CurrentThreadInvoker.GetInstance().Invoke(sub, "Hello");

            Assert.IsNotNull(invokeThread);
            Assert.IsNotNull(invokedWith);
            Assert.AreEqual(Thread.CurrentThread, invokeThread);
            Assert.AreEqual("Hello", invokedWith);
        }

        [TestMethod]
        public void NewThreadInvokerInvokesOnANewThread()
        {
            Thread invokeThread = null;
            string invokedWith = null;
            var action = new Action<string>(arg =>
            {
                invokedWith = arg;
                invokeThread = Thread.CurrentThread;
            });
            var sub = new Subscription(action);
            NewThreadInvoker.GetInstance().Invoke(sub, "Hello");

            Assert.IsNotNull(invokeThread);
            Assert.IsNotNull(invokedWith);
            Assert.AreNotEqual(Thread.CurrentThread, invokeThread);
            Assert.AreEqual("Hello", invokedWith);
        }

        [TestMethod]
        public void SynchronizedInvokerSendsToASyncContext()
        {
            string invokedWith = null;
            var sub = new Subscription(new Action<string>(s => invokedWith = s));
            var invoker = SynchronizedInvoker.GetInstance();

            Task.Factory.StartNew(() => invoker.Invoke(sub, "Hello")).Wait();

            Assert.AreEqual(1, _syncContextMock.CallCount);
            Assert.AreEqual("Hello", invokedWith);
        }

        [TestMethod]
        public void SynchronizedInvokerUsesSynchronizationContextOnlyIfInvokedOnDifferentThread()
        {
            string invokedWith = null;
            var sub = new Subscription(new Action<string>(s => invokedWith = s));
            var invoker = SynchronizedInvoker.GetInstance();

            invoker.Invoke(sub, "Hello");
            Assert.AreEqual(0, _syncContextMock.CallCount);
            Assert.AreEqual("Hello", invokedWith);

            Task.Factory.StartNew(() => invoker.Invoke(sub, "Hello2")).Wait();
            Assert.AreEqual(1, _syncContextMock.CallCount);
            Assert.AreEqual("Hello2", invokedWith);
        }
    }
}
