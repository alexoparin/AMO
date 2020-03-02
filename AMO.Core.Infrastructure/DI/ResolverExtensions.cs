namespace AMO.Core.Infrastructure
{
    public static class ResolverExtensions
    {
        public static T Resolve<T>(this IDIResolver resolver)
        {
            return (T)resolver.Resolve(typeof(T));
        }
    }
}
