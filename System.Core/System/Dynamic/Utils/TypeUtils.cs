namespace System.Dynamic.Utils
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal static class TypeUtils
    {
        private static readonly Assembly _mscorlib = typeof(object).Assembly;
        private static readonly Assembly _systemCore = typeof(Expression).Assembly;
        private const BindingFlags AnyStatic = (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        internal const MethodAttributes PublicStatic = (MethodAttributes.Static | MethodAttributes.Public);

        internal static bool AreEquivalent(Type t1, Type t2)
        {
            if (!(t1 == t2))
            {
                return t1.IsEquivalentTo(t2);
            }
            return true;
        }

        internal static bool AreReferenceAssignable(Type dest, Type src)
        {
            return (AreEquivalent(dest, src) || ((!dest.IsValueType && !src.IsValueType) && dest.IsAssignableFrom(src)));
        }

        internal static bool CanCache(this Type t)
        {
            Assembly assembly = t.Assembly;
            if ((assembly != _mscorlib) && (assembly != _systemCore))
            {
                return false;
            }
            if (t.IsGenericType)
            {
                foreach (Type type in t.GetGenericArguments())
                {
                    if (!type.CanCache())
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal static MethodInfo FindConversionOperator(MethodInfo[] methods, Type typeFrom, Type typeTo, bool implicitOnly)
        {
            foreach (MethodInfo info in methods)
            {
                if (((info.Name == "op_Implicit") || (!implicitOnly && (info.Name == "op_Explicit"))) && (AreEquivalent(info.ReturnType, typeTo) && AreEquivalent(info.GetParametersCached()[0].ParameterType, typeFrom)))
                {
                    return info;
                }
            }
            return null;
        }

        internal static Type FindGenericType(Type definition, Type type)
        {
            while ((type != null) && (type != typeof(object)))
            {
                if (type.IsGenericType && AreEquivalent(type.GetGenericTypeDefinition(), definition))
                {
                    return type;
                }
                if (definition.IsInterface)
                {
                    foreach (Type type2 in type.GetInterfaces())
                    {
                        Type type3 = FindGenericType(definition, type2);
                        if (type3 != null)
                        {
                            return type3;
                        }
                    }
                }
                type = type.BaseType;
            }
            return null;
        }

        internal static MethodInfo GetBooleanOperator(Type type, string name)
        {
            do
            {
                MethodInfo info = type.GetMethodValidated(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, null, new Type[] { type }, null);
                if (((info != null) && info.IsSpecialName) && !info.ContainsGenericParameters)
                {
                    return info;
                }
                type = type.BaseType;
            }
            while (type != null);
            return null;
        }

        internal static Type GetNonNullableType(this Type type)
        {
            if (type.IsNullableType())
            {
                return type.GetGenericArguments()[0];
            }
            return type;
        }

        internal static Type GetNonRefType(this Type type)
        {
            if (!type.IsByRef)
            {
                return type;
            }
            return type.GetElementType();
        }

        internal static Type GetNullableType(Type type)
        {
            if (type.IsValueType && !type.IsNullableType())
            {
                return typeof(Nullable<>).MakeGenericType(new Type[] { type });
            }
            return type;
        }

        internal static MethodInfo GetUserDefinedCoercionMethod(Type convertFrom, Type convertToType, bool implicitOnly)
        {
            Type nonNullableType = convertFrom.GetNonNullableType();
            Type type2 = convertToType.GetNonNullableType();
            MethodInfo[] methods = nonNullableType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            MethodInfo info = FindConversionOperator(methods, convertFrom, convertToType, implicitOnly);
            if (info != null)
            {
                return info;
            }
            MethodInfo[] infoArray2 = type2.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            info = FindConversionOperator(infoArray2, convertFrom, convertToType, implicitOnly);
            if (info != null)
            {
                return info;
            }
            if (!AreEquivalent(nonNullableType, convertFrom) || !AreEquivalent(type2, convertToType))
            {
                info = FindConversionOperator(methods, nonNullableType, type2, implicitOnly);
                if (info == null)
                {
                    info = FindConversionOperator(infoArray2, nonNullableType, type2, implicitOnly);
                }
                if (info != null)
                {
                    return info;
                }
            }
            return null;
        }

        internal static bool HasBuiltInEqualityOperator(Type left, Type right)
        {
            if (!left.IsInterface || right.IsValueType)
            {
                if (right.IsInterface && !left.IsValueType)
                {
                    return true;
                }
                if ((!left.IsValueType && !right.IsValueType) && (AreReferenceAssignable(left, right) || AreReferenceAssignable(right, left)))
                {
                    return true;
                }
                if (!AreEquivalent(left, right))
                {
                    return false;
                }
                Type nonNullableType = left.GetNonNullableType();
                if ((!(nonNullableType == typeof(bool)) && !IsNumeric(nonNullableType)) && !nonNullableType.IsEnum)
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool HasIdentityPrimitiveOrNullableConversion(Type source, Type dest)
        {
            return (AreEquivalent(source, dest) || ((source.IsNullableType() && AreEquivalent(dest, source.GetNonNullableType())) || ((dest.IsNullableType() && AreEquivalent(source, dest.GetNonNullableType())) || ((IsConvertible(source) && IsConvertible(dest)) && (dest.GetNonNullableType() != typeof(bool))))));
        }

        internal static bool HasReferenceConversion(Type source, Type dest)
        {
            if ((source == typeof(void)) || (dest == typeof(void)))
            {
                return false;
            }
            Type nonNullableType = source.GetNonNullableType();
            Type c = dest.GetNonNullableType();
            if (!nonNullableType.IsAssignableFrom(c))
            {
                if (c.IsAssignableFrom(nonNullableType))
                {
                    return true;
                }
                if (source.IsInterface || dest.IsInterface)
                {
                    return true;
                }
                if (!IsLegalExplicitVariantDelegateConversion(source, dest) && (!(source == typeof(object)) && !(dest == typeof(object))))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool HasReferenceEquality(Type left, Type right)
        {
            if (left.IsValueType || right.IsValueType)
            {
                return false;
            }
            if ((!left.IsInterface && !right.IsInterface) && !AreReferenceAssignable(left, right))
            {
                return AreReferenceAssignable(right, left);
            }
            return true;
        }

        internal static bool IsArithmetic(Type type)
        {
            type = type.GetNonNullableType();
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        return true;
                }
            }
            return false;
        }

        internal static bool IsBool(Type type)
        {
            return (type.GetNonNullableType() == typeof(bool));
        }

        private static bool IsContravariant(Type t)
        {
            return (GenericParameterAttributes.None != (t.GenericParameterAttributes & GenericParameterAttributes.Contravariant));
        }

        internal static bool IsConvertible(Type type)
        {
            type = type.GetNonNullableType();
            if (type.IsEnum)
            {
                return true;
            }
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;
            }
            return false;
        }

        private static bool IsCovariant(Type t)
        {
            return (GenericParameterAttributes.None != (t.GenericParameterAttributes & GenericParameterAttributes.Covariant));
        }

        private static bool IsDelegate(Type t)
        {
            return t.IsSubclassOf(typeof(Delegate));
        }

        internal static bool IsFloatingPoint(Type type)
        {
            type = type.GetNonNullableType();
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;
            }
            return false;
        }

        private static bool IsImplicitBoxingConversion(Type source, Type destination)
        {
            return ((source.IsValueType && ((destination == typeof(object)) || (destination == typeof(ValueType)))) || (source.IsEnum && (destination == typeof(Enum))));
        }

        internal static bool IsImplicitlyConvertible(Type source, Type destination)
        {
            if ((!AreEquivalent(source, destination) && !IsImplicitNumericConversion(source, destination)) && (!IsImplicitReferenceConversion(source, destination) && !IsImplicitBoxingConversion(source, destination)))
            {
                return IsImplicitNullableConversion(source, destination);
            }
            return true;
        }

        private static bool IsImplicitNullableConversion(Type source, Type destination)
        {
            return (destination.IsNullableType() && IsImplicitlyConvertible(source.GetNonNullableType(), destination.GetNonNullableType()));
        }

        private static bool IsImplicitNumericConversion(Type source, Type destination)
        {
            TypeCode typeCode = Type.GetTypeCode(source);
            TypeCode code2 = Type.GetTypeCode(destination);
            switch (typeCode)
            {
                case TypeCode.Char:
                    switch (code2)
                    {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.SByte:
                    switch (code2)
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;

                case TypeCode.Byte:
                    switch (code2)
                    {
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Int16:
                    switch (code2)
                    {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.UInt16:
                    switch (code2)
                    {
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Int32:
                    switch (code2)
                    {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.UInt32:
                    switch (code2)
                    {
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    switch (code2)
                    {
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Single:
                    return (code2 == TypeCode.Double);

                default:
                    return false;
            }
            return false;
        }

        private static bool IsImplicitReferenceConversion(Type source, Type destination)
        {
            return destination.IsAssignableFrom(source);
        }

        internal static bool IsInteger(Type type)
        {
            type = type.GetNonNullableType();
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        return true;
                }
            }
            return false;
        }

        internal static bool IsIntegerOrBool(Type type)
        {
            type = type.GetNonNullableType();
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        return true;
                }
            }
            return false;
        }

        private static bool IsInvariant(Type t)
        {
            return (GenericParameterAttributes.None == (t.GenericParameterAttributes & GenericParameterAttributes.VarianceMask));
        }

        internal static bool IsLegalExplicitVariantDelegateConversion(Type source, Type dest)
        {
            if ((!IsDelegate(source) || !IsDelegate(dest)) || (!source.IsGenericType || !dest.IsGenericType))
            {
                return false;
            }
            Type genericTypeDefinition = source.GetGenericTypeDefinition();
            if (dest.GetGenericTypeDefinition() != genericTypeDefinition)
            {
                return false;
            }
            Type[] genericArguments = genericTypeDefinition.GetGenericArguments();
            Type[] typeArray2 = source.GetGenericArguments();
            Type[] typeArray3 = dest.GetGenericArguments();
            for (int i = 0; i < genericArguments.Length; i++)
            {
                Type type2 = typeArray2[i];
                Type type3 = typeArray3[i];
                if (!AreEquivalent(type2, type3))
                {
                    Type t = genericArguments[i];
                    if (IsInvariant(t))
                    {
                        return false;
                    }
                    if (IsCovariant(t))
                    {
                        if (!HasReferenceConversion(type2, type3))
                        {
                            return false;
                        }
                    }
                    else if (IsContravariant(t) && (type2.IsValueType || type3.IsValueType))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal static bool IsNullableType(this Type type)
        {
            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        internal static bool IsNumeric(Type type)
        {
            type = type.GetNonNullableType();
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Char:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        return true;
                }
            }
            return false;
        }

        internal static bool IsSameOrSubclass(Type type, Type subType)
        {
            if (!AreEquivalent(type, subType))
            {
                return subType.IsSubclassOf(type);
            }
            return true;
        }

        internal static bool IsUnsigned(Type type)
        {
            type = type.GetNonNullableType();
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
            }
            return false;
        }

        internal static bool IsUnsignedInt(Type type)
        {
            type = type.GetNonNullableType();
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        return true;
                }
            }
            return false;
        }

        internal static bool IsValidInstanceType(MemberInfo member, Type instanceType)
        {
            Type declaringType = member.DeclaringType;
            if (AreReferenceAssignable(declaringType, instanceType))
            {
                return true;
            }
            if (instanceType.IsValueType)
            {
                if (AreReferenceAssignable(declaringType, typeof(object)))
                {
                    return true;
                }
                if (AreReferenceAssignable(declaringType, typeof(ValueType)))
                {
                    return true;
                }
                if (instanceType.IsEnum && AreReferenceAssignable(declaringType, typeof(Enum)))
                {
                    return true;
                }
                if (declaringType.IsInterface)
                {
                    foreach (Type type2 in instanceType.GetInterfaces())
                    {
                        if (AreReferenceAssignable(declaringType, type2))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        internal static void ValidateType(Type type)
        {
            if (type.IsGenericTypeDefinition)
            {
                throw Error.TypeIsGeneric(type);
            }
            if (type.ContainsGenericParameters)
            {
                throw Error.TypeContainsGenericParameters(type);
            }
        }
    }
}

