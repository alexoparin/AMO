using AMO.Core.Infrastructure;

namespace AMO.Core.DI
{
    internal interface IInstanceFactory
    {
        object CreateInstance(ref IDIResolveContext context);
    }
}