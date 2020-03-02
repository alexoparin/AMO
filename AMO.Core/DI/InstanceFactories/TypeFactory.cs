using System;
using System.Diagnostics;
using AMO.Core.Infrastructure;

namespace AMO.Core.DI
{
    internal class TypeFactory : IInstanceFactory
    {
        private readonly Type   _type;
        private TypeResolveInfo _resolveInfo;


        internal TypeFactory(Type type)
        {
            Debug.Assert(type != null);

            _type = type;
        }


        public object CreateInstance(ref IDIResolveContext context)
        {
            if (_resolveInfo == null)
            {
                lock (this)
                {
                    if (_resolveInfo == null)
                    {
                        _resolveInfo = TypeResolveInfo.Create(_type);
                    }
                }
            }

            //var ctorArgs = new object[_resolveInfo.ConstructorParams.Length];
            //for (int i = 0; i < ctorArgs.Length; ++i)
            //{
            //    ctorArgs[i] = context.Resolver.Resolve(_resolveInfo.ConstructorParams[i].ParameterType);
            //}
            //return _resolveInfo.Factory.DynamicInvoke(ctorArgs);

            return _resolveInfo.Factory(ref context);
        }
    }
}
