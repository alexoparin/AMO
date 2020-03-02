namespace AMO.Core.Infrastructure
{
    public static class RegistryExtensions
    {
        public const LifetimeManagement DEFAULT_LIFETIME = LifetimeManagement.Transient;

        public static IDIRegistry Register<T>(this IDIRegistry registry)
        {
            return registry.RegisterType(typeof(T), typeof(T), DEFAULT_LIFETIME);
        }

        public static IDIRegistry Register<TIntf, TImpl>(this IDIRegistry registry)
        {
            return registry.RegisterType(typeof(TIntf), typeof(TImpl), DEFAULT_LIFETIME);
        }

        public static IDIRegistry Register<TIntf, TImpl>(this IDIRegistry registry, LifetimeManagement lifetime)
        {
            return registry.RegisterType(typeof(TIntf), typeof(TImpl), lifetime);
        }

        public static IDIRegistry RegisterSingleton<T>(this IDIRegistry registry)
        {
            return registry.RegisterType(typeof(T), typeof(T), LifetimeManagement.Singleton);
        }

        public static IDIRegistry RegisterSingleton<TIntf, TImpl>(this IDIRegistry registry)
        {
            return registry.RegisterType(typeof(TIntf), typeof(TImpl), LifetimeManagement.Singleton);
        }
    }
}
