using System.Diagnostics;
using AMO.Core.Infrastructure;

namespace AMO.Core.DI
{
    internal class ExternalLifetimeManager : ILifetimeManager
    {
        private readonly IInstanceFactory _instanceFactory;


        internal ExternalLifetimeManager(IInstanceFactory instanceFactory)
        {
            Debug.Assert(instanceFactory != null);

            _instanceFactory = instanceFactory;
        }


        public object GetInstance(ref IDIResolveContext context)
        {
            return _instanceFactory.CreateInstance(ref context);
        }
    }
}
