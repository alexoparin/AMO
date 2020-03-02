using System;
using System.Diagnostics;
using AMO.Core.Infrastructure;

namespace AMO.Core.DI
{
    internal class SingletonLifetimeManager : ILifetimeManager, IDisposable
    {
        private readonly IInstanceFactory _instanceFactory;
        private bool   _isDisposed;
        private object _instance;


        internal SingletonLifetimeManager(IInstanceFactory instanceFactory)
        {
            Debug.Assert(instanceFactory != null);

            _instanceFactory = instanceFactory;
        }


        public object GetInstance(ref IDIResolveContext context)
        {
            if (_isDisposed) { throw new ObjectDisposedException(typeof(SingletonLifetimeManager).Name); }

            if (_instance == null)
            {
                lock (this)
                {
                    if (_instance == null)
                    {
                        if (_isDisposed) { throw new ObjectDisposedException(typeof(SingletonLifetimeManager).Name); }
                        _instance = _instanceFactory.CreateInstance(ref context);
                    }
                }
            }
            return _instance;
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            lock (this)
            {
                IDisposable instance;
                if (disposing && (instance = _instance as IDisposable) != null)
                {
                    instance.Dispose();
                }
                _isDisposed = true;
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
