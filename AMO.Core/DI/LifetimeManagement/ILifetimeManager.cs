using AMO.Core.Infrastructure;

namespace AMO.Core.DI
{
    internal interface ILifetimeManager
    {
        object GetInstance(ref IDIResolveContext context);
    }
}
