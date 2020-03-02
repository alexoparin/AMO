using AMO.Core.Infrastructure;

namespace AMO.Core.DI
{
    internal class ExternalInstance : IInstanceFactory
    {
        private readonly object _instance;


        internal ExternalInstance(object instance)
        {
            _instance = instance;
        }


        public object CreateInstance(ref IDIResolveContext context)
        {
            return _instance;
        }
    }
}
