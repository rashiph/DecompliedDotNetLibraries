namespace System.Xaml
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;

    internal static class ReflectionHelper
    {
        private static Hashtable _loadedAssembliesHash = new Hashtable(8);

        internal static Assembly GetAlreadyLoadedAssembly(string assemblyNameLookup)
        {
            return (Assembly) _loadedAssembliesHash[assemblyNameLookup];
        }

        private static string GetCustomAttributeData(MemberInfo mi, Type attrType, out Type typeValue)
        {
            string str = GetCustomAttributeData(CustomAttributeData.GetCustomAttributes(mi), attrType, out typeValue, true, false);
            if (str != null)
            {
                return str;
            }
            return string.Empty;
        }

        internal static string GetCustomAttributeData(Type t, Type attrType, bool allowTypeAlso, ref bool attributeDataFound, out Type typeValue)
        {
            typeValue = null;
            attributeDataFound = false;
            Type target = t;
            string str = null;
            while ((target != null) && !attributeDataFound)
            {
                IList<CustomAttributeData> customAttributes = CustomAttributeData.GetCustomAttributes(target);
                for (int i = 0; (i < customAttributes.Count) && !attributeDataFound; i++)
                {
                    CustomAttributeData cad = customAttributes[i];
                    if (cad.Constructor.ReflectedType == attrType)
                    {
                        attributeDataFound = true;
                        str = GetCustomAttributeData(cad, attrType, out typeValue, allowTypeAlso, false, false);
                    }
                }
                if (!attributeDataFound)
                {
                    target = target.BaseType;
                }
            }
            return str;
        }

        private static string GetCustomAttributeData(IList<CustomAttributeData> list, Type attrType, out Type typeValue, bool allowTypeAlso, bool allowZeroArgs)
        {
            typeValue = null;
            string str = null;
            for (int i = 0; i < list.Count; i++)
            {
                str = GetCustomAttributeData(list[i], attrType, out typeValue, allowTypeAlso, false, allowZeroArgs);
                if (str != null)
                {
                    return str;
                }
            }
            return str;
        }

        private static string GetCustomAttributeData(CustomAttributeData cad, Type attrType, out Type typeValue, bool allowTypeAlso, bool noArgs, bool zeroArgsAllowed)
        {
            string assemblyQualifiedName = null;
            typeValue = null;
            if (!(cad.Constructor.ReflectedType == attrType))
            {
                return assemblyQualifiedName;
            }
            IList<CustomAttributeTypedArgument> constructorArguments = cad.ConstructorArguments;
            if ((constructorArguments.Count == 1) && !noArgs)
            {
                CustomAttributeTypedArgument argument = constructorArguments[0];
                assemblyQualifiedName = argument.Value as string;
                if (((assemblyQualifiedName == null) && allowTypeAlso) && (argument.ArgumentType == typeof(Type)))
                {
                    typeValue = argument.Value as Type;
                    assemblyQualifiedName = typeValue.AssemblyQualifiedName;
                }
                if (assemblyQualifiedName == null)
                {
                    throw new ArgumentException(System.Xaml.SR.Get("ParserAttributeArgsLow", new object[] { attrType.Name }));
                }
                return assemblyQualifiedName;
            }
            if (constructorArguments.Count != 0)
            {
                throw new ArgumentException(System.Xaml.SR.Get("ParserAttributeArgsHigh", new object[] { attrType.Name }));
            }
            if (!noArgs && !zeroArgsAllowed)
            {
                throw new ArgumentException(System.Xaml.SR.Get("ParserAttributeArgsLow", new object[] { attrType.Name }));
            }
            return string.Empty;
        }

        internal static Type GetQualifiedType(string typeName)
        {
            string[] strArray = typeName.Split(new char[] { ',' }, 2);
            Type type = null;
            if (strArray.Length == 1)
            {
                return Type.GetType(strArray[0]);
            }
            if (strArray.Length != 2)
            {
                throw new InvalidOperationException(System.Xaml.SR.Get("QualifiedNameHasWrongFormat", new object[] { typeName }));
            }
            Assembly assembly = null;
            try
            {
                assembly = LoadAssembly(strArray[1].TrimStart(new char[0]), null);
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                assembly = null;
            }
            if (assembly != null)
            {
                try
                {
                    type = assembly.GetType(strArray[0]);
                }
                catch (ArgumentException)
                {
                    assembly = null;
                }
                catch (SecurityException)
                {
                    assembly = null;
                }
            }
            return type;
        }

        internal static Type GetSystemType(Type type)
        {
            return type;
        }

        internal static string GetTypeConverterAttributeData(MemberInfo mi, out Type converterType)
        {
            return GetCustomAttributeData(mi, GetSystemType(typeof(TypeConverterAttribute)), out converterType);
        }

        internal static string GetTypeConverterAttributeData(Type type, out Type converterType)
        {
            bool attributeDataFound = false;
            return GetCustomAttributeData(type, GetSystemType(typeof(TypeConverterAttribute)), true, ref attributeDataFound, out converterType);
        }

        internal static bool IsInternalType(Type type)
        {
            Type type2 = type;
            while ((type.IsNestedAssembly || type.IsNestedFamORAssem) || ((type2 != type) && type.IsNestedPublic))
            {
                type = type.DeclaringType;
            }
            return (type.IsNotPublic || ((type2 != type) && type.IsPublic));
        }

        internal static bool IsNullableType(Type type)
        {
            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        internal static bool IsPublicType(Type type)
        {
            while (type.IsNestedPublic)
            {
                type = type.DeclaringType;
            }
            return type.IsPublic;
        }

        internal static Assembly LoadAssembly(string assemblyName, string assemblyPath)
        {
            return LoadAssemblyHelper(assemblyName, assemblyPath);
        }

        private static Assembly LoadAssemblyHelper(string assemblyGivenName, string assemblyPath)
        {
            AssemblyName reference = new AssemblyName(assemblyGivenName);
            string str = reference.Name.ToUpper(CultureInfo.InvariantCulture);
            Assembly loadedAssembly = (Assembly) _loadedAssembliesHash[str];
            if (loadedAssembly != null)
            {
                if (reference.Version != null)
                {
                    AssemblyName definition = new AssemblyName(loadedAssembly.FullName);
                    if (!AssemblyName.ReferenceMatchesDefinition(reference, definition))
                    {
                        string str2 = reference.ToString();
                        string str3 = definition.ToString();
                        throw new InvalidOperationException(System.Xaml.SR.Get("ParserAssemblyLoadVersionMismatch", new object[] { str2, str3 }));
                    }
                }
                return loadedAssembly;
            }
            if (string.IsNullOrEmpty(assemblyPath))
            {
                loadedAssembly = SafeSecurityHelper.GetLoadedAssembly(reference);
            }
            if (loadedAssembly == null)
            {
                if (!string.IsNullOrEmpty(assemblyPath))
                {
                    loadedAssembly = Assembly.LoadFile(assemblyPath);
                }
                else
                {
                    try
                    {
                        loadedAssembly = Assembly.Load(assemblyGivenName);
                    }
                    catch (FileNotFoundException)
                    {
                        loadedAssembly = null;
                    }
                }
            }
            if (loadedAssembly != null)
            {
                _loadedAssembliesHash[str] = loadedAssembly;
            }
            return loadedAssembly;
        }

        internal static void ResetCacheForAssembly(string assemblyName)
        {
            string str = assemblyName.ToUpper(CultureInfo.InvariantCulture);
            _loadedAssembliesHash[str] = null;
        }
    }
}

