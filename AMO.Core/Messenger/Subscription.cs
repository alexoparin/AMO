using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace AMO.Core.Messenger
{
    /// <summary>
    /// Represents a subscription by keeping a handling method and a weak reference to its target object in case it exists
    /// </summary>
    internal struct Subscription : IEquatable<Subscription>
    {
        private readonly int           _hashCode;
        private readonly WeakReference _weakReference;

        internal bool       IsAlive { get { return Method != null && (_weakReference == null || _weakReference.IsAlive); } }
        internal MethodInfo Method  { get; private set; }


        internal Subscription(Delegate messageHandler)
        {
            Debug.Assert(messageHandler != null);
            
            _weakReference = messageHandler.Target != null ? new WeakReference(messageHandler.Target) : null;
            Method = messageHandler.Method;
            _hashCode = -740764636 ^ (messageHandler.Target != null ? messageHandler.Target.GetHashCode() : Method.GetHashCode());
        }

        /// <summary> Tries to get the object which owns the <see cref="Method"/> or null if <see cref="Method"/> is static. </summary>
        /// <param name="target">Method owner</param>
        /// <returns>true in the case of object is not deleted by garbage collector or <see cref="Method"/> is static, false otherwise</returns>
        internal bool TryGetTarget(out object target)
        {
            target = _weakReference != null ? _weakReference.Target : null;
            return IsAlive;
        }

        public bool Equals(Subscription other)
        {
            object target, otherTarget;

            return EqualityComparer<MethodInfo>.Default.Equals(Method, other.Method) &&
                TryGetTarget(out target) == other.TryGetTarget(out otherTarget) &&
                Equals(target, otherTarget);
        }

        public override bool Equals(object obj)
        {
            return obj is Subscription && Equals((Subscription)obj);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public static bool operator ==(Subscription sub1, Subscription sub2)
        {
            return sub1.Equals(sub2);
        }

        public static bool operator !=(Subscription sub1, Subscription sub2)
        {
            return !(sub1 == sub2);
        }
    }
}
