using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AMO.Core.Infrastructure;

namespace AMO.Core.DI
{
    
    internal class TypeResolveInfo
    {
        internal ConstructorInfo Constructor       { get; private set; }
        internal ParameterInfo[] ConstructorParams { get; private set; }
        internal ResolveDelegate Factory           { get; private set; }


        private TypeResolveInfo()
        {
        }


        internal static TypeResolveInfo Create(Type type)
        {
            var info = new TypeResolveInfo();

            foreach(var ctor in type.GetConstructors())
            {
                var currentCtorParams = ctor.GetParameters();

                if (info.Constructor == null ||
                    info.ConstructorParams.Length < currentCtorParams.Length)
                {
                    info.Constructor = ctor;
                    info.ConstructorParams  = currentCtorParams;
                }
            }

            if (info.Constructor == null)
            {
                throw new Exception(string.Format("{0} doesn't have public constructors", type.FullName));
            }

            info.Factory = createFactory(info.Constructor, info.ConstructorParams);

            return info;
        }

        private static ResolveDelegate createFactory(ConstructorInfo constructor, ParameterInfo[] constructorParams)
        {
            var prmsExpr = constructorParams
                .Select(p => Expression.Parameter(p.ParameterType, p.Name))
                .ToArray();

            foreach (var expr in prmsExpr)
            {

            }
            var ctorExpr = Expression.New(constructor, prmsExpr);
            //return Expression.Lambda(ctorExpr, prmsExpr).Compile();
            
            return Expression.Lambda<ResolveDelegate>(ctorExpr).Compile();
        }
    }
}
