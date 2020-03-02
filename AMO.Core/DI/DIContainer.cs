using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using AMO.Core.Infrastructure;

namespace AMO.Core.DI
{
    public class DIContainer : IDIContainer
    {
        private readonly Dictionary<Type, ILifetimeManager> _registry = new Dictionary<Type, ILifetimeManager>();

        [ThreadStatic]
        private static IDIResolveContext _resolveContext = ResolveContext.None;

        private volatile bool _isDisposed;
        private int _concurrentUsageCount;

        public IDIRegistry RegisterType(Type interfaceType, Type implementationType, LifetimeManagement lifetime)
        {
            if (_isDisposed)                { throw new ObjectDisposedException(typeof(DIContainer).Name); }
            if (implementationType == null) { throw new ArgumentNullException("implementationType"); }

            interfaceType = interfaceType ?? implementationType;
            if (!interfaceType.IsAssignableFrom(implementationType))
            {
                throw new ArgumentException(string.Format(
                    "The type being registered is {0} which is not assignable to " +
                    "provided interface type {1}", implementationType.FullName, interfaceType.FullName),
                    "instance");
            }
            if (!Enum.IsDefined(typeof(LifetimeManagement), lifetime) || lifetime == LifetimeManagement.External)
            {
                throw new ArgumentException(string.Format("LifetimeManagement value {0} is not applicable to type registrations", lifetime), "lifetime");
            }

            Interlocked.Increment(ref _concurrentUsageCount);
            try
            {
                _registry[interfaceType] = createLifetimeManager(lifetime, new TypeFactory(implementationType));
            }
            finally
            {
                Interlocked.Decrement(ref _concurrentUsageCount);
            }

            return this;
        }

        public IDIRegistry RegisterInstance(Type interfaceType, object instance)
        {
            if (_isDisposed) { throw new ObjectDisposedException(typeof(DIContainer).Name); }
            if (interfaceType == null) { throw new ArgumentNullException("interfaceType"); }
            if (instance != null && !interfaceType.IsAssignableFrom(instance.GetType()))
            {
                throw new ArgumentException(string.Format(
                    "The instance being registered is of type {0} which is not assignable to " +
                    "provided interface type {1}", instance.GetType().FullName, interfaceType.FullName),
                    "instance");
            }
            
            Interlocked.Increment(ref _concurrentUsageCount);
            _registry[interfaceType] = createLifetimeManager(LifetimeManagement.Singleton, new ExternalInstance(instance));
            Interlocked.Decrement(ref _concurrentUsageCount);
            return this;
        }

        public IDIRegistry RegisterFactory(Type interfaceType, ResolveDelegate factory)
        {
            if (_isDisposed)           { throw new ObjectDisposedException(typeof(DIContainer).Name); }
            if (interfaceType == null) { throw new ArgumentNullException("interfaceType"); }
            if (factory == null)       { throw new ArgumentNullException("factory"); }

            Interlocked.Increment(ref _concurrentUsageCount);
            _registry[interfaceType] = createLifetimeManager(LifetimeManagement.External, new ExternalFactory(factory));
            Interlocked.Decrement(ref _concurrentUsageCount);
            return this;
        }

        [System.Security.SecuritySafeCritical]
        public object Resolve(Type type)
        {
            if (type == null) { throw new ArgumentNullException("type"); }

            return resolveInternal(type);
        }

        private int lastHash;
        private IDIResolveContext lastMgr = ResolveContext.None;
        private TypeFactory factory;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        [System.Security.SecuritySafeCritical]
        private object resolveInternal(Type type)
        {
            //Debug.Assert(type != null);
            

            ILifetimeManager manager;
            if (lastHash == type.GetHashCode())
            {
                return factory.CreateInstance(ref lastMgr);
            }
            else if (!_registry.TryGetValue(type, out manager))
            {
                throw new Exception(string.Format("Type {0} is not registered", type.FullName));
            }
            else
            {
                lastHash = type.GetHashCode();
                factory = new TypeFactory(type);
            }

            //_resolveContext = new ResolveContext(type, this, _resolveContext);
            var instance    = manager.GetInstance(ref lastMgr);
            //_resolveContext = _resolveContext.OuterContext;
            return instance;
        }

        private static ILifetimeManager createLifetimeManager(LifetimeManagement lifetime, IInstanceFactory instanceFactory)
        {
            Debug.Assert(instanceFactory != null);

            switch (lifetime)
            {
                case LifetimeManagement.Transient: return new TransientLifetimeManager(instanceFactory);
                case LifetimeManagement.Singleton: return new SingletonLifetimeManager(instanceFactory);
                case LifetimeManagement.External:  return new ExternalLifetimeManager(instanceFactory);
                default:
                    throw new ArgumentException("Unsupported lifetime value", "lifetime");
            }
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;

            if (disposing)
            {
                Interlocked.Decrement(ref _concurrentUsageCount);
                SpinWait.SpinUntil(() => _concurrentUsageCount > -1);

                foreach (IDisposable manager in _registry.Values)
                {
                    manager.Dispose();
                }
                _registry.Clear();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
