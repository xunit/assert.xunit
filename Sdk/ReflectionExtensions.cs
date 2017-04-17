using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Xunit.Sdk
{
    static class ReflectionExtensions
    {
        static readonly TypeInfo NullableTypeInfo = typeof(Nullable<>).GetTypeInfo();

        public static TResult Invoke<TResult>(this MethodBase method, object @this, params object[] parameters)
        {
            return (TResult)method.Invoke(@this, parameters);
        }

        public static bool IsAssignableFrom(this Type type, Type otherType)
        {
            return type.GetTypeInfo().IsAssignableFrom(otherType.GetTypeInfo());
        }

        public static bool IsReferenceOrNullableType(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return !typeInfo.IsValueType ||
                (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition().GetTypeInfo().IsAssignableFrom(NullableTypeInfo));
        }

        public static bool IsSet(this Type type)
        {
            var implementedInterfaces = type.GetTypeInfo().ImplementedInterfaces.Select(i => i.GetTypeInfo());
            var genericInterfaces = implementedInterfaces.Where(ti => ti.IsGenericType);
            return genericInterfaces.Any(ti => ti.GetGenericTypeDefinition() == typeof(ISet<>));
        }

        public static Type MakeComparableType(this Type comparandType)
        {
            return typeof(IComparable<>).MakeGenericType(comparandType);
        }

        public static Type MakeEquatableType(this Type comparandType)
        {
            return typeof(IEquatable<>).MakeGenericType(comparandType);
        }
    }
}