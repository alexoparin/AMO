using System;
using System.Collections.Generic;
using DbImport2.Core.Tests.Messaging.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AMO.Core.Messenger;

namespace DbImport2.Core.Tests.Messaging
{
    [TestClass]
    public class SubscriptionTests
    {
        [TestMethod]
        public void DefaultIsNotAliveItsMethodIsNullAndCanNotReturnATarget()
        {
            var sub = new Subscription();
            object target;

            Assert.IsFalse(sub.IsAlive);
            Assert.IsNull(sub.Method);
            Assert.IsFalse(sub.TryGetTarget(out target));
            Assert.IsNull(target);
        }

        [TestMethod]
        public void DefaultsAreEqual()
        {
            checkSubscriptionEquality(new Subscription(), new Subscription(), true);
        }

        [TestMethod]
        public void SubsReferencingSameDelegateAreEqual()
        {
            var action = new Action(() => { });
            checkSubscriptionEquality(new Subscription(action), new Subscription(action), true);
        }

        [TestMethod]
        public void SubsReferencingSameStaticDelegateAreEqual()
        {
            var action1 = new Action<EventMessage>(Subscriber.Callback);
            var action2 = new Action<EventMessage>(Subscriber.Callback);
            checkSubscriptionEquality(new Subscription(action1), new Subscription(action2), true);
        }

        [TestMethod]
        public void SubsReferencingDifferentDelegatesAreNotEqual()
        {
            var action1 = new Action(() => { });
            var action2 = new Action(() => { });
            checkSubscriptionEquality(new Subscription(action1), new Subscription(action2), false);
        }

        [TestMethod]
        public void SubCopiesAreEqual()
        {
            var sub = new Subscription(new Action(() => { }));

            Assert.IsFalse(ReferenceEquals(sub, sub));
            checkSubscriptionEquality(sub, sub, true);
        }

        [TestMethod]
        public void SubsReferencingDifferentStaticDelegatesAreNotEqual()
        {
            var action1 = new Action<EventMessage>(Subscriber.Callback);
            var action2 = new Action<EventMessage>(Subscriber.Callback2);
            checkSubscriptionEquality(new Subscription(action1), new Subscription(action2), false);
        }

        [TestMethod]
        public void SubReferencingNonStaticDelegateHaveTarget()
        {
            var action = new Action(() => { });
            var sub = new Subscription(action);
            object target;

            Assert.IsTrue(sub.IsAlive);
            Assert.IsTrue(sub.TryGetTarget(out target));
            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void SubReferencingStaticDelegateHasNullTarget()
        {
            var action = new Action<EventMessage>(Subscriber.Callback);
            var sub = new Subscription(action);
            object target;

            Assert.IsTrue(sub.IsAlive);
            Assert.IsTrue(sub.TryGetTarget(out target));
            Assert.IsNull(target);
        }

        [TestMethod]
        public void SubKeepsAWeakReferenceToTarget()
        {
            Subscription sub;
            {
                var demo = new Subscriber();
                sub = new Subscription(new Action<EventMessage>(demo.OnMessage));
                demo = null;
                GC.Collect();
                GC.WaitForFullGCComplete();
            }
            object target;

            Assert.IsFalse(sub.IsAlive);
            Assert.IsFalse(sub.TryGetTarget(out target));
            Assert.IsNull(target);
        }

        [TestMethod]
        public void SubsReferencingSamemethodOfADeadTargetAreEqual()
        {
            Subscription sub1, sub2;
            {
                var demo = new Subscriber();
                sub1 = new Subscription(new Action<EventMessage>(demo.OnMessage));
                sub2 = new Subscription(new Action<EventMessage>(demo.OnMessage));
                demo = null;
                GC.Collect();
                GC.WaitForFullGCComplete();
            }

            Assert.IsFalse(sub1.IsAlive);
            Assert.IsFalse(sub2.IsAlive);
            checkSubscriptionEquality(sub1, sub2, true);
        }

        [TestMethod]
        public void SubsReferencingDifferentMethodsOfADeadTargetAreNotEqual()
        {
            Subscription sub1, sub2;
            {
                var demo = new Subscriber();
                sub1 = new Subscription(new Action<EventMessage>(demo.OnMessage));
                sub2 = new Subscription(new Action<EventMessage>(demo.OnMessage2));
                demo = null;
                GC.Collect();
                GC.WaitForFullGCComplete();
            }

            Assert.IsFalse(sub1.IsAlive);
            Assert.IsFalse(sub2.IsAlive);
            checkSubscriptionEquality(sub1, sub2, false);
        }

        [TestMethod]
        public void SubKeepsACorrectMethodTarget()
        {
            var action = new Action<EventMessage>(_ => { });
            var sub    = new Subscription(action);
            object target;

            Assert.IsTrue(sub.IsAlive);
            Assert.IsTrue(sub.TryGetTarget(out target));
            Assert.IsNotNull(target);
            Assert.AreEqual(action.Target, target);
        }


        private static void checkSubscriptionEquality(Subscription sub1, Subscription sub2, bool areEqual)
        {
            Assert.AreEqual(areEqual,    sub1 == sub2);
            Assert.AreNotEqual(areEqual, sub1 != sub2);
            Assert.AreEqual(areEqual, sub1.Equals(sub2));
            Assert.AreEqual(areEqual, ((object)sub1).Equals(sub2));
            Assert.AreEqual(areEqual, Equals(sub1, sub2));
            Assert.AreEqual(areEqual, EqualityComparer<Subscription>.Default.Equals(sub1, sub2));
        }
    }
}
