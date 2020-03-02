using System;
using System.Diagnostics;
using AMO.Core.Infrastructure;

namespace AMO.Core.DI
{
    internal struct ResolveContext : IDIResolveContext
    {
        internal static readonly ResolveContext None = default(ResolveContext);
        
        public Type              Type         { get; private set; }
        public IDIResolver       Resolver     { get; private set; }
        public IDIResolveContext OuterContext { get; private set; }

        internal ResolveContext(Type type, IDIResolver resolver, IDIResolveContext outerContext)
        {
            Debug.Assert(type != null);
            Debug.Assert(resolver != null);

            Type         = type;
            Resolver     = resolver;
            OuterContext = outerContext;
        }


        public object Resolve()
        {
            return Resolver.Resolve(Type);
        }
    }
}
