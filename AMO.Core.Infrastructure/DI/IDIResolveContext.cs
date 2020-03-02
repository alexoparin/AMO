using System;

namespace AMO.Core.Infrastructure
{
    public interface IDIResolveContext
    {
        Type              Type         { get; }
        IDIResolver       Resolver     { get; }
        IDIResolveContext OuterContext { get; }
    }
}
