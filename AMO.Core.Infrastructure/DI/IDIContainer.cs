using System;

namespace AMO.Core.Infrastructure
{
    public interface IDIContainer : IDIRegistry, IDIResolver, IDisposable
    {
    }
}
