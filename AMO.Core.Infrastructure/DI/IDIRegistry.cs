using System;

namespace AMO.Core.Infrastructure
{
    public interface IDIRegistry
    {
        IDIRegistry RegisterType(Type interfaceType, Type implementationType, LifetimeManagement lifetime);
        IDIRegistry RegisterInstance(Type interfaceType, object instance);
        IDIRegistry RegisterFactory(Type interfaceType, ResolveDelegate resolve);
    }
}
