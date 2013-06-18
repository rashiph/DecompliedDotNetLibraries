namespace System.Xaml.Schema
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Xaml;

    internal static class CollectionReflector
    {
        private static MethodInfo s_dictionaryAddMethod;
        private static MethodInfo s_getEnumeratorMethod;
        private static MethodInfo s_listAddMethod;
        private static Type[] s_typeOfObjectArray;
        private static Type[] s_typeOfTwoObjectArray;

        internal static MethodInfo GetAddMethod(Type type, Type contentType)
        {
            return GetMethod(type, "Add", new Type[] { contentType });
        }

        private static MethodInfo GetAddMethod(Type type, int paramCount, out bool hasMoreThanOne)
        {
            MethodInfo info = null;
            MemberInfo[] infoArray = type.GetMember("Add", MemberTypes.Method, GetBindingFlags(type));
            if (infoArray != null)
            {
                foreach (MemberInfo info2 in infoArray)
                {
                    MethodInfo method = (MethodInfo) info2;
                    if (TypeReflector.IsPublicOrInternal(method))
                    {
                        ParameterInfo[] parameters = method.GetParameters();
                        if ((parameters != null) && (parameters.Length == paramCount))
                        {
                            if (info != null)
                            {
                                hasMoreThanOne = true;
                                return null;
                            }
                            info = method;
                        }
                    }
                }
            }
            hasMoreThanOne = false;
            return info;
        }

        private static BindingFlags GetBindingFlags(Type type)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            if (!type.IsVisible)
            {
                flags |= BindingFlags.NonPublic;
            }
            return flags;
        }

        internal static MethodInfo GetEnumeratorMethod(Type type)
        {
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return IEnumerableGetEnumeratorMethod;
            }
            return LookupEnumeratorMethod(type);
        }

        private static Type GetGenericInterface(Type type, Type interfaceType, out bool hasMultiple)
        {
            Type type2 = null;
            hasMultiple = false;
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == interfaceType))
            {
                return type;
            }
            foreach (Type type3 in type.GetInterfaces())
            {
                if (type3.IsGenericType && (type3.GetGenericTypeDefinition() == interfaceType))
                {
                    if (type2 != null)
                    {
                        hasMultiple = true;
                        return null;
                    }
                    type2 = type3;
                }
            }
            return type2;
        }

        internal static MethodInfo GetIsReadOnlyMethod(Type collectionType, Type itemType)
        {
            Type type = typeof(ICollection<>).MakeGenericType(new Type[] { itemType });
            if (type.IsAssignableFrom(collectionType))
            {
                return type.GetProperty("IsReadOnly").GetGetMethod();
            }
            return null;
        }

        private static MethodInfo GetMethod(Type type, string name, Type[] argTypes)
        {
            MethodInfo method = type.GetMethod(name, GetBindingFlags(type), null, argTypes, null);
            if ((method != null) && !TypeReflector.IsPublicOrInternal(method))
            {
                method = null;
            }
            return method;
        }

        private static MethodInfo GetPublicMethod(Type type, string name, int argCount)
        {
            foreach (MemberInfo info in type.GetMember(name, MemberTypes.Method, BindingFlags.Public | BindingFlags.Instance))
            {
                MethodInfo info2 = (MethodInfo) info;
                if (info2.GetParameters().Length == argCount)
                {
                    return info2;
                }
            }
            return null;
        }

        internal static MethodInfo LookupAddMethod(Type type, XamlCollectionKind collectionKind)
        {
            MethodInfo addMethod = null;
            switch (collectionKind)
            {
                case XamlCollectionKind.Collection:
                    if (TryGetCollectionAdder(type, true, out addMethod) && (addMethod == null))
                    {
                        throw new XamlSchemaException(System.Xaml.SR.Get("AmbiguousCollectionItemType", new object[] { type }));
                    }
                    return addMethod;

                case XamlCollectionKind.Dictionary:
                    if (TryGetDictionaryAdder(type, true, out addMethod) && (addMethod == null))
                    {
                        throw new XamlSchemaException(System.Xaml.SR.Get("AmbiguousDictionaryItemType", new object[] { type }));
                    }
                    return addMethod;
            }
            return addMethod;
        }

        internal static XamlCollectionKind LookupCollectionKind(Type type, out MethodInfo addMethod)
        {
            addMethod = null;
            if (type.IsArray)
            {
                return XamlCollectionKind.Array;
            }
            if (typeof(IEnumerable).IsAssignableFrom(type) || (LookupEnumeratorMethod(type) != null))
            {
                if (typeof(IDictionary).IsAssignableFrom(type))
                {
                    return XamlCollectionKind.Dictionary;
                }
                if (TryGetIDictionaryAdder(type, out addMethod))
                {
                    return XamlCollectionKind.Dictionary;
                }
                if (typeof(IList).IsAssignableFrom(type))
                {
                    return XamlCollectionKind.Collection;
                }
                if (TryGetICollectionAdder(type, out addMethod))
                {
                    return XamlCollectionKind.Collection;
                }
                if (TryGetDictionaryAdder(type, false, out addMethod))
                {
                    return XamlCollectionKind.Dictionary;
                }
                if (TryGetCollectionAdder(type, false, out addMethod))
                {
                    return XamlCollectionKind.Collection;
                }
            }
            return XamlCollectionKind.None;
        }

        private static MethodInfo LookupEnumeratorMethod(Type type)
        {
            MethodInfo info = GetMethod(type, "GetEnumerator", Type.EmptyTypes);
            if ((info != null) && !typeof(IEnumerator).IsAssignableFrom(info.ReturnType))
            {
                info = null;
            }
            return info;
        }

        private static bool TryGetCollectionAdder(Type type, bool mayBeICollection, out MethodInfo addMethod)
        {
            bool flag = false;
            if (mayBeICollection && TryGetICollectionAdder(type, out addMethod))
            {
                if (addMethod != null)
                {
                    return true;
                }
                flag = true;
            }
            bool hasMoreThanOne = false;
            addMethod = GetAddMethod(type, 1, out hasMoreThanOne);
            if ((addMethod == null) && typeof(IList).IsAssignableFrom(type))
            {
                addMethod = IListAddMethod;
            }
            if (addMethod == null)
            {
                if (!hasMoreThanOne && !flag)
                {
                    return false;
                }
                addMethod = GetMethod(type, "Add", TypeOfObjectArray);
            }
            return true;
        }

        private static bool TryGetDictionaryAdder(Type type, bool mayBeIDictionary, out MethodInfo addMethod)
        {
            bool flag = false;
            if (mayBeIDictionary && TryGetIDictionaryAdder(type, out addMethod))
            {
                if (addMethod != null)
                {
                    return true;
                }
                flag = true;
            }
            bool hasMoreThanOne = false;
            addMethod = GetAddMethod(type, 2, out hasMoreThanOne);
            if ((addMethod == null) && typeof(IDictionary).IsAssignableFrom(type))
            {
                addMethod = IDictionaryAddMethod;
            }
            if (addMethod == null)
            {
                if (!hasMoreThanOne && !flag)
                {
                    return false;
                }
                addMethod = GetMethod(type, "Add", TypeOfTwoObjectArray);
            }
            return true;
        }

        private static bool TryGetICollectionAdder(Type type, out MethodInfo addMethod)
        {
            bool hasMultiple = false;
            Type type2 = GetGenericInterface(type, typeof(ICollection<>), out hasMultiple);
            if (type2 != null)
            {
                addMethod = type2.GetMethod("Add");
                return true;
            }
            addMethod = null;
            return hasMultiple;
        }

        private static bool TryGetIDictionaryAdder(Type type, out MethodInfo addMethod)
        {
            bool hasMultiple = false;
            Type type2 = GetGenericInterface(type, typeof(IDictionary<,>), out hasMultiple);
            if (type2 != null)
            {
                addMethod = GetPublicMethod(type2, "Add", 2);
                return true;
            }
            addMethod = null;
            return hasMultiple;
        }

        private static MethodInfo IDictionaryAddMethod
        {
            get
            {
                if (s_dictionaryAddMethod == null)
                {
                    s_dictionaryAddMethod = typeof(IDictionary).GetMethod("Add");
                }
                return s_dictionaryAddMethod;
            }
        }

        private static MethodInfo IEnumerableGetEnumeratorMethod
        {
            get
            {
                if (s_getEnumeratorMethod == null)
                {
                    s_getEnumeratorMethod = typeof(IEnumerable).GetMethod("GetEnumerator");
                }
                return s_getEnumeratorMethod;
            }
        }

        private static MethodInfo IListAddMethod
        {
            get
            {
                if (s_listAddMethod == null)
                {
                    s_listAddMethod = typeof(IList).GetMethod("Add");
                }
                return s_listAddMethod;
            }
        }

        private static Type[] TypeOfObjectArray
        {
            get
            {
                if (s_typeOfObjectArray == null)
                {
                    s_typeOfObjectArray = new Type[] { typeof(object) };
                }
                return s_typeOfObjectArray;
            }
        }

        private static Type[] TypeOfTwoObjectArray
        {
            get
            {
                if (s_typeOfTwoObjectArray == null)
                {
                    s_typeOfTwoObjectArray = new Type[] { typeof(object), typeof(object) };
                }
                return s_typeOfTwoObjectArray;
            }
        }
    }
}

