using System;

namespace AMO.Core.Infrastructure
{
    public interface IDIResolver
    {
        object Resolve(Type type);
    }
}
