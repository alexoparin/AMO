using System.Diagnostics;
using AMO.Core.Infrastructure;

namespace AMO.Core.DI
{
    internal class ExternalFactory : IInstanceFactory
    {
        private readonly ResolveDelegate _factory;


        internal ExternalFactory(ResolveDelegate factory)
        {
            Debug.Assert(factory != null);

            _factory = factory;
        }


        public object CreateInstance(ref IDIResolveContext context)
        {
            return _factory(ref context);
        }
    }
}
